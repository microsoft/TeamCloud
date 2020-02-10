#!/bin/sh
set -e

cdir=$(cd -P -- "$(dirname -- "$0")" && pwd -P)

echo "Deleting task hub for 'TeamCloud.Orchestrator'."

pushd $cdir/../src/TeamCloud.Orchestrator > /dev/null

    func durable delete-task-hub --connection-string-setting DurableFunctionsHubStorage
    echo ""

popd > /dev/null

for azureProvider in AppInsights DevOps DevTestLabs; do

    echo "Deleting task hub for 'TeamCloud.Providers.Azure.$azureProvider'."

    pushd $cdir/../../TeamCloud-Providers/Azure/TeamCloud.Providers.Azure.$azureProvider > /dev/null

        func durable delete-task-hub --connection-string-setting DurableFunctionsHubStorage
        echo ""

    popd > /dev/null

done

echo "Done."
