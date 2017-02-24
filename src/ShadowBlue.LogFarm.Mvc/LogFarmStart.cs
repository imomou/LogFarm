using System.Web;
using System.Web.Http;
using ShadowBlue.LogFarm.Mvc;
using WebActivatorEx;

[assembly: PostApplicationStartMethod(typeof(LogFarmStart), "Start")]

namespace ShadowBlue.LogFarm.Mvc
{
    public class LogFarmStart : HttpApplication
    {
        public static void Start()
        {
            GlobalConfiguration.Configuration.Filters.Add(new ElmahErrorAttribute());
        }
    }
}