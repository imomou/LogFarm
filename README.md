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


###Enable ELmah 


 <configuration>
 .
 .
 .
   <elmah>
     <errorLog type="ShadowBlue.LogFarm.Domain.Elmah.ElmahDynamoDbErrorLog, ShadowBlue.LogFarm.Domain"
       ddbAppName="$rootnamespace$" ddbTableName="elmah-dev" ddbEnvironment="local"  />
   </elmah>
 </configuration>
