@echo off
CLS

SET cdir=%~dp0
SET apiDll="bin\Debug\net6.0\TeamCloud.API.dll"
CD %cdir% && CLS

echo TeamCloud Client Generator
echo.

echo Installing AutoRest
call npm install -g autorest

pushd %cdir%..\src\TeamCloud.API

    echo Restoring dotnet tools
    dotnet tool restore
    echo.

    echo Generating openapi.json
    dotnet swagger tofile --output ..\..\openapi\openapi.json %apiDll% v1
    echo.

    echo Generating openapi.yaml
    dotnet swagger tofile --yaml --output ..\..\openapi\openapi.yaml %apiDll% v1
    echo.

popd

pushd %cdir%..\web

    echo Uninstalling teamcloud from web
    call npm uninstall teamcloud --legacy-peer-deps
    echo.

    echo Deleting web node_modules
    IF EXIST .\node_modules\ rd /q /s .\node_modules\
    echo.

    echo Deleting package lock
    IF EXIST .\package-lock.json del /f /q .\package-lock.json
    echo.

popd

pushd %cdir%..\web\teamcloud

    echo [TypeScript] Deleteing old node_modules
    IF EXIST .\node_modules\ rd /q /s .\node_modules\
    echo.

    echo [TypeScript] Deleteing package.lock
    IF EXIST .\package-lock.json del /f /q .\package-lock.json
    echo.

popd

pushd %cdir%..\openapi

    echo Reseting autorest
    call autorest --reset
    echo.

    echo "Generating python client"
    call autorest --v3 python.md
    echo ""

    echo [TypeScript] Generating client
    call autorest --v3 typescript.md
    echo.

popd

pushd %cdir%..\web\teamcloud

    echo [TypeScript] Installing node packages
    call npm install
    echo.

    echo [TypeScript] adding rimraf to dev dependencies
    call npm install rimraf@^3.0.0 -D
    echo.

    echo [TypeScript] Building client
    call npm run build
    echo.

    echo [TypeScript] Deleteing README.md
    IF EXIST ./README.md del /f /q ./README.md
    echo.

popd

pushd %cdir%..\web

    echo [TypeScript] Installing temacloud to web
    call npm install .\teamcloud --legacy-peer-deps
    echo.

popd

echo Done
echo.

