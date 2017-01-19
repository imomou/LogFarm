using System.Web;
using System.Web.Mvc;
using ShadowBlue.LogFarm.Mvc;
using WebActivatorEx;

[assembly: PostApplicationStartMethod(typeof(LogFarmStart), "Start")]

namespace ShadowBlue.LogFarm.Mvc
{
    public class LogFarmStart : HttpApplication
    {
        public static void Start()
        {
            GlobalFilters.Filters.Add(new ElmahErrorAttribute());
            GlobalFilters.Filters.Add(new HandleErrorAttribute());
        }
    }
}