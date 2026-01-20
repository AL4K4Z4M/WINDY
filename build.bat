@echo off
set "GAME_PATH=C:\Program Files (x86)\Steam\steamapps\common\Schedule I"
set "MOD_NAME=WindyFramework"

echo Building %MOD_NAME%...
dotnet build "%~dp0%MOD_NAME%.csproj" -c Release

if %ERRORLEVEL% EQU 0 (
    echo Build Successful!
    echo Cleaning legacy mods...
    if exist "%GAME_PATH%\Mods\Survival.dll" del "%GAME_PATH%\Mods\Survival.dll"
    if exist "%GAME_PATH%\Mods\WorldEditor.dll" del "%GAME_PATH%\Mods\WorldEditor.dll"
    
    echo Copying to Game Mods folder...
    copy /Y "%~dp0bin\Release\%MOD_NAME%.dll" "%GAME_PATH%\Mods\"
    echo Copying to Media Drive...
    copy /Y "%~dp0bin\Release\%MOD_NAME%.dll" "F:\MEDIA\"
    echo Success.
) else (
    echo Build FAILED. Check errors above.
)
pause