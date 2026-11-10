#!/bin/bash
# Builds a throwaway repository with 64 commits, one of which breaks a rule,
# and lets git bisect find it.
# Usage: bash bisect-demo.sh <empty-target-directory>.
# Never run it inside an existing repository.
set -e
quelle="$(cd "$(dirname "$0")" && pwd)"
ziel="$1"
mkdir -p "$ziel" && cd "$ziel"
git init -q -b main
git config user.name "Library Demo"
git config user.email "demo@example.invalid"
cp "$quelle/Kapitel05.csproj" "$quelle/LoanRules.cs" "$quelle/LoanRulesTests.cs" .
printf 'bin/\nobj/\n' > .gitignore
git add .
git commit -qm "Add loan rules with tests"
gut=$(git rev-parse HEAD)

for i in $(seq 1 63); do
    echo "Entry $i" >> CHANGELOG.txt
    if [ "$i" -eq 41 ]; then
        sed -i 's/GraceDays = 3/GraceDays = 4/' LoanRules.cs
        git add .
        git commit -qm "Tidy up loan rules"
    else
        git add .
        git commit -qm "Update changelog, entry $i"
    fi
done

git bisect start HEAD "$gut"
git bisect run bash "$quelle/bisect-check.sh" > bisect.log 2>&1 || true
grep -E "^Bisecting:" bisect.log
# Newer git writes "first 'bad' commit", older git "first bad commit".
sed -n "/is the first .\?bad.\? commit/,/files\? changed/p" bisect.log
git bisect log | grep -c "^git bisect \(good\|bad\)"
git bisect reset -q 2>/dev/null || git bisect reset
