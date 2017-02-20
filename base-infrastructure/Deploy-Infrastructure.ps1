param(
	$environment = (Read-Host 'Environment'),
	$ElmahTableName = (Read-Host 'Name of Elmah Table'),
    $NLogGroupName = (Read-Host 'Name of NLogGroup'),
	$Bucket = (Read-Host 'Name of the S3 bucket to store template'),
	$ProjectName = (Read-Host 'Name of the Project')
)

$build_number = 1

#Thanks to AWS Lego https://github.com/SleeperSmith/Aws-Lego

. .\Deployment.ps1

$tags = @(
    @{"Key" = "Project"; "Value" = $ProjectName},
    @{"Key" = "Environment"; "Value" = $environment}
)

$p1refix = New-Deployment -bucketname $Bucket -projectname $ProjectName -version $build_number -deployroot .\assets

Get-StackLinkParameters -StackParameters @(
    @{"Key" = "ElmahTableName"; "Value" = $ElmahTableName},
    @{"Key" = "NLogGroupName"; "Value" = $NLogGroupName},
    @{"Key" = "IsSubscribed"; "Value" = "subscribe"}    
) -TemplateUrl "$($p1refix)logfarm-base.template" 
    Upsert-StackLink -StackName "$environment-LogFarm" -Tags $tags



  