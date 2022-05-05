#!/bin/bash
set -e

cdir=$(cd -P -- "$(dirname -- "$0")" && pwd -P)
tc_dir=${cdir%/*}

apiDll=${1:-"bin/Debug/net6.0/TeamCloud.API.dll"}


log() { echo " " ; echo "[$(date +"%Y-%m-%d-%H%M%S")] $1"; echo " "; }
line() { echo " "; }

log "TeamCloud Python & Typescript Client Generator"

# check for autorest
if ! [ -x "$(command -v autorest)" ]; then
    log "[AutoRest] Installing AutoRest"
    npm install -g autorest
    # echo 'Error: autorest cli is not installed.\nAutoRest is required to run this script. To install the AutoRest, run npm install -g autorest, then try again. Aborting.' >&2
    # exit 1
fi

pushd $tc_dir/src/TeamCloud.API

    log "[dotnet] Restoring dotnet tools"
    dotnet tool restore

    log "[OpenAPI] Generating openapi.json"
    dotnet swagger tofile --output ../../openapi/openapi.json $apiDll v1

    log "[OpenAPI] Generating openapi.yaml"
    dotnet swagger tofile --yaml --output ../../openapi/openapi.yaml $apiDll v1

    if [ "$CI" = true ] ; then
        log "[OpenAPI] copying open api files to release_assets"
        cp ../../openapi/openapi.json ../../openapi/openapi.yaml ../../release_assets
    fi

popd

line

# pushd $tc_dir/web

#     log "[Web] Uninstalling teamcloud from web"
#     npm uninstall teamcloud --legacy-peer-deps

#     if [ -d ./node_modules ]; then
#         log "[Web] Deleteing web node_modules"
#         rm -rf ./node_modules
#     fi

#     if [ -f ./package-lock.json ]; then
#         log "[Web] Deleteing web package.lock"
#         rm ./package-lock.json
#     fi

# popd

# line

# pushd $tc_dir/web/teamcloud

#     # log "[TypeScript] Deleteing everything from teamcloud"
#     # rm -rf ./*

#     if [ -d ./node_modules ]; then
#         log "[TypeScript] Deleteing teamcloud node_modules"
#         rm -rf ./node_modules
#     fi

#     if [ -f ./package-lock.json ]; then
#         log "[TypeScript] Deleteing teamcloud package.lock"
#         rm ./package-lock.json
#     fi

# popd

# line

pushd $tc_dir/openapi

    log "[AutoRest] Reseting autorest"
    autorest --reset

    log "[AutoRest] Generating python client"
    autorest --v3 python.md

    log "[AutoRest] Generating typescript client"
    autorest --v3 typescript.md

popd

line

pushd $tc_dir/web/teamcloud

    log "[TypeScript] Installing node packages in teamcloud"
    npm install

    log "[TypeScript] adding rimraf to dev dependencies"
    npm install rimraf@^3.0.0 -D

    log "[TypeScript] Building client"
    npm run build

    if [ -f ./README.md ]; then
        log "[TypeScript] Deleteing README.md"
        rm ./README.md
    fi

popd

line

pushd $tc_dir/web

    log "[TypeScript] Installing temacloud to web"
    npm install ./teamcloud --legacy-peer-deps

popd

log "Done."
