# Cool Equalizer - All In One Setup 📦

This folder contains a custom-built **C# Bootstrapper Installer**.

## 🌟 What does it do?
It bundles two massive applications into a single `.exe`:
1. The **Equalizer APO Installer** (`EqualizerAPO-x64-1.4.2.exe`).
2. The **Cool Equalizer Portable App** (`CoolEqualizer.exe`).

When a user runs `CoolEqualizer_Setup.exe`:
- It prompts for Administrator privileges (required to install Equalizer APO).
- It extracts `CoolEqualizer.exe` directly into `C:\Program Files\EqualizerAPO\`.
- It automatically creates a shortcut on the user's Desktop for quick access.
- It extracts and launches the native Equalizer APO setup wizard.
- Everything is done sequentially and safely before Equalizer APO can request a system reboot.

## 🛠️ How to Build
Double-click `Build_Setup.bat`. 

**Note**: You must have already built the portable version (in `../portable/`), as the compiler pulls `portable\CoolEqualizer.exe` and `EqualizerAPO-x64-1.4.2.exe` to embed them as resources.
