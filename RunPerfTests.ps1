Push-Location $PSScriptRoot

if (-not (Test-Path "artifacts")) {
    Remove-Item .\artifacts -Force -Recurse
    New-Item -Path $directoryPath -ItemType Directory
}

foreach ($test in Get-ChildItem "$PSScriptRoot/test" -Filter "*.PerformanceTests" -Directory) {
    Push-Location $test.FullName

    echo "perf: Running performance test project in $test"

    & dotnet test -c Release
    if($LASTEXITCODE -ne 0) { exit 2 }

    cp ".\bin\Release\net6.0\BenchmarkDotNet.Artifacts\results\*.*" "$PSScriptRoot\artifacts\"

    Pop-Location
}

Pop-Location
