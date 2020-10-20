import json
import argparse

parser = argparse.ArgumentParser()
parser.add_argument('version', help='version number string')

args = parser.parse_args()

version = args.version.lower()
if version[:1].isdigit():
    version = 'v' + version

index = {}
index['foo'] = {
    'version': '{}'.format(version)
}

print(json.dumps(index, indent=4))

print("::set-output name=version::{}".format(version))
