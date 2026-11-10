#!/bin/bash
# Shows that a default value is copied into the caller, and that adding an
# optional parameter breaks a compiled caller. Usage: bash run.sh
# Expected: 250, 250 (new DLL), 300 (app rebuilt), MissingMethodException.
set -e
probe="$(cd "$(dirname "$0")" && pwd)"
out="$probe/bin/run"
app="$probe/App/OptionalProbeApp.csproj"
lib="$probe/Library/OptionalProbeLibrary.csproj"
dotnet build -v q -nologo "$app" -o "$out/v1"
dotnet "$out/v1/OptionalProbeApp.dll" for
dotnet build -v q -nologo "$lib" -p:DefineConstants=V2 -o "$out/lib2"
cp "$out/lib2/OptionalProbeLibrary.dll" "$out/v1/"
dotnet "$out/v1/OptionalProbeApp.dll" for
dotnet build -v q -nologo "$app" -p:DefineConstants=V2 -o "$out/v2"
dotnet "$out/v2/OptionalProbeApp.dll" for
dotnet build -v q -nologo "$app" -o "$out/v1"
dotnet build -v q -nologo "$lib" -p:DefineConstants=V3 -o "$out/lib3"
cp "$out/lib3/OptionalProbeLibrary.dll" "$out/v1/"
dotnet "$out/v1/OptionalProbeApp.dll" fine 2>&1 \
    | grep -m1 -o "MissingMethodException"
