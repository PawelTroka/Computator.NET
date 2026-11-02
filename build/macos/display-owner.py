"""Track the actual private XQuartz server after a native launcher hand-off."""
import argparse
import json
import os
from pathlib import Path
import platform
import re
import shlex
import signal
import subprocess

parser=argparse.ArgumentParser(description=__doc__)
parser.add_argument('operation',choices=('record','stop'))
parser.add_argument('directory',type=Path)
parser.add_argument('--display')
args=parser.parse_args()
if platform.system()!='Darwin' or os.environ.get('GITHUB_ACTIONS')!='true':
    parser.error('only the disposable native Mac CI session is supported')
directory=args.directory.resolve(strict=True)
record=directory/'owned-server.json'
auth=str(directory/'Xauthority')


def servers(display):
    result=[]
    output=subprocess.check_output(['/bin/ps','-axo','pid=,uid=,pgid=,command='],text=True)
    for line in output.splitlines():
        fields=line.strip().split(None,3)
        if len(fields)!=4:
            continue
        pid,uid,pgid,command=fields
        # Only record the unique private server command, never other processes'
        # environments or unrelated command lines.
        if int(uid)!=os.getuid() or auth not in command or display not in command:
            continue
        tokens=shlex.split(command)
        if (not tokens or Path(tokens[0]).name not in ('Xquartz','X11.bin','X11')
                or display not in tokens or '-auth' not in tokens
                or tokens.index('-auth')+1>=len(tokens) or tokens[tokens.index('-auth')+1]!=auth):
            continue
        result.append({'pid':int(pid),'uid':int(uid),'pgid':int(pgid),'command':command,
                       'display':display,'auth':auth,'scope':'task-owned-authenticated-server'})
    return result


if args.operation=='record':
    if not args.display or not re.fullmatch(r':\d+',args.display):
        parser.error('record requires the exact private :DISPLAY number')
    observed=servers(args.display)
    (directory/'server-processes.json').write_text(json.dumps(observed,indent=2)+'\n')
    if len(observed)!=1:
        raise SystemExit('Cannot establish one actual server owner from the exact private display/auth arguments.')
    record.write_text(json.dumps(observed[0],indent=2)+'\n')
    print('Actual task-owned XQuartz server PID',observed[0]['pid'],'PGID',observed[0]['pgid'])
elif record.exists():
    owned=json.loads(record.read_text())
    if owned.get('auth')!=auth or not re.fullmatch(r':\d+',owned.get('display','')):
        raise SystemExit('Unsafe owned-server record; no process was signaled.')
    current=[item for item in servers(owned['display']) if item['pid']==owned['pid']]
    if current:
        os.kill(owned['pid'],signal.SIGTERM)
        print('Stopped the verified actual task-owned XQuartz server PID',owned['pid'])
