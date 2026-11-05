param(
    [Parameter(Mandatory=$true)][string]$UnityEditor
)
$ErrorActionPreference='Stop'
$project=(Resolve-Path (Join-Path $PSScriptRoot '..')).Path
if(-not (Test-Path -LiteralPath $UnityEditor)){
    throw "Unity Editor executable not found: $UnityEditor"
}
$root=Join-Path $project 'ValidationResults/Emergent'
New-Item -Path $root -ItemType Directory -Force | Out-Null
$log=Join-Path $root 'unity-emergent.log'
$before=@(Get-ChildItem -LiteralPath $root -Directory -Filter 'run-*' -ErrorAction SilentlyContinue |
    ForEach-Object { $_.FullName })
$start=[DateTime]::UtcNow
Write-Host "Launching Unity for the integrated emergent-creature experiment..."
$arguments=@(
    '-batchmode','-nographics','-quit',
    '-projectPath', ('"' + $project + '"'),
    '-logFile', ('"' + $log + '"'),
    '-executeMethod','EarthRogueOrigins.Editor.OpenMorphEvolutionAudit.Run'
)
$process=Start-Process -FilePath $UnityEditor -ArgumentList $arguments -Wait -PassThru
if($process.ExitCode -ne 0){
    if(Test-Path -LiteralPath $log){Get-Content -LiteralPath $log -Tail 70}
    throw "Unity emergent-creature experiment failed with exit code $($process.ExitCode)"
}
$latest=Get-ChildItem -LiteralPath $root -Directory -Filter 'run-*' |
    Where-Object {
        $_.FullName -notin $before -and
        $_.CreationTimeUtc -ge $start.AddSeconds(-3)
    } | Sort-Object CreationTimeUtc -Descending | Select-Object -First 1
if(-not $latest){
    throw "Unity exited but produced no new emergent experiment directory. Inspect $log"
}
foreach($file in @('actual_emergent_generation.png','actual_emergent_structures.png','actual_expression_alignment.png','branch_expression.csv','causal_branch_ablation.csv','population.csv','champions.csv','provenance.txt')){
    if(-not (Test-Path -LiteralPath (Join-Path $latest.FullName $file))){
        throw "Incomplete emergent experiment: missing $file in $($latest.FullName)"
    }
}
$population=Import-Csv (Join-Path $latest.FullName 'population.csv')
if($population.Count -ne 32){
    throw "Expected 4 generations x 8 individuals; got $($population.Count)"
}
$expected=@('novelty','selection_score','guide_coverage','supported_tissue_cells','expressed_branches','unexpressed_branches','causal_branches','strongest_branch_effect')
$actualHeader=Get-Content -LiteralPath (Join-Path $latest.FullName 'population.csv') -TotalCount 1
foreach($column in $expected){
    if($column -notin ($actualHeader -split ',')){
        throw "Missing v2.1 population column '$column' in current Unity result"
    }
}
$genomes=@(Get-ChildItem -LiteralPath $latest.FullName -Filter 'genome_*.json')
if($genomes.Count -ne 8){
    throw "Expected 8 exported final-genome JSON records; got $($genomes.Count)"
}
Write-Host "Verified real Unity experiment: $($latest.FullName)"
Write-Host "Upload: $(Join-Path $latest.FullName 'actual_emergent_generation.png')"
Write-Host "Upload: $(Join-Path $latest.FullName 'actual_emergent_structures.png')"
Write-Host "Upload: $(Join-Path $latest.FullName 'actual_expression_alignment.png')"
Write-Host "Upload: $(Join-Path $latest.FullName 'branch_expression.csv')"
Write-Host "Upload: $(Join-Path $latest.FullName 'causal_branch_ablation.csv')"
Write-Host "Upload: $(Join-Path $latest.FullName 'population.csv')"
Write-Host "Upload: $(Join-Path $latest.FullName 'champions.csv')"
Write-Host "These are experimental 2D tissue morphologies, not finished animated creatures."
