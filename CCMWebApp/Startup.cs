using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(CCMWebApp.Startup))]
namespace CCMWebApp
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
