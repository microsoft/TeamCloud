@echo off

cls
cd "%~dp0"
set "lock=%temp%\wait%random%.lock"

echo   ______                     ________                __
echo  /_  __/__  ____ _____ ___  / ____/ /___  __  ______/ /
echo   / / / _ \/ __ `/ __ `__ \/ /   / / __ \/ / / / __  / 
echo  / / /  __/ /_/ / / / / / / /___/ / /_/ / /_/ / /_/ /  
echo /_/  \___/\__,_/_/ /_/ /_/\____/_/\____/\__,_/\__,_/   
echo.

echo - Starting Azure Storage Emulator
start "" cmd /C azurestorageemulator start

echo - Building API and Orchestrator
start /min /D .\src "" 9>"%lock%1" dotnet build --force -c Debug

echo - Building Web UI
start /min /D .\web\teamcloud "" 9>"%lock%2" cmd /C npm install

:Wait 
1>nul 2>nul ping /n 2 ::1
for %%N in (1 2) do (
  (call ) 9>"%lock%%%N" || goto :Wait
) 2>nul

del "%lock%*"

SET terminal=wt -d .\src\TeamCloud.API cmd /C "dotnet run --no-build" ;
SET terminal=%terminal% split-pane -V -d .\src\TeamCloud.Orchestrator cmd /C "func host start --no-build --script-root bin/Debug/netcoreapp3.1" ;
SET terminal=%terminal% split-pane -H -d .\web\teamcloud cmd /C "npm start"

start "" /B %terminal%
