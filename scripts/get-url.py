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
        public_url = res_json["tunnels"][0]["public_url"]
        print(public_url)
        return public_url
    except requests.exceptions.ConnectionError:
        return None


def get_ole_uri():
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

old_uri = get_ole_uri()

if not old_uri:
    raise ValueError('no old uri found in .env.development')

with open(Path(Path.cwd() / 'web') / '.env.development', 'r') as f:
    env_dev = f.read()

env_dev = env_dev.replace(old_uri, f'REACT_APP_TC_API_URL={new_uri}')

with open(Path(Path.cwd() / 'web') / '.env.development', 'w') as f:
    f.write(env_dev)
