using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

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

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {
            if (Request.IsAuthenticated)
            {
                //get the username which we previously set in
                //forms authentication ticket in our login1_authenticate event
                string loggedUser = HttpContext.Current.User.Identity.Name;

                //build a custom identity and custom principal object based on this username
                CustomIdentitiy identity = new CustomIdentitiy(loggedUser);
                CustomPrincipal principal = new CustomPrincipal(identity);

                //set the principal to the current context
                HttpContext.Current.User = principal;
            }
        }
    }
}
