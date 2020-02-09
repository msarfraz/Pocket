using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace Pocket
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            GlobalConfiguration.Configuration.Routes.MapHttpRoute(
     name: "DefaultApi",
     routeTemplate: "api/{controller}/{id}",
     defaults: new { id = System.Web.Http.RouteParameter.Optional }
   );
            GlobalConfiguration.Configuration.Routes.MapHttpRoute(
    name: "ActionApi",
    routeTemplate: "api/{controller}/{action}/{id}",
    defaults: new { id = RouteParameter.Optional }
);
            // New code:
           /* var json = config.Formatters.JsonFormatter;
            json.SerializerSettings.PreserveReferencesHandling =
                Newtonsoft.Json.PreserveReferencesHandling.Objects;

            config.Formatters.Remove(config.Formatters.XmlFormatter);*/
        }
    }
}
