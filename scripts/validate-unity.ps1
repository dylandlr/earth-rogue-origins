param(
  [Parameter(Mandatory=$true)][string]$UnityEditor,
  [switch]$SkipBuild
)
$ErrorActionPreference = 'Stop'
$project = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$out = Join-Path $project 'ValidationResults'
New-Item -ItemType Directory -Force -Path $out | Out-Null

if (-not (Test-Path $UnityEditor)) {
  throw "Unity Editor executable not found: $UnityEditor"
}

function Invoke-Unity([string]$name,[string[]]$arguments) {
  $log = Join-Path $out ($name + '.log')
  Write-Host "Running $name..."
  $all = @('-batchmode','-nographics','-quit','-projectPath',$project,'-logFile',$log) + $arguments
  & $UnityEditor @all
  $status = $LASTEXITCODE
  if ($status -ne 0) {
    Write-Host "Unity log: $log"
    if (Test-Path $log) { Get-Content $log -Tail 60 | Out-Host }
    throw "$name failed (exit code $status)"
  }
  Write-Host "$name passed. Log: $log"
}

Invoke-Unity 'scene-validation' @('-executeMethod','EarthRogueOrigins.Editor.ProjectValidation.GenerateAndValidate')
Invoke-Unity 'editmode-tests' @('-runTests','-testPlatform','EditMode','-testResults',(Join-Path $out 'editmode-results.xml'))
$results = Join-Path $out 'editmode-results.xml'
if (-not (Test-Path $results)) { throw "Unity did not produce test results XML: $results" }
[xml]$xml = Get-Content $results -Raw
$failures = $xml.SelectNodes("//*[@result='Failed' or @result='Error']")
if ($failures.Count -gt 0) { throw "Unity EditMode suite reports $($failures.Count) failed/error elements." }
Write-Host 'EditMode XML verification passed.'

if (-not $SkipBuild) {
  Invoke-Unity 'windows-build' @('-executeMethod','EarthRogueOrigins.Editor.ProjectSetup.BuildWindows')
}

Write-Host "Validation complete. Artifacts in $out"
