@echo off
set CSC=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe

if not exist "%CSC%" (
    echo Khong tim thay trinh bien dich C#.
    pause
    exit /b
)

echo Dang bien dich CoolEqualizer_Setup.exe...
"%CSC%" /target:winexe /win32icon:..\portable\app_icon.ico /resource:..\EqualizerAPO-x64-1.4.2.exe /resource:..\portable\CoolEqualizer.exe /out:CoolEqualizer_Setup.exe InstallerApp.cs

if exist CoolEqualizer_Setup.exe (
    echo Xong! Da tao CoolEqualizer_Setup.exe
) else (
    echo Loi bien dich.
)
pause
