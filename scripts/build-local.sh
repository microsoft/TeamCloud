#!/bin/bash
set -e

cdir=$(cd -P -- "$(dirname -- "$0")" && pwd -P)
tc_dir=${cdir%/*}

sem_ver="1.0.0"
build_config="Release"

src_dir="$tc_dir/src"
web_dir="$tc_dir/web"
bic_dir="$tc_dir/deploy/bicep"
loc_dir="$tc_dir/local"
rel_dir="$tc_dir/local/release_assets"

echo "Build & Package for Release"

dotnet build "$src_dir/TeamCloud.sln" -c Release -p:VersionPrefix="$sem_ver" -p:AssemblyVersion="$sem_ver" -p:FileVersion="$sem_ver" -p:IncludeSymbols=true


echo "Create Release Asset - TeamCloud.API"

dotnet publish "$src_dir/TeamCloud.API/TeamCloud.API.csproj" -o $loc_dir/TeamCloud.API -c Release -p:VersionPrefix="$sem_ver" -p:AssemblyVersion="$sem_ver" -p:FileVersion="$sem_ver" -p:IncludeSymbols=true --no-build

pushd $loc_dir/TeamCloud.API > /dev/null
    zip -r $rel_dir/TeamCloud.API.zip *
popd > /dev/null


echo "Create Release Asset - TeamCloud.Orchestrator"

dotnet publish "$src_dir/TeamCloud.Orchestrator/TeamCloud.Orchestrator.csproj" -o $loc_dir/TeamCloud.Orchestrator -c Release -p:VersionPrefix="$sem_ver" -p:AssemblyVersion="$sem_ver" -p:FileVersion="$sem_ver" -p:IncludeSymbols=true --no-build

pushd $loc_dir/TeamCloud.Orchestrator > /dev/null
    zip -r $rel_dir/TeamCloud.Orchestrator.zip *
popd > /dev/null


echo "Create Release Asset - TeamCloud.Web"

pushd $web_dir > /dev/null
    zip -r $rel_dir/TeamCloud.Web.zip * .deployment -x .vscode -x \*.md -x .DS_Store -x .env.development -x build/\* -x lib/\* -x node_modules/\* -x .gitignore
popd > /dev/null


echo "Compile Bicep Templates to ARM"

pushd $bic_dir > /dev/null
    az bicep build -f main.bicep --outfile $rel_dir/azuredeploy.json
popd > /dev/null