$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$fixtures = [ordered]@{
    "metaschema-test-cases.jsonl" = [ordered]@{
        1 = @($true, $false)
        2 = @($true, $false)
        3 = @($true, $false)
    }
    "vocabulary-test-cases.jsonl" = [ordered]@{
        1 = @($false, $true, $true)
        2 = @($false, $true)
    }
    "format-test-cases.jsonl" = [ordered]@{
        1 = @($true, $false)
        2 = @($true, $true)
        3 = @($true, $true)
        4 = @($true, $false)
    }
}

$project = Join-Path $PSScriptRoot "dotnet-lateapexearlyspeed-jsonschema.csproj"
$checked = 0

foreach ($fixture in $fixtures.GetEnumerator()) {
    $inputPath = Join-Path $PSScriptRoot $fixture.Key
    $output = @(& dotnet run --project $project --no-build --configuration Release -- $inputPath)
    if ($LASTEXITCODE -ne 0) {
        throw "$($fixture.Key): harness exited with code $LASTEXITCODE."
    }

    $responses = @($output | ForEach-Object { $_ | ConvertFrom-Json })
    $commands = @(Get-Content -LiteralPath $inputPath | ForEach-Object { $_ | ConvertFrom-Json })
    if ($responses.Count -ne $commands.Count - 1) {
        throw "$($fixture.Key): expected one response per command except stop."
    }

    $runs = @($responses | Where-Object { $_.PSObject.Properties.Name -contains "seq" })
    $expectedRuns = @($fixture.Value.GetEnumerator())
    if ($runs.Count -ne $expectedRuns.Count) {
        throw "$($fixture.Key): expected $($expectedRuns.Count) run responses, got $($runs.Count)."
    }

    for ($runIndex = 0; $runIndex -lt $expectedRuns.Count; $runIndex++) {
        $response = $runs[$runIndex]
        $expected = $expectedRuns[$runIndex]
        $label = "$($fixture.Key), seq $($expected.Key)"

        if (($response.PSObject.Properties.Name | Sort-Object) -join "," -ne "results,seq") {
            throw "${label}: expected only seq and results, got $($response | ConvertTo-Json -Depth 10 -Compress)."
        }
        if ($response.seq -ne $expected.Key) {
            throw "${label}: got sequence $($response.seq)."
        }
        if (@($response.results).Count -ne $expected.Value.Count) {
            throw "${label}: expected $($expected.Value.Count) test results, got $(@($response.results).Count)."
        }

        for ($testIndex = 0; $testIndex -lt $expected.Value.Count; $testIndex++) {
            $result = $response.results[$testIndex]
            if (@($result.PSObject.Properties).Count -ne 1 -or $result.PSObject.Properties.Name -ne "valid") {
                throw "${label}, test ${testIndex}: expected only valid, got $($result | ConvertTo-Json -Depth 10 -Compress)."
            }
            if ($result.valid -isnot [bool] -or $result.valid -ne $expected.Value[$testIndex]) {
                throw "${label}, test ${testIndex}: expected $($expected.Value[$testIndex]), got $($result.valid)."
            }
            $checked++
        }
    }
    Write-Output "$($fixture.Key): passed."
}

Write-Output "All $checked fixture checks passed."
