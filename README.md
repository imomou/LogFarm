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
 
LogFarm by default, it is configured not to write to AWS, it's only going to be enabled when build set to release mode (Web.release.config). For the reason, to me most log events and exceptions is not going to be useful during local development. However user can still choose to enable it by following instructions


###Enable Elmah 

Inside Web.config, put below errorLog inside the elmah section

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
 <nLog>
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


Simply run Deploy-Infrastructure.ps1 powershell script at [base-infrastructure](https://github.com/imomou/LogFarm/tree/master/base-infrastructure"), make sure to grab Deployment.ps1 as well with template. And please provide an existing S3 Bucket ( ATM, it doesn't make sense for LogFarm to have a bucket entirely for itself ) 

###Deployments

Logfarm use Kaiseki for CI/CD, it provides very consistent manner dealing with build,test and packaging. I strongely recommend using it along with LogFarm. Furthermore, by default it provided a lot of configuration required for dealing with CI/CD, so all you and the team need to is simply run build.ps1 and then deploy with running WebDeploy.ps1 and specify which config.xml to use e.g WebDeploy.ps1 ... env Site.SetParameters.Prod

[Kaiseki](https://github.com/SleeperSmith/Kaiseki)

###LogFarm TODO

* Profiling, MiniProfiler, Glimpse (still evaluating )
* Support for .Net Core
* Intergration with Kinsis ( AWS Lambda or Docker )


