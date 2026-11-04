param(
    [Parameter(Mandatory=$true)][string]$UnityEditor
)
$ErrorActionPreference='Stop'
$project=(Resolve-Path (Join-Path $PSScriptRoot '..')).Path
if(-not (Test-Path -LiteralPath $UnityEditor)){
    throw "Unity Editor executable not found: $UnityEditor"
}
$root=Join-Path $project 'ValidationResults/OpenTissue'
New-Item -Path $root -ItemType Directory -Force | Out-Null
$log=Join-Path $root 'unity-tissue-export.log'
Remove-Item -LiteralPath $log -ErrorAction SilentlyContinue
$old=@(Get-CimInstance Win32_Process -Filter "Name = 'Unity.exe'" -ErrorAction SilentlyContinue |
    Where-Object {
        $_.CommandLine -and
        $_.CommandLine.Replace('/','\').IndexOf($project,
            [StringComparison]::OrdinalIgnoreCase) -ge 0
    })
if($old.Count -gt 0){
    throw "Close the existing Unity Editor for this project before batch exporting."
}
$arguments=@(
    '-batchmode','-nographics','-quit',
    '-projectPath', ('"' + $project + '"'),
    '-logFile', ('"' + $log + '"'),
    '-executeMethod','EarthRogueOrigins.Editor.OpenMorphTissueAudit.Run'
)
$startedUtc=[DateTime]::UtcNow
$before=@(Get-ChildItem -LiteralPath $root -Directory -Filter 'run-*' -ErrorAction SilentlyContinue |
    ForEach-Object { $_.FullName })
Write-Host "Starting actual Unity tissue simulation..."
$process=Start-Process -FilePath $UnityEditor -ArgumentList $arguments -Wait -PassThru
if($process.ExitCode -ne 0){
    if(Test-Path $log){Get-Content -LiteralPath $log -Tail 65}
    throw "Unity tissue export failed (exit code $($process.ExitCode))."
}
$latest=Get-ChildItem -LiteralPath $root -Directory -Filter 'run-*' |
    Where-Object {
        $_.FullName -notin $before -and
        $_.CreationTimeUtc -ge $startedUtc.AddSeconds(-3)
    } |
    Sort-Object CreationTimeUtc -Descending | Select-Object -First 1
if(-not $latest){
    throw "Unity exited but no NEW tissue audit directory was created during this invocation. Check $log"
}
$required=@(
    'actual_tissue_growth.png',
    'tissue_metrics.csv',
    'grn_growth_trace.csv',
    'grn_growth_outcomes.csv'
)
foreach($filename in $required){
    $path=Join-Path $latest.FullName $filename
    if(-not (Test-Path -LiteralPath $path)){
        throw "New Unity run is incomplete: missing $filename in $($latest.FullName)"
    }
}
$outcomes=Join-Path $latest.FullName 'grn_growth_outcomes.csv'
$trace=Join-Path $latest.FullName 'grn_growth_trace.csv'
$expectedOutcomes=@(
    'static_total_eligible',
    'recurrent_total_eligible',
    'recurrent_inhibited_sites',
    'recurrent_boosted_sites',
    'recurrent_mean_final_chance'
)
$expectedTrace=@(
    'eligible_sites',
    'mean_static_chance',
    'mean_final_chance',
    'inhibited_sites',
    'boosted_sites'
)
$outcomeHeader=Get-Content -LiteralPath $outcomes -TotalCount 1
$traceHeader=Get-Content -LiteralPath $trace -TotalCount 1
foreach($field in $expectedOutcomes){
    if($field -notin ($outcomeHeader -split ',')){
        throw "Stale GRN outcomes schema: missing '$field'. New run: $($latest.FullName)"
    }
}
foreach($field in $expectedTrace){
    if($field -notin ($traceHeader -split ',')){
        throw "Stale GRN trace schema: missing '$field'. New run: $($latest.FullName)"
    }
}
$png=Join-Path $latest.FullName 'actual_tissue_growth.png'
$csv=Join-Path $latest.FullName 'tissue_metrics.csv'
Write-Host "Verified current run: $($latest.FullName)"
Write-Host "Verified GRN diagnostic schema: v1.3"
Write-Host "Upload: $outcomes"
Write-Host "Upload: $trace"
Write-Host "Actual C# tissue growth image: $png"
Write-Host "Cell fate metrics: $csv"
Write-Host "This is experimental cellular morphology, NOT viable evolved organisms."
