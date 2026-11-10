#!/bin/bash
dotnet build --nologo -v q || exit 125
dotnet test --no-build --nologo -v q
