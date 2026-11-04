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
Write-Host "Starting actual Unity tissue simulation..."
$process=Start-Process -FilePath $UnityEditor -ArgumentList $arguments -Wait -PassThru
if($process.ExitCode -ne 0){
    if(Test-Path $log){Get-Content -LiteralPath $log -Tail 65}
    throw "Unity tissue export failed (exit code $($process.ExitCode))."
}
$latest=Get-ChildItem $root -Directory -Filter 'run-*' |
    Sort-Object LastWriteTime -Descending | Select-Object -First 1
if(-not $latest){throw "Unity finished but created no tissue audit directory."}
$png=Join-Path $latest.FullName 'actual_tissue_growth.png'
$csv=Join-Path $latest.FullName 'tissue_metrics.csv'
if(-not (Test-Path $png) -or -not (Test-Path $csv)){
    throw "Unity exited but did not write both expected audit files."
}
Write-Host "Actual C# tissue growth image: $png"
Write-Host "Cell fate metrics: $csv"
Write-Host "This is experimental cellular morphology, NOT viable evolved organisms."
