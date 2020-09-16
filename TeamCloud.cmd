@echo off
set "lock=%temp%\wait%random%.lock"

cd "%~dp0"
start "" cmd /C azurestorageemulator start
start /D .\src "" 9>"%lock%1" dotnet build --force -c Debug
start /D .\web\teamcloud "" 9>"%lock%2" cmd /C npm install

:: wait for a locks to be resolved - modify loop if more parallel
:: tasks should be supported
:Wait 
1>nul 2>nul ping /n 2 ::1
for %%N in (1 2) do (
  (call ) 9>"%lock%%%N" || goto :Wait
) 2>nul

:: do some cleanup work
del "%lock%*"

SET terminal=wt -d .\src\TeamCloud.API cmd /C "dotnet run --no-build" ;
SET terminal=%terminal% split-pane -V -d .\src\TeamCloud.Orchestrator cmd /C "func host start --no-build --script-root bin/Debug/netcoreapp3.1" ;
SET terminal=%terminal% split-pane -H -d .\web\teamcloud cmd /C "npm start"

start "" /B %terminal%
