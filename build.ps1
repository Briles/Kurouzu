$cwd = "./"

if (Test-Path("$cwd\logs")) {
    Remove-Item -Recurse -Path "$cwd\logs" -Force
}
if (Test-Path("$cwd\Assets")) {
    Remove-Item -Recurse -Path "$cwd\Assets" -Force
}
$csvs = @();
Get-ChildItem "$cwd\data\*\*\" -Recurse -Filter *.csv | % {
    $game = $_.Directory.Name;
    $self = $_.Name;
    Copy-Item -Path $_.FullName -Destination "$cwd\data\$game $self" -Force;
    $csvs += "/res:data\$game $self";
}

$build = (&"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe" `
          /win32icon:"Graphics\icon.ico" `
          $csvs `
          /optimize `
          /platform:x64 `
          /target:exe `
          /out:kurouzu.exe `
          src\*.cs
)
if ($build.Count -gt 5){ $build[5..$build.Count] | foreach { Write-Host $_ -ForegroundColor Red; } } else { clear }

Get-ChildItem -File "$cwd\data\" -Filter *.csv | Remove-Item -Force
