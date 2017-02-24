using System;
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
            base.OnException(actionExecutedContext);

            var e = actionExecutedContext.Exception;
            var error = new Error(e, HttpContext.Current);
            var elmahError = ErrorLog.GetDefault(HttpContext.Current);

            elmahError.Log(error);
        }
    }
}