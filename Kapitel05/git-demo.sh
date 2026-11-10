#!/bin/bash
# Shows merge against rebase, a secret that survives its deletion, a rescue
# through the reflog, a release tag, and a merge without conflict that breaks
# the build. Needs git and the .NET SDK.
# Usage: bash git-demo.sh <empty-target-directory>.
# Never run it inside an existing repository.
set -e
quelle="$(cd "$(dirname "$0")" && pwd)"
mkdir -p "$1" && cd "$1"
git init -q -b main
git config user.name "Library Demo"
git config user.email "demo@example.invalid"

# Each commit writes its own file, so merge and rebase run without conflict.
commit() {
    echo "$1" > "$(echo "$1" | tr ' ' '_').txt"
    git add . && git commit -qm "$1"
}

commit "Add renewal rule"
git switch -qc feature/reminders
commit "Add reminder text"
commit "Send reminder after two weeks"
git switch -q main
commit "Fix fee rounding"

git switch -qc try-merge feature/reminders
git merge -q --no-edit main
echo "--- merge"
git log --oneline --graph --format='%s' main feature/reminders try-merge

git switch -qc try-rebase feature/reminders
git rebase -q main
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

echo "--- tag"
git tag -a v1.2.0 -m "Release 1.2.0"
cp "$quelle/Kapitel05.csproj" "$quelle/LoanRules.cs" "$quelle/LoanRulesTests.cs" .
printf 'bin/
obj/
' > .gitignore
git add . && git commit -qm "Add loan rules with tests"
git describe
git log v1.2.0..HEAD --oneline --format='%s'

echo "--- semantic merge"
git switch -qc feature/rename
sed -i 's/FeeInCents/LateFeeInCents/' LoanRules.cs LoanRulesTests.cs
git commit -qam "Rename FeeInCents to LateFeeInCents"
git switch -q main
cat > Reminder.cs <<'CS'
namespace Library.Lending;

public static class Reminder
{
    public static string Text(int daysLate) =>
        $"Please pay {LoanRules.FeeInCents(daysLate)} cents.";
}
CS
git add . && git commit -qm "Add reminder text with the fee"
git merge --no-edit feature/rename | tail -1
git status --short
dotnet build --nologo -v q 2>&1 | grep -m1 -o "Reminder.cs.*CS0117[^[]*" || true
