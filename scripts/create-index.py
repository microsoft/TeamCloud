import json
import argparse
from pathlib import Path

parser = argparse.ArgumentParser()
parser.add_argument('version', help='version number string')

args = parser.parse_args()

version = args.version.lower()
if version[:1].isdigit():
    version = 'v' + version

index = {}

index['teamcloud'] = {
    'version': '{}'.format(version),
    'deployUrl': 'https://github.com/microsoft/TeamCloud/releases/download/{}/azuredeploy.json'.format(version),
    'apiZipUrl': 'https://github.com/microsoft/TeamCloud/releases/download/{}/TeamCloud.API.zip'.format(version),
    'orchestratorZipUrl': 'https://github.com/microsoft/TeamCloud/releases/download/{}/TeamCloud.Orchestrator.zip'.format(version),
}

index['webapp'] = {
    'version': '{}'.format(version),
    'deployUrl': 'https://github.com/microsoft/TeamCloud/releases/download/{}/azuredeploy.web.json'.format(version),
    'zipUrl': 'https://github.com/microsoft/TeamCloud/releases/download/{}/TeamCloud.Web.zip'.format(version),
}

with open(Path.cwd() / 'index.json', 'w') as f:
    json.dump(index, f, ensure_ascii=False, indent=4, sort_keys=True)
