using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security;
using Pocket.Common;
using Owin;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.AspNet.Identity.EntityFramework;
using Pocket.Models;
using Microsoft.Owin.Security.OAuth;

namespace Pocket
{
    public partial class Startup
    {
        // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
            OAuthBearerOptions = new OAuthBearerAuthenticationOptions();
            app.UseOAuthBearerAuthentication(OAuthBearerOptions);

            // Enable the application to use a cookie to store information for the signed in user
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login")
            });
            // Use a cookie to temporarily store information about a user logging in with a third party login provider
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);
            
            /*
            app.UseGoogleAuthentication();
            app.UseFacebookAuthentication(
                appId: "645145318865406",
               appSecret: "918d5ededf8f9bc4f68b8276c9871e32");
            */

            app.UseGoogleAuthentication("454833270142-rnr6usroi63gssvm43oucqvp622u49on.apps.googleusercontent.com", "7aWM8nP8QyH3mwwNUPNsGR8U");
            app.UseFacebookAuthentication(
                appId: "306815996164601",
               appSecret: "d3c80d58e5d3750606a1b7a37a9c245e");

            DataProtector = app.CreateDataProtector();
            
            //var RoleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(new QDbContext()));
            //IdentityRole irole = RoleManager.FindByName("Admin");
            //if(irole == null)
            //    RoleManager.Create(new IdentityRole("Admin"));
        }

        public static IDataProtector DataProtector { get; set; }
        public static OAuthBearerAuthenticationOptions OAuthBearerOptions { get; private set; }
    }
}