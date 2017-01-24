# LogFarm

A Diagnostics tool for .Net Application. Currently it contains NLog Target that writes to AWS CloudWatch and Elmah Repository to AWS DynamoDb

# Installation 

To add LogFarm to your Visual Studio project, run the following command in Package Manager Console

<div class="nuget-badge">
<p>
<code>PM&gt; Install-Package LogFarm.Mvc</code>
</p>
</div>

# Configuration
 
LogFarm by default, it is configured not to write to AWS, it's only going to be enabled when build set to release mode (Web.release.config).
For the reason to me most log events and exceptions is not going to be useful during local development. However a user can still choose enable it by following instructions


###Enable Elmah 

```xml
 <configuration>
  .
  .
  .
   <elmah>
     <errorLog type="ShadowBlue.LogFarm.Domain.Elmah.ElmahDynamoDbErrorLog, ShadowBlue.LogFarm.Domain"
       ddbAppName="ChangeMe" ddbTableName="elmah-dev" ddbEnvironment="local"  />
   </elmah>
 </configuration>
```
###Enable NLog CloudWatchLog

Inside the NLog.Config, specify "cloudwatchlog" in writeTo

```xml
 </nLog>
  .
  .
  .
  </targets>
  <rules>
    <logger name="*" minlevel="Info" writeTo="jsonFile,cloudwatchlog" />
  </rules>
 </nlog>
```

###Provision Resources requires for LogFarm 


Simple run #####Deploy-Infrastructure.ps1 powershell script at [base-infrastructure](https://github.com/imomou/LogFarm/tree/master/base-infrastructure"), make sure to grab Deployment.ps1 as well with template. And please create provide an existing S3 Bucket

