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
 
LogFarm by default, it is configured not to write to AWS, it's only going to be enabled when build with release mode, which trigger Web.release.config transformation that contains LogFarm references. For the reason to me most log events and exceptions often not useful during local development. However user can still choose to enable it by following instructions


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

And provide configuration parameter in NLog.Params.Config

```xml

<?xml version="1.0" encoding="utf-8" ?>
<nlog>
  <variable name="Environment"
            value="Local" />
  <variable name="ApplicationName"
            value="" />
  <variable name="LogPath"
            value="${basedir}/App_Data/nlog/" />
  <variable name="LogGroup"
            value="AWSLogGroupName" />
</nlog>

```

###Provision Resources require for LogFarm 


Simply run Deploy-Infrastructure.ps1 powershell script at [base-infrastructure](https://github.com/imomou/LogFarm/tree/master/base-infrastructure"), make sure to grab Deployment.ps1 as well with template. And please provide an existing S3 Bucket ( ATM, it doesn't make sense for LogFarm to have a bucket entirely for itself ) 

###Deployments

Logfarm use Kaiseki for CI/CD, it provides very consistent manner dealing with build,test and packaging. I strongely recommend using it along with LogFarm. Furthermore, by default it provided a lot of configuration required for dealing with CI/CD, so all you and the team need to is simply run build.ps1 and then deploy by running WebDeploy.ps1 and specify which config.xml to use e.g WebDeploy.ps1 ... env Site.SetParameters.Prod

[Kaiseki](https://github.com/SleeperSmith/Kaiseki)

###LogFarm TODO

* Application profiling such as MiniProfiler, Glimpse (still evaluating )
* Support for .Net Core
* Intergration with Kinesis ( AWS Lambda or Docker )


If You Have Any Questions or Concerns, Please Contact at ko.sam.wang@gmail.com. All suggestions are welcome! 
Happy Logging! 



