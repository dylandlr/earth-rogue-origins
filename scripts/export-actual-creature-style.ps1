param(
    [Parameter(Mandatory=$true)][string]$UnityEditor
)
$ErrorActionPreference='Stop'
$project=(Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$output=Join-Path $project 'ValidationResults/CreatureStyle'
New-Item -ItemType Directory -Force -Path $output | Out-Null
if(-not (Test-Path $UnityEditor)){
    throw "Unity Editor not found at $UnityEditor"
}
$log=Join-Path $output 'unity-export.log'
& $UnityEditor -batchmode -nographics -quit -projectPath $project -logFile $log -executeMethod EarthRogueOrigins.Editor.CreatureStyleContactSheet.Run
if($LASTEXITCODE -ne 0){
    Get-Content $log -Tail 90 | Out-Host
    throw "Unity style audit failed with exit code $LASTEXITCODE. Inspect $log"
}
$sheet=Join-Path $output 'actual_csharp_full_creatures.png'
$csv=Join-Path $output 'visual_metrics.csv'
if(-not (Test-Path $sheet) -or -not (Test-Path $csv)){
    throw "Unity audit missing sheet or metrics. Inspect $log"
}
$rows=@(Import-Csv $csv)
if($rows.Count -ne 8){
    throw "Expected 8 archetype metrics rows, got $($rows.Count)"
}
foreach($row in $rows){
    if([int]$row.visible_pixels -lt 150){
        throw "Nearly blank creature: $($row.archetype)"
    }
}
Write-Host "Actual C# full-creature sheet: $sheet"
Write-Host "Metrics: $csv"
Write-Host "This is a renderer asset capture, not an in-game scene screenshot."
