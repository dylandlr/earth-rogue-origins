param(
    [Parameter(Mandatory=$true)][string]$UnityEditor
)
$ErrorActionPreference='Stop'
$project=(Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$output=Join-Path $project 'ValidationResults/CreatureStyle'
New-Item -ItemType Directory -Force -Path $output | Out-Null
if(-not (Test-Path -LiteralPath $UnityEditor)){
    throw "Unity Editor not found at $UnityEditor"
}
# Unity may silently refuse to launch a second Editor instance for the
# same project. Check first rather than presenting an older compile log.
$otherEditors=@(Get-CimInstance Win32_Process -Filter "Name = 'Unity.exe'" -ErrorAction SilentlyContinue |
    Where-Object {
        $cmd=$_.CommandLine
        $cmd -and (
            $cmd.Replace('/','\\').IndexOf($project,[StringComparison]::OrdinalIgnoreCase) -ge 0
        )
    })
if($otherEditors.Count -gt 0){
    $ids=($otherEditors | ForEach-Object { $_.ProcessId }) -join ', '
    throw "Unity is already running this project (PID(s): $ids). Close that Unity Editor window and retry the batch export."
}
$log=Join-Path $output 'unity-export.log'
$sheet=Join-Path $output 'actual_csharp_full_creatures.png'
$csv=Join-Path $output 'visual_metrics.csv'
# A previous run's logfile is not evidence that this invocation compiled.
# Remove it to make a new Editor launch provable.
Remove-Item -LiteralPath $log,$sheet,$csv -ErrorAction SilentlyContinue
Write-Host "Launching Unity Editor: $UnityEditor"
Write-Host "Project: $project"
Write-Host "Log: $log"
& $UnityEditor -batchmode -nographics -quit -projectPath $project -logFile $log -executeMethod EarthRogueOrigins.Editor.CreatureStyleContactSheet.Run
$exitCode=$LASTEXITCODE
if(-not (Test-Path -LiteralPath $log)){
    throw "Unity exited ($exitCode) without creating a fresh log. Close the GUI Editor for this project and rerun."
}
if($exitCode -ne 0 -or -not (Test-Path -LiteralPath $sheet) -or -not (Test-Path -LiteralPath $csv)){
    Write-Host "Unity exit code: $exitCode"
    Write-Host "===== Fresh Unity log, last 60 lines ====="
    Get-Content -LiteralPath $log -Tail 60 | Out-Host
    Write-Host "===== Compiler errors and failures ====="
    Select-String -Path $log -Pattern 'error CS[0-9]+','Scripts have compiler errors','Exception','Aborting','Executing method','executeMethod' |
        Select-Object -Last 30 | Out-Host
    throw "Actual creature export failed. Only the fresh log above applies."
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
