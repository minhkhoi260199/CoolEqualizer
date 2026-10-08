# Cool Equalizer - Portable Edition 🚀

This folder contains the **100% Portable Edition** of the Cool Equalizer (APO) front-end.

## 🌟 Why Portable?
- **True Single-File Executable**: The application compiles into a single, lightweight `CoolEqualizer.exe` (~1.5 MB).
- **Self-Extracting UI Assets**: It automatically extracts its own embedded ICO file (`/win32icon`) to render the high-quality logo inside the app. No external `.png` or `.ico` files need to accompany the `.exe`.
- **Zero Desktop Clutter**: Unlike traditional portable apps that drop `.txt` or `.ini` files next to the executable, this version cleverly hides its settings (`cool_settings.txt` and `cool_presets.txt`) directly inside the Equalizer APO configuration directory (`C:\Program Files\EqualizerAPO\config\`). You can leave the `.exe` on your pristine Desktop without it generating messy side-files!

## 🛠️ How to Build
Simply double-click the `Build_Native.bat` script. It uses the built-in Windows `csc.exe` compiler to instantly generate `CoolEqualizer.exe`. No Visual Studio required.

## 🎛️ Core Features
- **16-Band Graphic EQ** with an isolated Pre-amp slider.
- **Elastic String Dragging** (Right-Click) to move multiple bands simultaneously.
- **Modern Neon UI** with a custom-drawn Power Toggle and glowing Splines.
- **Auto-Persistence** across sessions.
