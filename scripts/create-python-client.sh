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

    echo "Generating swagger.json"
    dotnet swagger tofile --output ../../client/swagger.json $apiDll v1
    echo ""

    echo "Generating swagger.yaml"
    dotnet swagger tofile --yaml --output ../../client/swagger.yaml $apiDll v1
    echo ""

popd > /dev/null

pushd $cdir/../client > /dev/null

    echo "Reseting autorest"
    autorest --reset
    echo ""

    echo "Generating python client"
    autorest --v3 \
        --use:@autorest/python@latest \
        --input-file=swagger.yaml \
        --namespace=teamcloud \
        --add-credentials=true \
        --credential-scopes=openid \
        --override-client-name=TeamCloudClient \
        --license-header=MICROSOFT_MIT_NO_VERSION \
        --output-folder=tc/azext_tc/vendored_sdks/teamcloud \
        --no-namespace-folders=true \
        --clear-output-folder
    echo ""

popd > /dev/null

echo "Done."
echo ""
