@echo off

SET cdir=%~dp0
SET apiDll="bin\Debug\netcoreapp3.1\TeamCloud.API.dll"
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
    call npm uninstall teamcloud
    echo.

popd 

pushd %cdir%..\web\teamcloud 

    echo [TypeScript] Deleteing old node_modules
    rd /s /q .\node_modules
    echo.

    echo [TypeScript] Deleteing package.lock
    del /f /q .\package-lock.json
    echo.

popd 

pushd %cdir%..\openapi 

    echo Reseting autorest
    call autorest --reset
    echo.

    # echo "Generating python client"
    # autorest --v3 python.md
    # echo ""

    echo [TypeScript] Generating client
    call autorest --v3 typescript.md
    echo.

popd 

pushd %cdir%..\web\teamcloud 

    echo [TypeScript] Installing node packages
    call npm install
    echo.

    echo [TypeScript] Building client
    call npm run-script build
    echo.

popd 

pushd %cdir%..\web 

    echo [TypeScript] Installing temacloud to web
    call npm install .\teamcloud
    echo.

popd 

echo Done
echo.

