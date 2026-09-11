$ErrorActionPreference = 'Stop'
$projectRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../CatalogStudio'))
$testRoot = Join-Path $PSScriptRoot 'work/SmokeTests'
New-Item -ItemType Directory -Path $testRoot -Force | Out-Null
if (-not (Test-Path (Join-Path $testRoot 'SmokeTests.csproj'))) {
    dotnet new console -n SmokeTests -o $testRoot --framework net10.0
    if ($LASTEXITCODE -ne 0) { throw 'Test projesi oluşturulamadı.' }
    dotnet add $testRoot reference (Join-Path $projectRoot 'CatalogStudio.csproj')
    dotnet add $testRoot package Microsoft.AspNetCore.Mvc.Testing --version 10.0.9
    if ($LASTEXITCODE -ne 0) { throw 'Test bağımlılıkları yüklenemedi.' }
}
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'SmokeTests.cs.txt') -Destination (Join-Path $testRoot 'Program.cs')
Copy-Item -LiteralPath (Join-Path $PSScriptRoot '../CatalogStudio.sln') -Destination (Join-Path $PSScriptRoot 'work/CatalogStudio.sln')
$previousRoot = $env:CATALOG_APP_ROOT
try {
    $env:CATALOG_APP_ROOT = $projectRoot
    Push-Location $testRoot
    dotnet run -c Release
    if ($LASTEXITCODE -ne 0) { throw 'Doğrulama başarısız.' }
} finally {
    Pop-Location
    $env:CATALOG_APP_ROOT = $previousRoot
}
