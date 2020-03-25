@echo off

cd "%~dp0"
start "" cmd /C azurestorageemulator start
dotnet build -c Debug

SET terminal=wt -d .\TeamCloud.API cmd /C "dotnet run --no-build" ;
SET terminal=%terminal% split-pane -V -d .\TeamCloud.Orchestrator cmd /C "func host start --no-build --script-root bin/Debug/netcoreapp3.1"

start "" /B %terminal%
