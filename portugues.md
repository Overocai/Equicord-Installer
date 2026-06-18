<div align="center">

[![English](https://img.shields.io/badge/🇺🇸%20English-2b2d31?style=for-the-badge)](README.md)
[![Português](https://img.shields.io/badge/🇧🇷%20Português-5865F2?style=for-the-badge)](portugues.md)

<br>

# Equicord Installer

Instale o **[Equicord](https://github.com/Equicord/Equicord)** no seu Discord com **um clique**.
Sem terminal, sem comandos — o instalador baixa e configura **tudo sozinho**.

<br>

<!-- Troque "overocai/Equicord-Installer" abaixo se o nome do repositório for outro -->
### [⬇️ Baixar o Equicord Installer](https://github.com/overocai/Equicord-Installer/releases/latest/download/Equicord%20Installer.exe)

[![Baixar agora](https://img.shields.io/badge/⬇%20BAIXAR%20AGORA-5865F2?style=for-the-badge&logo=discord&logoColor=white)](https://github.com/overocai/Equicord-Installer/releases/latest/download/Equicord%20Installer.exe)

<sub>Windows · grátis · código aberto</sub>

</div>

---

## 💡 Por que isso existe

Eu sei como é chato baixar **Node.js**, **Git** e **pnpm**, depois clonar o
**Equicord/Vencord** e fazer o build — só pra instalar um plugin de terceiro.
Então automatizei isso tudo. Baixa um arquivo, clica uma vez, pronto.

---

## ✨ O que ele faz por você

É só abrir o programa e clicar em **Iniciar Instalação**. Ele faz o resto **automaticamente**:

- ⬇️ Baixa e instala o **Git** (instalador oficial, se você não tiver)
- ⬇️ Baixa e instala o **Node.js LTS** (instalador oficial, se você não tiver)
- 📦 Instala o **pnpm**
- 📂 Clona o **Equicord** na sua pasta **Documentos**
- 🔨 Instala as dependências e faz o **build**
- 🚀 Abre o instalador do Discord (`pnpm inject`) no final — opcional

Você não precisa instalar nada antes. **Baixou, abriu, clicou. Pronto.**

---

## 🚀 Como usar

1. Clique em **[⬇️ Baixar o Equicord Installer](https://github.com/overocai/Equicord-Installer/releases/latest/download/Equicord%20Installer.exe)**.
2. Abra o `Equicord Installer.exe` que baixou.
3. Clique em **Iniciar Instalação** e aguarde.
4. Quando o Windows pedir permissão de administrador (UAC), clique em **Sim**.
5. No final, **feche o Discord completamente** antes de continuar a injeção.

> 💡 Deixe a opção **"Abrir o instalador do Discord (pnpm inject) ao final"** marcada
> para já injetar no Discord automaticamente.

---

## 🔄 Atualizar depois

Rode o instalador de novo a qualquer momento. Se o Equicord já estiver na pasta
**Documentos**, ele **atualiza para a versão mais recente** — e os seus
`userplugins` são preservados.

---

## ❓ Dúvidas comuns

**Onde o Equicord é instalado?**
Na sua pasta `Documentos\Equicord`.

**Por que pede permissão de administrador?**
Só para instalar o Git e o Node.js oficiais (caso você ainda não tenha). O resto
roda sem admin.

**O antivírus reclamou.**
É um falso positivo comum com instaladores não assinados. O código é aberto —
você pode conferir tudo nesta página.

---

<div align="center">
<sub>Feito para a comunidade do <a href="https://github.com/Equicord/Equicord">Equicord</a> · não afiliado oficialmente ao Discord.</sub>
</div>
