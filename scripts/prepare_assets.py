import os
import json
import argparse
import subprocess
from pathlib import Path
from re import search

parser = argparse.ArgumentParser()
parser.add_argument('version', help='version number string')

args = parser.parse_args()

version = args.version.lower()

if version[:1].isdigit():
    version = 'v' + version

ci = os.environ.get('CI', False)

assets = []
# assets_dir = 'release_assets' if ci else 'local/release_assets'

bicep_dir = '{}/deploy/bicep'.format(Path.cwd())
assets_dir = '{}/{}'.format(Path.cwd(), 'release_assets' if ci else 'local/release_assets')


# Get CLI version
with open(Path(Path.cwd() / 'client/tc') / 'setup.py', 'r') as f:
    for line in f:
        if line.startswith('VERSION'):
            txt = str(line).rstrip()
            match = search(r'VERSION = [\'\"](.*)[\'\"]$', txt)
            if match:
                cli_version = match.group(1)
                cli_name = 'tc-{}-py2.py3-none-any.whl'.format(cli_version)

index = {}

index['teamcloud'] = {
    'version': '{}'.format(version),
    'deployUrl': 'https://github.com/microsoft/TeamCloud/releases/download/{}/azuredeploy.json'.format(version),
    'webZipUrl': 'https://github.com/microsoft/TeamCloud/releases/download/{}/TeamCloud.Web.zip'.format(version),
    'apiZipUrl': 'https://github.com/microsoft/TeamCloud/releases/download/{}/TeamCloud.API.zip'.format(version),
    'orchestratorZipUrl': 'https://github.com/microsoft/TeamCloud/releases/download/{}/TeamCloud.Orchestrator.zip'.format(version),
}

# index['webapp'] = {
#     'version': '{}'.format(version),
#     'deployUrl': 'https://github.com/microsoft/TeamCloud/releases/download/{}/azuredeploy.web.json'.format(version),
#     'zipUrl': 'https://github.com/microsoft/TeamCloud/releases/download/{}/TeamCloud.Web.zip'.format(version),
# }

index['extensions'] = {
    'tc': [
        {
            'downloadUrl': 'https://github.com/microsoft/TeamCloud/releases/download/{}/{}'.format(version, cli_name),
            'filename': '{}'.format(cli_name),
            'metadata': {
                'azext.isPreview': True,
                'azext.isExperimental': True,
                'azext.minCliCoreVersion': '2.10.0',
                'azext.maxCliCoreVersion': '3.0.0',
                'classifiers': [
                    'Development Status :: 4 - Beta',
                    'Intended Audience :: Developers',
                    'Intended Audience :: System Administrators',
                    'Programming Language :: Python',
                    'Programming Language :: Python :: 3',
                    'Programming Language :: Python :: 3.6',
                    'Programming Language :: Python :: 3.7',
                    'Programming Language :: Python :: 3.8',
                    'Programming Language :: Python :: 3.9',
                    'License :: OSI Approved :: MIT License',
                ],
                'extensions': {
                    'python.details': {
                        'contacts': [
                            {
                                'email': 'colbyw@microsoft.com',
                                'name': 'Microsoft Corporation',
                                'role': 'author'
                            }
                        ],
                        'document_names': {
                            'description': 'DESCRIPTION.rst'
                        },
                        'project_urls': {
                            'Home': 'https://github.com/microsoft/TeamCloud'
                        }
                    }
                },
                'generator': 'bdist_wheel (0.30.0)',
                'license': 'MIT',
                'metadata_version': '2.0',
                'name': 'tc',
                'summary': 'Microsoft Azure Command-Line Tools TeamCloud Extension',
                'version': '{}'.format(cli_version)
            }
        }
    ]
}

with open('{}/{}'.format(assets_dir, 'index.json'), 'w') as f:
    json.dump(index, f, ensure_ascii=False, indent=4, sort_keys=True)

with os.scandir(assets_dir) as s:
    for f in s:
        if f.is_file():
            print(f.path)
            name = f.name.rsplit('.', 1)[0]
            assets.append({'name': f.name, 'path': f.path})

if not ci:
    with open('{}/{}'.format(assets_dir, 'assets.json'), 'w') as f:
        json.dump(assets, f, ensure_ascii=False, indent=4, sort_keys=True)

print("::set-output name=assets::{}".format(json.dumps(assets)))
