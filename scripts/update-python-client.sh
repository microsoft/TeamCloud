#!/bin/sh
set -e

cdir=$(cd -P -- "$(dirname -- "$0")" && pwd -P)

pushd $cdir/../src/TeamCloud.API > /dev/null

    echo "Restoring dotnet tools"
    dotnet tool restore

    echo "Generating swagger.json"
    dotnet swagger tofile --output ../../client/swagger.json bin/Debug/netcoreapp3.1/TeamCloud.API.dll v1

    echo "Generating swagger.yaml"
    dotnet swagger tofile --yaml --output ../../client/swagger.yaml bin/Debug/netcoreapp3.1/TeamCloud.API.dll v1

    echo ""

popd > /dev/null

pushd $cdir/../client > /dev/null

    echo "Reseting autorest"
    autorest --reset

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
