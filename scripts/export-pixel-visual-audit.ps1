param(
    [Parameter(Mandatory=$true)][string]$UnityEditor
)
$ErrorActionPreference='Stop'
$project=(Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$out=Join-Path $project 'ValidationResults/PixelVisualAudit'
New-Item -ItemType Directory -Force -Path $out | Out-Null
if(-not (Test-Path $UnityEditor)) { throw "Unity Editor not found: $UnityEditor" }
$log=Join-Path $out 'render.log'
& $UnityEditor -batchmode -nographics -quit -projectPath $project -logFile $log -executeMethod EarthRogueOrigins.Editor.PixelCreatureVisualAudit.Export
if($LASTEXITCODE -ne 0) {
    Get-Content $log -Tail 65 | Out-Host
    throw "Actual C# sprite review failed; exit code $LASTEXITCODE"
}
$png=Join-Path $out 'actual_unity_csharp_creatures.png'
$csv=Join-Path $out 'visibility.csv'
if(-not (Test-Path $png) -or -not (Test-Path $csv)) {
    throw "Missing output. Inspect log $log"
}
$rows=Import-Csv $csv
if($rows.Count -ne 5){throw "Expected five hero renders; got $($rows.Count)"}
foreach($row in $rows){
    if([int]$row.visible_pixels -lt 250) {throw "Nearly blank hero: $($row.archetype)"}
}
Write-Host "Unity C# asset review output: $png"
Write-Host "All five hero sprite visibility counts are nonzero."
