<div align="center">

![English](https://img.shields.io/badge/🇺🇸%20English-5865F2?style=for-the-badge)
[![Português](https://img.shields.io/badge/🇧🇷%20Português-2b2d31?style=for-the-badge)](portugues.md)

<br>

# Equicord Installer

Install **[Equicord](https://github.com/Equicord/Equicord)** on your Discord with **one click**.
No terminal, no commands — the installer downloads and sets up **everything for you**.

<br>

<!-- Change "Overocai/Equicord-Installer" below if your repository name is different -->
### [⬇️ Download Equicord Installer](https://github.com/Overocai/Equicord-Installer/releases/latest/download/Equicord.Installer.exe)

[![Download now](https://img.shields.io/badge/⬇%20DOWNLOAD%20NOW-5865F2?style=for-the-badge&logo=discord&logoColor=white)](https://github.com/Overocai/Equicord-Installer/releases/latest/download/Equicord.Installer.exe)

<sub>Windows · free · open source</sub>

</div>

---

## 💡 Why this exists

I know how annoying it is to install **Node.js**, **Git** and **pnpm**, then clone
**Equicord/Vencord** and build it — just to install one third-party plugin.
So I automated the whole thing. Download one file, click once, done.

---

## ✨ What it does for you

Just open the program and click **Start Installation**. It does the rest **automatically**:

- ⬇️ Downloads and installs **Git** (official installer, if you don't have it)
- ⬇️ Downloads and installs **Node.js LTS** (official installer, if you don't have it)
- 📦 Installs **pnpm**
- 📂 Clones **Equicord** into your **Documents** folder
- 🔨 Installs dependencies and runs the **build**
- 🚀 Opens the Discord installer (`pnpm inject`) at the end — optional

You don't need to install anything beforehand. **Download, open, click. Done.**

---

## 🚀 How to use

1. Click **[⬇️ Download Equicord Installer](https://github.com/Overocai/Equicord-Installer/releases/latest/download/Equicord.Installer.exe)**.
2. Open the `Equicord Installer.exe` you downloaded.
3. Click **Start Installation** and wait.
4. When Windows asks for administrator permission (UAC), click **Yes**.
5. At the end, **fully close Discord** before continuing the injection.

> 💡 Keep the **"Open the Discord installer (pnpm inject) at the end"** option checked
> to inject into Discord automatically.

---

## 🔄 Updating later

Run the installer again anytime. If Equicord is already in your **Documents**
folder, it **updates to the latest version** — and your `userplugins` are preserved.

---

## ❓ FAQ

**Where is Equicord installed?**
In your `Documents\Equicord` folder.

**Why does it ask for administrator permission?**
Only to install official Git and Node.js (in case you don't have them yet). The
rest runs without admin.

**My antivirus flagged it.**
That's a common false positive with unsigned installers. The code is open source —
you can check everything on this page.

---

<div align="center">
<sub>Made for the <a href="https://github.com/Equicord/Equicord">Equicord</a> community · not officially affiliated with Discord.</sub>
</div>
