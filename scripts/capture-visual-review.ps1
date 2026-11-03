param(
    [Parameter(Mandatory=$true)][string]$UnityEditor
)
$ErrorActionPreference='Stop'
$project=(Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$out=Join-Path $project 'ValidationResults/VisualReview'
New-Item -ItemType Directory -Force -Path $out | Out-Null
if(-not (Test-Path $UnityEditor)) { throw "Unity Editor not found: $UnityEditor" }
$log=Join-Path $out 'capture.log'
# Camera.Render requires graphics: do not use -nographics.
& $UnityEditor -batchmode -quit -projectPath $project -logFile $log -executeMethod EarthRogueOrigins.Editor.MorphologyReviewBuilder.BuildReview
if($LASTEXITCODE -ne 0) {
    Get-Content $log -Tail 70 | Out-Host
    throw "Morphology review failed; Unity exit code $LASTEXITCODE"
}
$png=Join-Path $out 'morphology_board.png'
$csv=Join-Path $out 'morphology_metrics.csv'
if(-not (Test-Path $png) -or -not (Test-Path $csv)) {
    throw "Morphology review did not produce both PNG and CSV. Check $log"
}
$bytes=[IO.File]::ReadAllBytes($png)
if($bytes.Length -lt 1000) { throw "Visual review PNG unexpectedly small: $($bytes.Length) bytes" }
$signature=[byte[]](137,80,78,71,13,10,26,10)
for($i=0;$i -lt 8;$i++) {
    if($bytes[$i] -ne $signature[$i]) { throw 'Invalid PNG signature' }
}
$width=($bytes[16] -shl 24) -bor ($bytes[17] -shl 16) -bor ($bytes[18] -shl 8) -bor $bytes[19]
$height=($bytes[20] -shl 24) -bor ($bytes[21] -shl 16) -bor ($bytes[22] -shl 8) -bor $bytes[23]
if($width -ne 1600 -or $height -ne 1000) { throw "Unexpected image resolution $width x $height" }
$rows=Import-Csv $csv
if($rows.Count -ne 20) { throw "Expected 20 variants in CSV, got $($rows.Count)" }
Write-Host "Visual review captured: $png"
Write-Host "Phenotype metrics: $csv"
Write-Host "Dimensions: $width x $height; variants: $($rows.Count)"
