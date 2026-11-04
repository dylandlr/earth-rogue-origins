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
# Remove stale files so a new run cannot accidentally pass using an older capture.
$sheet=Join-Path $output 'actual_csharp_full_creatures.png'
$csv=Join-Path $output 'visual_metrics.csv'
Remove-Item -LiteralPath $sheet,$csv -ErrorAction SilentlyContinue
Write-Host "Launching Unity Editor: $UnityEditor"
Write-Host "Project: $project"
Write-Host "Log: $log"
& $UnityEditor -batchmode -nographics -quit -projectPath $project -logFile $log -executeMethod EarthRogueOrigins.Editor.CreatureStyleContactSheet.Run
if($LASTEXITCODE -ne 0){
    Get-Content $log -Tail 90 | Out-Host
    throw "Unity style audit failed with exit code $LASTEXITCODE. Inspect $log"
}
if(-not (Test-Path $sheet) -or -not (Test-Path $csv)){
    Write-Host "Unity returned exit code $LASTEXITCODE but did not generate the expected outputs." -ForegroundColor Yellow
    if(Test-Path $log){
        Write-Host "===== Unity editor log (last 120 lines) ====="
        Get-Content -LiteralPath $log -Tail 120 | Out-Host
        Write-Host "===== Likely errors ====="
        Select-String -Path $log -Pattern 'error CS','error:','Exception','executeMethod','Compilation failed','Aborting','Assembly' |
            Select-Object -Last 25 | Out-Host
    } else {
        Write-Warning "Unity produced no editor log at the requested path."
    }
    throw "Unity audit missing generated PNG or CSV. See diagnostics above."
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
