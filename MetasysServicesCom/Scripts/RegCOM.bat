@echo off

SET newPath=HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full

SET Key_Name=Version

REM query the value. pipe it through findstr in order to find the matching line that has the value
FOR /f "tokens=2,*" %%a IN ('reg query "%newPath%" /v %Key_Name% ^| findstr %Key_Name%') DO (
    SET Key_Value=%%b
)
REM extract Home drive from Win Dir path
SET Home_Drive=%windir:~0,2%
REM check registry .NET Version that required version is installed
IF "%Key_Value%"=="4.7.03062" ( 
    echo .NET Framework 4.7.2 is Installed
) ELSE (
   dotnetfx472_full_x86_x64-4.7.3081.0.exe \q \norestart
)
REM set base path for DLL
SET JCI_COM_PATH=%Home_Drive%\ProgramData\Johnson Controls\Metasys Services
REM Register using x86 .NET Framework according to the client lib
cd %windir%\Microsoft.NET\Framework\v4.0.30319
regasm "%JCI_COM_PATH%\MetasysServicesCom.dll" /tlb:"%JCI_COM_PATH%\MetasysServicesCom.tlb"

