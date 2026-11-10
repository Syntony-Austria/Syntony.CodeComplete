#!/bin/bash
# Shows merge against rebase, a secret that survives its deletion, and a rescue through the reflog.
# Usage: bash git-demo.sh <empty-target-directory>. Never run it inside an existing repository.
set -e
mkdir -p "$1" && cd "$1"
git init -q -b main
git config user.name "Library Demo"
git config user.email "demo@example.invalid"

commit() { echo "$1" >> notes.txt; git add .; git commit -qm "$1"; }

commit "Add renewal rule"
git switch -qc feature/reminders
commit "Add reminder text"
commit "Send reminder after two weeks"
git switch -q main
commit "Fix fee rounding"

git switch -qc try-merge feature/reminders
git merge -q --no-edit -X ours main
echo "--- merge"
git log --oneline --graph --format='%s' main feature/reminders try-merge

git switch -qc try-rebase feature/reminders
git rebase -q -X theirs main
echo "--- rebase"
git log --oneline --graph --format='%s' try-rebase

echo "--- secret"
git switch -q main
echo 'ApiKey=demo-not-a-real-key-1234' > settings.ini
git add . && git commit -qm "Add settings"
git rm -q settings.ini && git commit -qm "Remove settings again"
ls settings.ini 2>/dev/null || echo "no settings.ini in the working tree"
git log -S"demo-not-a-real-key" --format='%s'
git show HEAD~1:settings.ini

echo "--- reflog"
git reset -q --hard HEAD~2
git log --format='%s' -1
git reflog -3 --format='%gs'
git reset -q --hard 'HEAD@{1}'
git log --format='%s' -1

echo "--- churn"
git log --format= --name-only | sort | uniq -c | sort -rn | head -3
