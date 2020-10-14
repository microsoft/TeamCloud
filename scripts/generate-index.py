import json
from pathlib import Path

index = {}
index['teamcloud'] = {
    'version': 'v${{ steps.gitversion.outputs.majorMinorPatch }}',
    'deployUrl': 'https://github.com/microsoft/TeamCloud/releases/download/v${{ steps.gitversion.outputs.majorMinorPatch }}/azuredeploy.json',
    'apiZipUrl': 'https://github.com/microsoft/TeamCloud/releases/download/v${{ steps.gitversion.outputs.majorMinorPatch }}/TeamCloud.API.zip',
    'orchestratorZipUrl': 'https://github.com/microsoft/TeamCloud/releases/download/v${{ steps.gitversion.outputs.majorMinorPatch }}/TeamCloud.Orchestrator.zip',
}
index['webapp'] = {
    'version': 'v${{ steps.gitversion.outputs.majorMinorPatch }}',
    'deployUrl': 'https://github.com/microsoft/TeamCloud/releases/download/v${{ steps.gitversion.outputs.majorMinorPatch }}/azuredeploy.web.json',
    'zipUrl': 'https://github.com/microsoft/TeamCloud/releases/download/v${{ steps.gitversion.outputs.majorMinorPatch }}/TeamCloud.Web.zip',
}
with open(Path.cwd() / 'index.json', 'w') as f:
    json.dump(index, f, ensure_ascii=False, indent=4, sort_keys=True)
