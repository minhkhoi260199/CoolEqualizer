# Cool Equalizer - Classic Source Edition 💾

This folder contains the **Classic Version** of the Cool Equalizer (APO) front-end.

## 🌟 Characteristics of this version
- **Localized Settings**: The configuration files (`cool_settings.txt` and `presets.txt`) are generated and saved **in the exact same directory** as the executable. This makes it a standard portable application where the user's data roams directly with the `.exe`.
- **External Asset Dependency**: The application requires the `CE-icon2.png` file to be present in its directory to render the UI logo inside the top bar. (Note: The taskbar icon is still permanently embedded into the `.exe` for pinning support).

## 🛠️ How to Build
Simply double-click the `Build_Native.bat` script. It uses the built-in Windows `csc.exe` compiler to instantly generate `CoolEqualizer.exe`. No Visual Studio required.

## 🎛️ Core Features
- **16-Band Graphic EQ** with an isolated Pre-amp slider.
- **Elastic String Dragging** (Right-Click) to move multiple bands simultaneously.
- **Modern Neon UI** with a custom-drawn Power Toggle and glowing Splines.
- **Preset Management** with real-time saving to the local `presets.txt` file.
