
param(
    [int]$Count = 3
)

$ErrorActionPreference = 'Stop'

if ($Count -lt 1) {
    throw 'Count must be at least 1.'
}

$project = 'benchmarks/StackVm.Benchmarks'
$output = 'results/raw/call_mixed'
$artifact = 'BenchmarkDotNet.Artifacts/results'
$report = 'StackVm.Benchmarks.CallAndMixedBenchmarks-report.csv'

New-Item -ItemType Directory -Force -Path $output |
    Out-Null

for ($i = 1; $i -le $Count; $i++) {
    Write-Host ""
    Write-Host "Running Calls/Mixed campaign $i of $Count"

    $destination = Join-Path $output (
        'call_mixed_run_{0:D2}.csv' -f $i
    )

    $source = Join-Path $artifact $report

    if (Test-Path $source) {
        Remove-Item $source
    }

    dotnet run --project $project -c Release -- `
        --filter '*CallAndMixedBenchmarks*'

    if ($LASTEXITCODE -ne 0) {
        throw "Benchmark failed during campaign $i."
    }

    if (-not (Test-Path $source)) {
        throw "Expected CSV was not generated: $source"
    }

    Copy-Item $source $destination

    Write-Host "Saved: $destination"
}

Write-Host ""
Write-Host "All campaigns completed."
