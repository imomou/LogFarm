param(
    $environment = (Read-Host 'Environment'),
    $ElmahTableName = (Read-Host 'Name of Elmah Table'),
    $MiniProfilerTableName = (Read-Host 'Name of MiniProfiler Table'),
    $Bucket = (Read-Host 'Name of MiniProfiler Table')

)

$build_number = 1

. .\Deployment.ps1

$tags = @(
    @{"Key" = "Project"; "Value" = "Forensics"},
    @{"Key" = "Environment"; "Value" = $environment}
)

$p1refix = New-Deployment -bucketname $Bucket -projectname "LogFarm" -version $build_number -deployroot .\assets

Get-StackLinkParameters -StackParameters @(
    @{"Key" = "ElmahTableName"; "Value" = $ElmahTableName},
    @{"Key" = "MiniProfilerTableName"; "Value" = $MiniProfilerTableName},
    @{"Key" = "IsSubscribed"; "Value" = "subscribe"}
) -TemplateUrl "$($p1refix)logfarm-base.template" |
    Upsert-StackLink -StackName "$environment-Forensics" -Tags $tags



