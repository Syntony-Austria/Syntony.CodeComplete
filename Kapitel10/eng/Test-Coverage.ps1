# Runs the tests with coverage and compares line coverage with the recorded floor.
# Exit codes: 0 fine, 1 tests failed, 2 coverage fell below the floor.
$invariant = [Globalization.CultureInfo]::InvariantCulture
$floorFile = Join-Path $PSScriptRoot 'coverage-floor.txt'
$results = Join-Path ([IO.Path]::GetTempPath()) "coverage-$PID"

dotnet test tests --nologo -v q `
    --collect:"XPlat Code Coverage" --results-directory $results
if ($LASTEXITCODE -ne 0) { exit 1 }

$report = Get-ChildItem $results -Recurse -Filter coverage.cobertura.xml |
    Select-Object -First 1
[xml]$xml = Get-Content $report.FullName
$lineRate = [double]::Parse($xml.coverage.'line-rate', $invariant)
$rate = [math]::Round($lineRate * 100, 1)
Remove-Item $results -Recurse -Force

$floor = [double]::Parse((Get-Content $floorFile), $invariant)

if ($rate -lt $floor) {
    Write-Error "Line coverage $rate % is below the floor of $floor %."
    exit 2
}
if ($rate -gt $floor) {
    Set-Content $floorFile $rate.ToString($invariant)
    Write-Output "Coverage $rate %. Floor raised from $floor %, commit the file."
} else {
    Write-Output "Line coverage $rate %, floor $floor %."
}
