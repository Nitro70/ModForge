# Builds ModForge and puts the single exe in bin\publish. Fails if any template does not validate.
$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot

dotnet build src\ModForge\ModForge.csproj -c Release -nologo -v q
if ($LASTEXITCODE -ne 0) { throw "build failed" }

$exe = "src\ModForge\bin\Release\ModForge.exe"
$report = & $exe --cli validate | Out-String
Write-Host $report
if ($LASTEXITCODE -ne 0) { throw "a template failed validation" }

New-Item -ItemType Directory -Force bin\publish | Out-Null
Copy-Item $exe bin\publish\ModForge.exe -Force
"{0}  {1:N0} KB" -f (Resolve-Path bin\publish\ModForge.exe), ((Get-Item bin\publish\ModForge.exe).Length / 1KB)
