#!/bin/bash
set -e
cd "$(dirname "$0")"
export MONO_PATH="$PWD/root/usr/lib/mono/4.5"
export LD_LIBRARY_PATH="$PWD/root/usr/lib"
export MONO_CFG_DIR="$PWD/root/etc"
root/usr/bin/mono-sgen "$MONO_PATH/mcs.exe" -langversion:latest -nostdlib -r:"$MONO_PATH/mscorlib.dll" -r:"$MONO_PATH/System.Core.dll" -r:"$MONO_PATH/System.dll" -out:run.exe *.cs
root/usr/bin/mono-sgen run.exe "$1" "$2"
