import json
index = {}
index['foo'] = {
    'version': 'v${{ steps.gitversion.outputs.majorMinorPatch }}'
}
print(json.dumps(index, indent=4))
