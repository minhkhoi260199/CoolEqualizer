# Cool Equalizer (APO) 🎛️

A lightweight, zero-dependency, standalone 16-band Graphic Equalizer GUI built in native C# WinForms. Designed to run as a front-end for [Equalizer APO](https://equalizerapo.com/), this application features a sleek, futuristic VST Studio interface.

![Cool Equalizer Interface](CE-icon2.png)

## 🌟 Features
- **16-Band Graphic EQ**: Perfectly spaced frequency bands (2/3 octave) for precise audio control.
- **Pre-amp Slider**: Dedicated pre-amp control isolated on the left side to compensate for volume loss when cutting frequencies.
- **Elastic String Effect (Right-click)**: Dragging sliders with the Right Mouse Button simulates an elastic string, pulling adjacent bands automatically using Gaussian decay. 
- **Single Band Edit (Left-click)**: Standard drag-and-drop on individual frequency bands.
- **Modern Neon UI**: A beautiful cyberpunk-inspired VST-like interface with glowing Spline curves, semi-transparent spectrum bars, and a custom-drawn glowing "Modern Toggle" Power Switch.
- **Preset Management**: Create, save, and delete custom presets. Includes default presets like Bass Boost, Treble Boost, and V-Shape.
- **Auto-Persistence**: Remembers your EQ curve, Pre-amp value, and selected preset upon closing and automatically restores them when you reopen the app.
- **Zero Dependencies**: Compiles directly using the built-in Windows `.NET csc.exe` compiler. No Visual Studio or heavy SDKs required.

## 🛠️ Prerequisites
- **Windows OS** with .NET Framework 4.0 or higher (built-in on modern Windows).
- **[Equalizer APO](https://sourceforge.net/projects/equalizerapo/)** installed and configured for your audio playback device.

## 🚀 How to Build and Run
You do not need an IDE or any external tools to build this app.

1. Clone or download this repository.
2. Double-click the `Build_Native.bat` script.
   *(This script automatically uses the native Windows C# compiler `csc.exe` to compile the app in less than a second).*
3. A new file named `CoolEqualizer.exe` will be generated.
4. Double-click `CoolEqualizer.exe` to run the app.

## ⚙️ How it works
Under the hood, the app modifies the `C:\Program Files\EqualizerAPO\config\cool_eq.txt` file and links it to your main APO configuration. Equalizer APO instantly applies the EQ rules to your system audio output without any latency.

## 📝 License
This project is open-source and free to use.
