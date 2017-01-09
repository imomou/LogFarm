Write-Host "## Loading Modules ##"

cd $psake.context.originalDirectory

Get-ChildItem .\packages\Kaiseki.1.1.1\Tools\Tasks-*.ps1 -Recurse -File | % {
    Write-Host Loading $_.Name
    Include $_
}

Get-ChildItem .\kaiseki-bootstrap\New-NugetPackagesFromSpecFilesProp.ps1 | % {
    Write-Host Loading $_.Name
    Include $_
}
    
#Global properties
properties {
    $OutputPath = "CiOutput"
    $ArtefactPath = "CiArtefact"
    # Hard code for now.
    $AssemblyVersion = "1.0.0.0"
}

Task Default -depends Clean,New-CsvOutputCollection,New-CiOutFolder,Transform-InjectBuildInfo,
    Execute-PreBuildAnalysis,Execute-MsBuild,Execute-PostBuildAnalysis,
    Execute-Nunit,Execute-ReportGenerator,
    New-NugetPackagesFromSpecFilesProp,Write-CsvOutputCollection,
    Copy-Nunit,Copy-TestAssemblies,Copy-KaisekiModules
    

Write-Host "## Loading Modules > Done ##"
Write-Host
Write-Host