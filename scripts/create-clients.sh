#!/bin/bash
set -e

cdir=$(cd -P -- "$(dirname -- "$0")" && pwd -P)
tc_dir=${cdir%/*}

apiDll=${1:-"bin/Debug/net6.0/TeamCloud.API.dll"}

echo "TeamCloud Python & Typescript Client Generator"
echo ""

# echo $apiDll
# exit 0

# check for autorest
if ! [ -x "$(command -v autorest)" ]; then
    echo "Installing AutoRest"
    npm install -g autorest
    echo ""
    # echo 'Error: autorest cli is not installed.\nAutoRest is required to run this script. To install the AutoRest, run npm install -g autorest, then try again. Aborting.' >&2
    # exit 1
fi


pushd $tc_dir/src/TeamCloud.API > /dev/null

    echo "Restoring dotnet tools"
    dotnet tool restore
    echo ""

    echo "Generating openapi.json"
    dotnet swagger tofile --output ../../openapi/openapi.json $apiDll v1
    echo ""

    echo "Generating openapi.yaml"
    dotnet swagger tofile --yaml --output ../../openapi/openapi.yaml $apiDll v1
    echo ""

    if [ "$CI" = true ] ; then
        cp ../../openapi/openapi.json ../../openapi/openapi.yaml ../../release_assets
    fi

popd > /dev/null

pushd $tc_dir/web > /dev/null

    echo "Uninstalling teamcloud from web"
    npm uninstall teamcloud
    echo ""

popd > /dev/null

pushd $tc_dir/web/teamcloud > /dev/null

    if [ -d ./node_modules ]; then
        echo "[TypeScript] Deleteing old node_modules"
        rm -rf ./node_modules
        echo ""
    fi

    if [ -f ./package-lock.json ]; then
        echo "[TypeScript] Deleteing package.lock"
        rm ./package-lock.json
        echo ""
    fi

popd > /dev/null

pushd $tc_dir/openapi > /dev/null

    echo "Reseting autorest"
    autorest --reset
    echo ""

    echo "Generating python client"
    autorest --v3 python.md
    echo ""

    echo "[TypeScript] Generating client"
    autorest --v3 typescript.md
    echo ""

popd > /dev/null

pushd $tc_dir/web/teamcloud > /dev/null

    echo "[TypeScript] Installing node packages"
    npm install
    echo ""

    echo "[TypeScript] Building client"
    npm run-script build
    echo ""

    if [ -f ./README.md ]; then
        echo "[TypeScript] Deleteing README.md"
        rm ./README.md
        echo ""
    fi

popd > /dev/null

pushd $tc_dir/web > /dev/null

    echo "[TypeScript] Installing temacloud to web"
    npm install ./teamcloud
    echo ""

popd > /dev/null

echo "Done."
echo ""

