using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace EquicordInstaller;

public partial class MainWindow : Window
{
    // ----- Cores do log -----
    private static readonly Brush Normal = Frozen(0xCD, 0xD0, 0xDA);
    private static readonly Brush Header = Frozen(0x78, 0xC8, 0xFF);
    private static readonly Brush Ok     = Frozen(0x78, 0xE6, 0x82);
    private static readonly Brush Err    = Frozen(0xFF, 0x6E, 0x6E);
    private static readonly Brush Info   = Frozen(0xEB, 0xCD, 0x78);

    private static readonly HttpClient Http = CreateHttp();

    private readonly string _documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    private readonly string _target;
    private readonly string _toolsDir;
    private readonly List<string> _extraPaths = new();

    public MainWindow()
    {
        InitializeComponent();

        _toolsDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Equicord Installer", "tools");

        _target = Path.Combine(_documents, "Equicord");

        LogBox.Document.PagePadding = new Thickness(4);

        AppendLog("Clique em \"Iniciar Instalacao\" para comecar.", Info);
        AppendLog("Se faltar, ele instala Git e Node.js pelos instaladores OFICIAIS (vai pedir permissao de admin).", Normal);
        AppendLog("Destino: " + _target, Normal);
    }

    private static HttpClient CreateHttp()
    {
        var h = new HttpClient { Timeout = TimeSpan.FromMinutes(15) };
        h.DefaultRequestHeaders.UserAgent.ParseAdd("EquicordInstaller/1.1");
        return h;
    }

    private static SolidColorBrush Frozen(byte r, byte g, byte b)
    {
        var brush = new SolidColorBrush(Color.FromRgb(r, g, b));
        brush.Freeze();
        return brush;
    }

    // ===================== UI helpers =====================
    private void AppendLog(string text, Brush color)
    {
        var p = new Paragraph(new Run(text))
        {
            Foreground = color,
            Margin = new Thickness(0),
            LineHeight = 17
        };
        LogBox.Document.Blocks.Add(p);
        LogBox.ScrollToEnd();
    }

    private void SetStatus(string text) => StatusText.Text = text;

    private void SetBusy(bool busy)
    {
        StartButton.IsEnabled = !busy;
        InjectCheck.IsEnabled = !busy;
        Progress.IsIndeterminate = busy;
        if (busy)
        {
            OpenFolderButton.IsEnabled = false;
            Progress.Value = 0;
        }
        else
        {
            Progress.IsIndeterminate = false;
            Progress.Value = 100;
        }
    }

    // ===================== PATH (ferramentas portateis) =====================
    private void AddPath(string dir)
    {
        if (!_extraPaths.Contains(dir)) _extraPaths.Add(dir);
    }

    private string ComposedPath()
    {
        var sys = Environment.GetEnvironmentVariable("PATH") ?? "";
        if (_extraPaths.Count == 0) return sys;
        return string.Join(';', _extraPaths) + ";" + sys;
    }

