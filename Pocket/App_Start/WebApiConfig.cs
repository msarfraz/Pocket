using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web.Http;

namespace Pocket
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            /*
                        GlobalConfiguration.Configuration.Routes.MapHttpRoute(
                 name: "DefaultApi",
                 routeTemplate: "api/{controller}/{action}/{UserName}/{Password}"
                 //, defaults: new { id = System.Web.Http.RouteParameter.Optional }
               );*/
                        GlobalConfiguration.Configuration.Routes.MapHttpRoute(
                name: "ActionApi",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
            // New code:
             /*           var jsonFormatter = config.Formatters.OfType<JsonMediaTypeFormatter>().First();
                        jsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            var json = config.Formatters.JsonFormatter;
            json.SerializerSettings.PreserveReferencesHandling =
                Newtonsoft.Json.PreserveReferencesHandling.Objects;

            config.Formatters.Remove(config.Formatters.XmlFormatter);*/
        }
    }
}
