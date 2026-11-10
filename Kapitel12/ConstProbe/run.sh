#!/bin/bash
# Shows that a const is copied into the caller and a static readonly is not.
# Usage: bash run.sh   Expected output: "3 3", then "3 7".
set -e
probe="$(cd "$(dirname "$0")" && pwd)"
out="$probe/bin/run"
dotnet build -v q -nologo "$probe/App/ConstProbeApp.csproj" -o "$out/app"
dotnet "$out/app/ConstProbeApp.dll"
dotnet build -v q -nologo "$probe/Library/ConstProbeLibrary.csproj" \
    -p:DefineConstants=SEVEN -o "$out/library"
cp "$out/library/ConstProbeLibrary.dll" "$out/app/"
dotnet "$out/app/ConstProbeApp.dll"
