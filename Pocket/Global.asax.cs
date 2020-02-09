using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using OpenAuth.App_Start;
using WebMatrix.WebData;

using System.Web.Http;

namespace Pocket
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            //Database.SetInitializer<Pocket.Models.QDbContext>(new DropCreateDatabaseAlways<Pocket.Models.QDbContext>());
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register); 
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            //WebSecurity.InitializeDatabaseConnection("QDbContext", "Users", "User_Id", "Login_id", autoCreateTables: true);
            AuthConfig.RegisterAuth();

            
        }
    }
}
