param(
    [Parameter(Mandatory=$true)][string]$UnityEditor
)
$ErrorActionPreference='Stop'
$project=(Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$out=Join-Path $project 'ValidationResults/FaceContour'
New-Item -ItemType Directory -Force -Path $out | Out-Null
if(-not (Test-Path $UnityEditor)){throw "Unity Editor executable not found: $UnityEditor"}
$log=Join-Path $out 'face-audit.log'
& $UnityEditor -batchmode -nographics -quit -projectPath $project -logFile $log -executeMethod EarthRogueOrigins.Editor.FaceContourAudit.Run
if($LASTEXITCODE -ne 0){
    Get-Content $log -Tail 75 | Out-Host
    throw "Unity face contour audit failed: exit $LASTEXITCODE"
}
$csv=Join-Path $out 'face_landmarks.csv'
$png=Join-Path $out 'csharp_face_contours.png'
if(-not (Test-Path $csv) -or -not (Test-Path $png)){
    throw "Missing audit outputs: $csv or $png"
}
$rows=Import-Csv $csv
if($rows.Count -ne 12){throw "Expected 12 head morphs; got $($rows.Count)"}
foreach($r in $rows){
    if($r.landmarks_valid -ne 'True' -or $r.eye_ink_visible -ne 'True'){
        throw "Face failed: $($r.style) / $($r.head_size)"
    }
}
Write-Host "PASS: 12 authoritative C# head morphs checked; review image: $png"