    // ===================== Lógica principal =====================
    private async void StartButton_Click(object sender, RoutedEventArgs e)
    {
        SetBusy(true);
        LogBox.Document.Blocks.Clear();
        try
        {
            AppendLog("=== Equicord Installer ===", Header);
            AppendLog("Destino: " + _target, Normal);

            // 1) Git (instala o Git oficial se nao tiver)
            if (!await EnsureGitAsync()) return;

            // 2) Node.js (instala o Node LTS oficial se nao tiver)
            if (!await EnsureNodeAsync()) return;

            // 3) pnpm (script oficial)
            if (!await EnsurePnpmAsync()) return;

            // 4) Clonar ou atualizar
            int code;
            var parent = Path.GetDirectoryName(_target)!;
            Directory.CreateDirectory(parent);
            if (Directory.Exists(Path.Combine(_target, ".git")))
            {
                // Atualizacao robusta: forca a versao mais recente. Arquivos nao rastreados
                // (ex.: src/userplugins) sao preservados; so descarta mudancas locais em arquivos do repo.
                AppendLog("[INFO] Forcando a atualizacao para a versao mais recente (seus userplugins sao preservados).", Info);
                code = await RunStepAsync("git fetch --all --prune && git reset --hard @{u}", _target, "Atualizando o Equicord");
            }
            else
            {
                code = await RunStepAsync($"git clone https://github.com/Equicord/Equicord.git \"{_target}\"", parent, "Clonando o Equicord do GitHub");
            }
            if (code != 0) { AppendLog($"[ERRO] Falha ao baixar/atualizar o repositorio (codigo {code}).", Err); return; }
            AppendLog("[OK] Repositorio pronto.", Ok);

            // 5) Dependencias
            code = await RunStepAsync("pnpm install --frozen-lockfile", _target, "Instalando dependencias");
            if (code != 0) { AppendLog($"[ERRO] Falha ao instalar dependencias (codigo {code}).", Err); return; }
            AppendLog("[OK] Dependencias instaladas.", Ok);

            // 6) Build
            code = await RunStepAsync("pnpm build", _target, "Build do Equicord");
            if (code != 0) { AppendLog($"[ERRO] Falha no build (codigo {code}).", Err); return; }
            AppendLog("[OK] Build concluido!", Ok);

            AppendLog("", Normal);
            AppendLog("============================================================", Ok);
            AppendLog(" CONCLUIDO! Equicord pronto em:", Ok);
            AppendLog(" " + _target, Ok);
            AppendLog("============================================================", Ok);
            SetStatus("Concluido com sucesso.");
            OpenFolderButton.IsEnabled = true;

            // 7) Inject opcional (janela separada, pois e interativo)
            if (InjectCheck.IsChecked == true)
            {
                AppendLog("", Normal);
                AppendLog("[INFO] Abrindo o instalador do Discord em uma nova janela.", Info);
                AppendLog("[INFO] Feche o Discord COMPLETAMENTE antes de continuar nessa janela.", Info);
                Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/k set \"PATH={ComposedPath()}\" && set COREPACK_ENABLE_DOWNLOAD_PROMPT=0 && cd /d \"{_target}\" && pnpm inject",
                    UseShellExecute = true
                });
            }
        }
        catch (Exception ex)
        {
            AppendLog("[ERRO] " + ex.Message, Err);
        }
        finally
        {
            SetBusy(false);
        }
    }

    // ===================== Garantir Git (instalador oficial) =====================
    private async Task<bool> EnsureGitAsync()
    {
        if (CommandExists("git")) { AppendLog("[OK] Git ja instalado.", Ok); return true; }

        // Talvez ja esteja instalado no local padrao, so fora do PATH deste processo.
        var gitCmd = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Git", "cmd");
        if (File.Exists(Path.Combine(gitCmd, "git.exe")))
        {
            AddPath(gitCmd);
            AppendLog("[OK] Git encontrado em " + gitCmd, Ok);
            return true;
        }

        try
        {
            AppendLog("", Normal);
            AppendLog(">>> Baixando o Git (instalador oficial)", Header);
            SetStatus("Procurando a versao mais recente do Git...");
            Directory.CreateDirectory(_toolsDir);

            var api = await Http.GetStringAsync("https://api.github.com/repos/git-for-windows/git/releases/latest");
            using var doc = JsonDocument.Parse(api);
            string? url = null;
            string? assetName = null;
            foreach (var asset in doc.RootElement.GetProperty("assets").EnumerateArray())
            {
                var name = asset.GetProperty("name").GetString() ?? "";
                // Ex.: "Git-2.54.0-64-bit.exe" (Setup oficial). "PortableGit-..." comeca diferente, fica de fora.
                if (name.StartsWith("Git-", StringComparison.OrdinalIgnoreCase)
                    && name.EndsWith("-64-bit.exe", StringComparison.OrdinalIgnoreCase))
                {
                    url = asset.GetProperty("browser_download_url").GetString();
                    assetName = name;
                    break;
                }
            }
            if (url == null) { AppendLog("[ERRO] Nao encontrei o instalador do Git.", Err); return false; }

            var exe = Path.Combine(_toolsDir, assetName!);
            await DownloadFileAsync(url, exe, "Baixando Git");

            // Instalacao silenciosa (Inno Setup) — precisa de admin (UAC).
            var code = await RunInstallerElevatedAsync(exe,
                "/VERYSILENT /NORESTART /SP- /NOCANCEL /CLOSEAPPLICATIONS",
                "Instalando o Git");
            try { File.Delete(exe); } catch { }

            if (code != 0) { AppendLog($"[ERRO] A instalacao do Git falhou (codigo {code}).", Err); return false; }

            if (!File.Exists(Path.Combine(gitCmd, "git.exe")) && !CommandExists("git"))
            {
                AppendLog("[ERRO] Git instalado, mas nao encontrei o git.exe.", Err); return false;
            }
            AddPath(gitCmd);
            AppendLog("[OK] Git instalado.", Ok);
            return true;
        }
        catch (Exception ex)
        {
            AppendLog("[ERRO] Nao consegui instalar o Git: " + ex.Message, Err);
            return false;
        }
    }

    // ===================== Garantir Node.js (instalador .msi oficial) =====================
    private async Task<bool> EnsureNodeAsync()
    {
        if (CommandExists("node")) { AppendLog("[OK] Node.js ja instalado.", Ok); return true; }

        // Talvez ja esteja instalado no local padrao, so fora do PATH deste processo.
        var nodeDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "nodejs");
        if (File.Exists(Path.Combine(nodeDir, "node.exe")))
        {
            AddPath(nodeDir);
            AppendLog("[OK] Node.js encontrado em " + nodeDir, Ok);
            return true;
        }

        try
        {
            AppendLog("", Normal);
            AppendLog(">>> Baixando o Node.js LTS (instalador .msi)", Header);
            SetStatus("Procurando a versao LTS do Node.js...");
            Directory.CreateDirectory(_toolsDir);

            var indexJson = await Http.GetStringAsync("https://nodejs.org/dist/index.json");
            using var doc = JsonDocument.Parse(indexJson);
            string? ver = null;
            foreach (var el in doc.RootElement.EnumerateArray())
            {
                if (el.GetProperty("lts").ValueKind != JsonValueKind.False)
                {
                    ver = el.GetProperty("version").GetString();
                    break;
                }
            }
            if (ver == null) { AppendLog("[ERRO] Nao encontrei uma versao LTS do Node.", Err); return false; }

            var url = $"https://nodejs.org/dist/{ver}/node-{ver}-x64.msi";
            var msi = Path.Combine(_toolsDir, $"node-{ver}-x64.msi");
            await DownloadFileAsync(url, msi, "Baixando Node.js " + ver);

            // Instalacao silenciosa via msiexec — precisa de admin (UAC).
            var code = await RunInstallerElevatedAsync("msiexec.exe",
                $"/i \"{msi}\" /qn /norestart",
                "Instalando o Node.js " + ver);
            try { File.Delete(msi); } catch { }

            if (code != 0 && code != 3010) // 3010 = sucesso, mas pede reinicio
            {
                AppendLog($"[ERRO] A instalacao do Node.js falhou (codigo {code}).", Err); return false;
            }

            if (!File.Exists(Path.Combine(nodeDir, "node.exe")) && !CommandExists("node"))
            {
                AppendLog("[ERRO] Node.js instalado, mas nao encontrei o node.exe.", Err); return false;
            }
            AddPath(nodeDir);
            AppendLog("[OK] Node.js " + ver + " instalado.", Ok);
            return true;
        }
        catch (Exception ex)
        {
            AppendLog("[ERRO] Nao consegui instalar o Node.js: " + ex.Message, Err);
            return false;
        }
    }

    // ===================== Garantir pnpm (script oficial) =====================
    private async Task<bool> EnsurePnpmAsync()
    {
        if (CommandExists("pnpm")) { AppendLog("[OK] pnpm encontrado.", Ok); return true; }

        // O script oficial instala em %LOCALAPPDATA%\pnpm — sem precisar de admin.
        var pnpmHome = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "pnpm");
        if (File.Exists(Path.Combine(pnpmHome, "pnpm.exe")))
        {
            AddPath(pnpmHome);
            AppendLog("[OK] pnpm encontrado (cache).", Ok);
            return true;
        }

        // Instalacao pelo script oficial: Invoke-WebRequest https://get.pnpm.io/install.ps1 | Invoke-Expression
        await RunStepAsync(
            "powershell -NoProfile -ExecutionPolicy Bypass -Command \"$ProgressPreference='SilentlyContinue'; Invoke-WebRequest https://get.pnpm.io/install.ps1 -UseBasicParsing | Invoke-Expression\"",
            _documents, "Instalando o pnpm (script oficial)");

        if (File.Exists(Path.Combine(pnpmHome, "pnpm.exe")))
        {
            AddPath(pnpmHome);
            AppendLog("[OK] pnpm instalado.", Ok);
            return true;
        }

        // Reserva: corepack (vem junto com o Node).
        await RunStepAsync("corepack enable", _documents, "Ativando pnpm (corepack)");
        if (CommandExists("pnpm")) { AppendLog("[OK] pnpm pronto.", Ok); return true; }

        AppendLog("[ERRO] Nao foi possivel preparar o pnpm.", Err);
        return false;
    }

    // ===================== Rodar instalador com permissao de admin (UAC) =====================
    // Para instaladores silenciosos (Git Setup, msiexec) que precisam escrever em Program Files.
    private async Task<int> RunInstallerElevatedAsync(string file, string args, string title)
    {
        AppendLog("", Normal);
        AppendLog(">>> " + title, Header);
        AppendLog("[INFO] O Windows vai pedir permissao de administrador (UAC). Clique em \"Sim\".", Info);
        SetStatus(title);
        Progress.IsIndeterminate = true;

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = file,
                Arguments = args,
                UseShellExecute = true,   // necessario para o verbo "runas" (UAC)
                Verb = "runas",
                WindowStyle = ProcessWindowStyle.Hidden
            };
            using var proc = Process.Start(psi);
            if (proc == null) { AppendLog("[ERRO] Nao consegui iniciar o instalador.", Err); return -1; }
            await proc.WaitForExitAsync();
            return proc.ExitCode;
        }
        catch (System.ComponentModel.Win32Exception ex) when (ex.NativeErrorCode == 1223)
        {
            AppendLog("[ERRO] Voce cancelou o pedido de administrador (UAC). Tente de novo e clique em \"Sim\".", Err);
            return -2;
        }
        catch (Exception ex)
        {
            AppendLog("[ERRO] " + ex.Message, Err);
            return -1;
        }
    }

    // ===================== Download com progresso =====================
    private async Task DownloadFileAsync(string url, string destPath, string label)
    {
        using var resp = await Http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        resp.EnsureSuccessStatusCode();
        long? total = resp.Content.Headers.ContentLength;

        await using var src = await resp.Content.ReadAsStreamAsync();
        await using var dst = new FileStream(destPath, FileMode.Create, FileAccess.Write, FileShare.None);

        var buffer = new byte[81920];
        long read = 0;
        int lastPct = -1;
        int n;
        Progress.IsIndeterminate = (total == null);
        while ((n = await src.ReadAsync(buffer.AsMemory(0, buffer.Length))) > 0)
        {
            await dst.WriteAsync(buffer.AsMemory(0, n));
            read += n;
            if (total is > 0)
            {
                int pct = (int)(read * 100 / total.Value);
                if (pct != lastPct)
                {
                    lastPct = pct;
                    Progress.Value = pct;
                    SetStatus($"{label}: {pct}%  ({read / 1048576} / {total.Value / 1048576} MB)");
                }
            }
            else
            {
                SetStatus($"{label}: {read / 1048576} MB");
            }
        }
    }

    // Executa um comando via cmd.exe e transmite a saida para o log em tempo real.
    private Task<int> RunStepAsync(string commandLine, string workingDir, string title)
    {
        AppendLog("", Normal);
        AppendLog(">>> " + title, Header);
        SetStatus(title);
        Progress.IsIndeterminate = true;

        var tcs = new TaskCompletionSource<int>();
        var psi = new ProcessStartInfo
        {
            FileName = Environment.GetEnvironmentVariable("ComSpec") ?? "cmd.exe",
            Arguments = "/c chcp 65001>nul & " + commandLine,
            WorkingDirectory = workingDir,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };
        psi.Environment["PATH"] = ComposedPath();
        psi.Environment["COREPACK_ENABLE_DOWNLOAD_PROMPT"] = "0"; // nao perguntar antes de baixar o pnpm

        var proc = new Process { StartInfo = psi, EnableRaisingEvents = true };

        void OnData(string? data)
        {
            if (data == null) return;
            Dispatcher.BeginInvoke(new Action(() => AppendLog(data, Normal)));
        }

        proc.OutputDataReceived += (_, ev) => OnData(ev.Data);
        proc.ErrorDataReceived  += (_, ev) => OnData(ev.Data);
        proc.Exited += (_, _) =>
        {
            int code = proc.ExitCode;
            proc.Dispose();
            tcs.TrySetResult(code);
        };

        try
        {
            proc.Start();
            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();
        }
        catch (Exception ex)
        {
            AppendLog("[ERRO] " + ex.Message, Err);
            tcs.TrySetResult(-1);
        }

        return tcs.Task;
    }

    private bool CommandExists(string name)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/c where " + name,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            psi.Environment["PATH"] = ComposedPath();
            using var p = Process.Start(psi);
            if (p == null) return false;
            p.WaitForExit();
            return p.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    // ===================== Barra de titulo =====================
    private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left) DragMove();
    }

    private void MinButton_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

    private void OpenFolderButton_Click(object sender, RoutedEventArgs e)
    {
        if (Directory.Exists(_target))
            Process.Start(new ProcessStartInfo { FileName = "explorer.exe", Arguments = $"\"{_target}\"", UseShellExecute = true });
    }
}
