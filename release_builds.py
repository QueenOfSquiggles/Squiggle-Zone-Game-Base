import subprocess
import configparser
import os
import sys

# note: this is edited from the GIST to allow it to be in a different directory.


DEBUG = False
RELEASE_CONFIG_FILE = r'release_config.cfg'
BUTLER_CMD = '$HOME/.config/itch/apps/butler/butler'

def RunCommand(cmd : str):
    result = subprocess.run(cmd, shell=True)
    if DEBUG:
        print(result)

def StripChannelName(path : str) -> str:
    if path.endswith(".zip"):
        return path.removesuffix(".zip")
    if path.endswith(".app"):
        return path.removesuffix(".app")
    if path.endswith(".dmg"):
        return path.removesuffix(".dmg")
    return path

def ProcessChannel(path : str, name: str):
    print(f"\n\n\tProcessing channel for: {path}")
    channel = StripChannelName(name)

    cmd = f'{BUTLER_CMD} push {path} {author}/{game}:{channel}'
    if version != 'auto':
        cmd += f' --userversion {version}'
    if DEBUG:
        cmd += " -v"
    print(cmd)
    RunCommand(cmd)

def IsValidArchive(path : str) -> bool:
    if (path.lower().endswith(".zip")):
        return True
    if (path.lower().endswith(".app")):
        return True
    if (path.lower().endswith(".dmg")):
        return True
    return False
 

# read args
root_path = r"./export"
if len(sys.argv) > 1:
    root_path = sys.argv[1]

# Load configuration file
cfg = configparser.RawConfigParser()
cfg.read_file(open(RELEASE_CONFIG_FILE))


author = cfg.get('Meta', 'author')
game = cfg.get('Meta', 'game')
version = cfg.get('Game', 'version')

print(f'Running release for {author}\'s game "{game}". Version numbering: {version}')
dir_obj = os.scandir(root_path)
for entry in dir_obj:
    if entry.is_dir():
        print(f"Dir: {entry.name}")
    if entry.is_file() and IsValidArchive(entry.name):
        print(f"Archive: {entry.name}")

conf = input("Planned channels have been listed above. Enter 'Y' to confirm...")

if conf.lower() == 'y':
    dir_obj = os.scandir(root_path) # reload to start again
    for entry in dir_obj:
        if entry.is_dir():
            ProcessChannel(entry.path, entry.name)
        if entry.is_file() and IsValidArchive(entry.name):
            ProcessChannel(entry.path, entry.name)


input("\n\nPress enter to close terminal...")