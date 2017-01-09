$AssemblyVersion = 1.0.0.0
$ArtefactPath = "CiArtefact"

properties {
    $NugetBinPath = ".\.nuget\NuGet.exe"
}
Task New-NugetPackagesFromSpecFilesProp {
	$command = "pack"

    Pop-Location

    Write-Host "> Packing nuget packages with version number $AssemblyVersion"

    $nuspecFiles = Get-ChildItem *.nuspec -Recurse | ? {
        !($_.FullName.Contains("\packages\"))
    }

    foreach($nuspecFile in $nuspecFiles) {
        Write-Host "> Packing $($nuspecFile.FullName)"
        &$NugetBinPath $command $nuspecFile.FullName -Pro -Version="$AssemblyVersion"
    }

    Write-Host "> Moving nuget packages to artefact path"
    Get-ChildItem -Path .\ -Filter *.nupkg | % {
        Copy-Item -Path $_.FullName -Destination $ArtefactPath
    }
}