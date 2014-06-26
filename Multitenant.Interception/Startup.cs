using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Multitenant.Interception.Startup))]
namespace Multitenant.Interception
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
