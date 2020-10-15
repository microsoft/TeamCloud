#!/bin/bash
set -e

cdir=$(cd -P -- "$(dirname -- "$0")" && pwd -P)
tcdir=${cdir%/*}

echo "TeamCloud CLI Build Utility"
echo ""

pushd $tcdir > /dev/null

    echo "Creating a virtual environment"
    python -m venv env
    echo ""

    echo "Activating virtual environment"
    source env/bin/activate
    echo ""

    echo "Installing Azure CLI Dev Tools (azdev)"
    pip install azdev
    echo ""

    echo "Setting up Azure CLI Dev Tools (azdev)"
    azdev setup -r $PWD -e tc
    echo ""

    echo "Running Linter on TeamCloud CLI source"
    azdev linter tc
    echo ""

    echo "Running Style Checks on TeamCloud CLI source"
    azdev style tc
    echo ""

    echo "Building TeamCloud CLI"
    azdev extension build tc
    echo ""

    echo "Deactivating virtual environment"
    deactivate
    echo ""

popd > /dev/null

echo "Done."
echo ""
