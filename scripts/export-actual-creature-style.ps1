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
            $cmd.Replace('/','\').IndexOf($project,[StringComparison]::OrdinalIgnoreCase) -ge 0
        )
    })
if($otherEditors.Count -gt 0){
    $ids=($otherEditors | ForEach-Object { $_.ProcessId }) -join ', '
    throw "Unity is already running this project (PID(s): $ids). Close that Unity Editor window and retry the batch export."
}
$log=Join-Path $output 'unity-export.log'
# Give each export a unique destination to avoid mapped PNG file locks.
$runId='run-'+(Get-Date -Format 'yyyyMMdd-HHmmss-fff')+'-'+[System.Guid]::NewGuid().ToString('N').Substring(0,6)
$runOutput=Join-Path $output $runId
New-Item -ItemType Directory -Force -Path $runOutput | Out-Null
$sheet=Join-Path $runOutput 'actual_csharp_full_creatures.png'
$csv=Join-Path $runOutput 'visual_metrics.csv'
$env:ERO_CREATURE_EXPORT_DIR=$runOutput
# Previous log entries are not evidence of this Unity run.
Remove-Item -LiteralPath $log -ErrorAction SilentlyContinue
Write-Host "Launching Unity Editor: $UnityEditor"
Write-Host "Project: $project"
Write-Host "Log: $log"
Write-Host "New capture directory: $runOutput"
# Wait for the actual Unity GUI process to finish initialization.
$stdout=Join-Path $output 'unity-stdout.txt'
$stderr=Join-Path $output 'unity-stderr.txt'
Remove-Item -LiteralPath $stdout,$stderr -ErrorAction SilentlyContinue
$argsUnity=@(
    '-batchmode','-nographics','-quit',
    '-projectPath', ('"' + $project + '"'),
    '-logFile', ('"' + $log + '"'),
    '-executeMethod','EarthRogueOrigins.Editor.CreatureStyleContactSheet.Run'
)
$process=Start-Process -FilePath $UnityEditor -ArgumentList $argsUnity -Wait -PassThru `
    -RedirectStandardOutput $stdout -RedirectStandardError $stderr
$exitCode=$process.ExitCode
Write-Host "Unity process PID: $($process.Id) | Exit code: $exitCode"
if(-not (Test-Path -LiteralPath $log)){
    Write-Host "Unity did not write its Editor log. Startup stdout/stderr follow:"
    if(Test-Path $stdout){Get-Content -LiteralPath $stdout -Tail 60 | Out-Host}
    if(Test-Path $stderr){Get-Content -LiteralPath $stderr -Tail 60 | Out-Host}
    Write-Host "Other Unity processes:"
    Get-CimInstance Win32_Process -Filter "Name = 'Unity.exe'" -ErrorAction SilentlyContinue |
        Select-Object ProcessId,CommandLine | Format-List | Out-Host
    throw "Unity exited ($exitCode) without a fresh Editor log; the export method was not confirmed to run. Try opening the project via Unity Hub."
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
Write-Host "New capture directory: $runOutput"
Write-Host "Metrics: $csv"
Write-Host "This is a renderer asset capture, not an in-game scene screenshot."
