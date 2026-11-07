#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")"
mcs -langversion:latest -out:run.exe OpenMorph*.cs Shim.cs Runner.cs
mono run.exe "$1" "$2"
