$BUILD_NUMBER = 1

Write-Host "######################"
Write-Host "## Starting Kaiseki ##"
Write-Host "######################"
Write-Host

Write-Host "== Restoring packages =="
.".\.nuget\NuGet.exe" restore

.\packages\psake.4.4.2\tools\psake.ps1 .\kaiseki-bootstrap\custombuildfile.ps1 # -properties @{"MsbVsVersion"="14"}
#Invoke-psake .\kaiseki-bootstrap\New-NugetPackagesFromSpecFilesProp.ps1 -taskList New-NugetPackagesFromSpecFilesProp

Write-Host
Write-Host "######################"
Write-Host "## Kaiseki Finished ##"
Write-Host "######################"
