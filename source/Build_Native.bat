@echo off
set CSC=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe

if not exist "%CSC%" (
    echo Khong tim thay trinh bien dich C#.
    pause
    exit /b
)

echo Dang bien dich CoolEqualizer.exe...
"%CSC%" /r:Microsoft.VisualBasic.dll /win32icon:app_icon.ico /target:winexe /out:CoolEqualizer.exe EqualizerApp.cs

if exist CoolEqualizer.exe (
    echo Xong! Da tao CoolEqualizer.exe
) else (
    echo Loi bien dich.
)
pause
