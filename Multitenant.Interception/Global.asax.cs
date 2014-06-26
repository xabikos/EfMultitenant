using System;
using System.Data.Entity.Infrastructure.Interception;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Multitenant.Interception.Entities;

namespace Multitenant.Interception
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
        
    }
}