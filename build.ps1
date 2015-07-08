$cwd = "C:\Users\Brian\Business\Projects\Kurouzu\"

if (Test-Path("$cwd\Source\logs")) {
    Remove-Item -Recurse -Path "$cwd\Source\logs" -Force
}
if (Test-Path("$cwd\Source\Assets")) {
    Remove-Item -Recurse -Path "$cwd\Source\Assets" -Force
}
$csvs = @();
Get-ChildItem "$cwd\Source\data\*\*\" -Recurse -Filter *.csv | % {
    $game = $_.Directory.Name;
    $self = $_.Name;
    Copy-Item -Path $_.FullName -Destination "$cwd\Source\data\$game $self" -Force;
    $csvs += "/res:data\$game $self";
}
          # /win32res:"$cwd\gga\GGa\Release\x64\GGa.res" `
$build = (&"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe" `
          /win32icon:"..\Graphics\icon.ico" `
          $csvs `
          /optimize `
          /platform:x64 `
          /target:exe `
          /out:kurouzu.exe `
          bin\*.cs
)
if ($build.Count -gt 5){ $build[5..$build.Count] | foreach { Write-Host $_ -ForegroundColor Red; } } else { clear }

Get-ChildItem -File "$cwd\Source\data\" -Filter *.csv | Remove-Item -Force
