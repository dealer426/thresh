import urllib.request, urllib.error, json, ssl, sys

ctx = ssl.create_default_context()
ctx.check_hostname = False
ctx.verify_mode = ssl.CERT_NONE

token = 'thresh_cli_1ac0a25e-3ed1-4375-b8e6-3bef1ef39cf5_DIK6B09reEJDkVCOik7ojkgXYIke56C7'
url = 'https://192.168.4.85:7200/mcp'

payload = {'jsonrpc':'2.0','id':1,'method':'tools/call','params':{'name':'list_blueprints','arguments':{'node_id':'df312049-ca8e-4fb4-ae2c-cba4fbcf0b65'}}}
body = json.dumps(payload).encode()

req = urllib.request.Request(url, data=body, headers={'Content-Type':'application/json','Authorization':'Bearer '+token})
try:
    with urllib.request.urlopen(req, context=ctx, timeout=30) as resp:
        print(resp.read().decode())
except urllib.error.HTTPError as e:
    print(f'HTTP {e.code}: {e.read().decode()}')
except Exception as e:
    print(f'Error: {e}')
