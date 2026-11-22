
param([int]$Count = 3)

$ErrorActionPreference = 'Stop'

if ($Count -lt 1) {
    throw 'Count must be >= 1'
}

New-Item -ItemType Directory -Force `
    -Path 'results/raw/campaigns' | Out-Null

for ($i = 1; $i -le $Count; $i++) {
    Write-Host "Campaign $i of $Count"

    dotnet run --project benchmarks/StackVm.Benchmarks `
        -c Release --no-launch-profile

    if ($LASTEXITCODE -ne 0) {
        throw "Benchmark failed on campaign $i"
    }

    $csv = 'BenchmarkDotNet.Artifacts/results/' +
        'StackVm.Benchmarks.VirtualMachineBenchmarks-report.csv'

    if (-not (Test-Path $csv)) {
        throw "Missing BenchmarkDotNet CSV: $csv"
    }

    Copy-Item $csv (
        "results/raw/campaigns/campaign_{0:D2}.csv" -f $i
    )
}
