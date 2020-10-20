#!/bin/bash
set -e

cdir=$(cd -P -- "$(dirname -- "$0")" && pwd -P)

apiDll=${1:-"bin/Debug/netcoreapp3.1/TeamCloud.API.dll"}

echo "TeamCloud Python Client Generator"
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


pushd $cdir/../src/TeamCloud.API > /dev/null

    echo "Restoring dotnet tools"
    dotnet tool restore
    echo ""

    echo "Generating openapi.json"
    dotnet swagger tofile --output ../../openapi/openapi.json $apiDll v1
    echo ""

    echo "Generating openapi.yaml"
    dotnet swagger tofile --yaml --output ../../openapi/openapi.yaml $apiDll v1
    echo ""

popd > /dev/null

pushd $cdir/../openapi > /dev/null

    echo "Reseting autorest"
    autorest --reset
    echo ""

    echo "Generating python client"
    autorest --v3 python.md
    echo ""

    echo "Generating typescript client"
    autorest --v3 typescript.md
    echo ""

popd > /dev/null

echo "Done."
echo ""

