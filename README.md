# Cool Equalizer (APO) 🎛️

A lightweight, zero-dependency, standalone 16-band Graphic Equalizer GUI built in native C# WinForms. Designed to run as a front-end for [Equalizer APO](https://equalizerapo.com/) (Stable at EqualizerAPO-x64-1.4.2), this application features a sleek, futuristic VST Studio interface.

## 📁 Repository Structure

This repository is split into two distinct versions of the application to serve different use cases:

### 1. [`/portable`](./portable/) (Recommended 🌟)
The ultimate **100% Portable** version of the app. 
- The executable (`CoolEqualizer.exe`) is completely self-contained.
- The UI logo is dynamically extracted from the `.exe`'s embedded resources, requiring no external image files.
- Settings and presets are gracefully hidden inside the Equalizer APO directory (`C:\Program Files\EqualizerAPO\config\`), keeping your desktop perfectly clean.
- *Best for end-users who just want to carry a single `.exe` file on a USB drive or leave it on their desktop.*

### 2. [`/source`](./source/) (Classic Version)
The traditional version of the app.
- Keeps configuration files (`cool_settings.txt` and `presets.txt`) strictly localized in the same folder as the executable.
- Relies on an external image file (`CE-icon2.png`) to draw the in-app logo.
- *Best for developers who want to observe standard file I/O behavior and have explicit access to their local setting files.*

---

## 🛠️ **Prerequisites for both versions:**

Windows OS with .NET Framework 4.0+ and [Equalizer APO](https://equalizerapo.com/) installed **(Stable at EqualizerAPO-x64-1.4.2)**.

### 3. [/Installer](./Installer/) (All-in-One Setup)
A custom C# bootstrapper that bundles both the Equalizer APO installer and our Portable executable into a single "CoolEqualizer_Setup.exe". When run, it silently drops the app into "C:\Program Files\EqualizerAPO", creates a Desktop shortcut, and then launches the APO installation wizard. 
- *Best for distribution to new users who haven't installed Equalizer APO yet.*
