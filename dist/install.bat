@echo off
setlocal EnableDelayedExpansion
title Moon Sweepers QoL Mod - Installer

:: --- locate installer source folder (where this bat lives) ---
set "SRC=%~dp0"
:: remove trailing backslash
if "%SRC:~-1%"=="\" set "SRC=%SRC:~0,-1%"

:: --- try to auto-detect Steam install path from registry ---
set "STEAM="
for /f "tokens=2,*" %%A in ('reg query "HKLM\SOFTWARE\Valve\Steam" /v InstallPath 2^>nul') do set "STEAM=%%B"
if not defined STEAM (
  for /f "tokens=2,*" %%A in ('reg query "HKLM\SOFTWARE\WOW6432Node\Valve\Steam" /v InstallPath 2^>nul') do set "STEAM=%%B"
)

:: --- build the default game directory ---
if defined STEAM (
  set "GAME=%STEAM%\steamapps\common\Moon Sweepers"
) else (
  set "GAME="
)

:: --- ask the user to confirm / enter the game folder ---
echo.
echo Moon Sweepers QoL Mod installer
echo ---------------------------------
if defined GAME (
  echo Detected game folder:
  echo   %GAME%
  echo.
  set /p "ANS=Use this path? (Y = yes, or paste the game root folder): "
  if /i "!ANS!"=="Y" (
    set "DST=!GAME!"
  ) else (
    if not "!ANS!"=="" set "DST=!ANS!"
  )
) else (
  echo Could not auto-detect Steam. Please enter the game root folder
  echo (the folder that contains MoonSweepers.exe), e.g.:
  echo   C:\Program Files (x86)\Steam\steamapps\common\Moon Sweepers
  echo.
  set /p "DST=Game root folder: "
)

if not defined DST (
  echo No destination provided. Aborting.
  pause
  exit /b 1
)

:: strip quotes if user added them
set "DST=%DST:"=%"

if not exist "%DST%\MoonSweepers.exe" (
  if not exist "%DST%\Moon Sweepers.exe" (
    echo.
    echo WARNING: MoonSweepers.exe was not found in that folder.
    echo Destination: %DST%
    set /p "OK=Continue anyway? (Y/N): "
    if /i not "!OK!"=="Y" (
      echo Aborted.
      pause
      exit /b 1
    )
  )
)

:: --- copy BepInEx runtime + loader + plugin ---
echo.
echo Installing to: %DST%
xcopy "%SRC%\BepInEx" "%DST%\BepInEx\" /E /I /Y /Q >nul
xcopy "%SRC%\dotnet" "%DST%\dotnet\" /E /I /Y /Q >nul
copy /Y "%SRC%\winhttp.dll" "%DST%\winhttp.dll" >nul
copy /Y "%SRC%\doorstop_config.ini" "%DST%\doorstop_config.ini" >nul
copy /Y "%SRC%\README.md" "%DST%\MoonSweepersQoL_README.md" >nul

echo.
echo Done! BepInEx + Moon Sweepers QoL Mod installed.
echo Start the game normally; press F8 in-game to open the panel.
echo.
pause
endlocal
