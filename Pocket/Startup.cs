using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Pocket.Startup))]
namespace Pocket
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
