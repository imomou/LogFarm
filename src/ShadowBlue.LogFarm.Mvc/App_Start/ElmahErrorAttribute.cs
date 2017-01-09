using System.Web;
using System.Web.Http.Filters;
using System.Web.Mvc;
using Elmah;

namespace ShadowBlue.LogFarm.Mvc
{
    public class ElmahErrorAttribute : ExceptionFilterAttribute, System.Web.Mvc.IExceptionFilter
    {
        public void OnException(ExceptionContext filterContext)
        {
            var error = new Error(filterContext.Exception, HttpContext.Current);
            var elmahError = ErrorLog.GetDefault(HttpContext.Current);
            elmahError.Log(error);
        }

        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            var error = new Error(actionExecutedContext.Exception, HttpContext.Current);
            var elmahError = ErrorLog.GetDefault(HttpContext.Current);
            elmahError.Log(error);

            base.OnException(actionExecutedContext);
        }
    }
}