import json
import requests
from re import search
from pathlib import Path


def get_new_uri():
    tunnels_uri = 'http://localhost:4040/api/tunnels'
    try:
        res = requests.get(tunnels_uri)
        res_unicode = res.content.decode("utf-8")
        res_json = json.loads(res_unicode)
        https_url = next((t['public_url'] for t in res_json["tunnels"] if t['public_url'].startswith('https')), None)
        return https_url
    except requests.exceptions.ConnectionError:
        return None


def get_old_uri():
    with open(Path(Path.cwd() / 'web') / '.env.development', 'r') as f:
        for line in f:
            if line.startswith('REACT_APP_TC_API_URL'):
                txt = str(line).rstrip()
                match = search(r'REACT_APP_TC_API_URL=(?:https?://[0-9a-zA-Z.-]*\.ngrok\.io)*$', txt)
                if match:
                    return match.group(0)
    return None


new_uri = get_new_uri()

if not new_uri:
    raise ValueError('ngrok not running, start ngrok and try again.')

old_uri = get_old_uri()

if not old_uri:
    raise ValueError('no old uri found in .env.development')

with open(Path(Path.cwd() / 'web') / '.env.development', 'r') as f:
    env_dev = f.read()

env_dev = env_dev.replace(old_uri, f'REACT_APP_TC_API_URL={new_uri}')

with open(Path(Path.cwd() / 'web') / '.env.development', 'w') as f:
    f.write(env_dev)
