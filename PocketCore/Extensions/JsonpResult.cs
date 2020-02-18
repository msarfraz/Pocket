//using Newtonsoft.Json;
//using Pocket.Common;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net;
//using System.Web;
//using System.Web.Mvc;
//using System.Web.Script.Serialization;

namespace Pocket.Extensions
{

//    public class JsonResult : JsonResult
//    {
//        //object data = null;

//        public JsonResult()
//        {
//        }

//        public JsonResult(object data):base()
//        {
//            this.Data = data;
//        }

////        public override void ExecuteResult(ControllerContext controllerContext)
////        {
////            base.ExecuteResult(controllerContext);
////            JsonRequestBehavior = System.Web.Mvc.JsonRequestBehavior.AllowGet;
////            return;
////            if (controllerContext != null)
////            {
////                HttpResponseBase Response = controllerContext.HttpContext.Response;
////                HttpRequestBase Request = controllerContext.HttpContext.Request;
////                var callbackname = controllerContext.HttpContext.Request["d"];
////                if (string.IsNullOrEmpty(callbackname))
////                {
////                    throw new Exception("Callback function name must be provided in the request!");
////                }
////                Response.ContentType = "application/x-javascript";
//////                Response.AddHeader(
////  //              "Access-Control-Allow-Origin", "*");
////                if (this.Data != null)
////                {
////                    JavaScriptSerializer serializer = new JavaScriptSerializer();
////                    Response.Write(string.Format("{0}({1});", callbackname, serializer.Serialize(this.Data)));
////                }
////            }

//            /*var serializer = new JavaScriptSerializer();
//            //string json = serializer.Serialize(_obj);
//            string json = JsonConvert.SerializeObject(this.data, Formatting.Indented);

           
//            var jsonp = string.Format("{0}({1})", callbackname, json);
//            var response = context.HttpContext.Response;
//            response.ContentType = "application/json";
//            response.Write(jsonp);*/
//        //}
        
        
//    }
//    public static class Methods
//    {
//        public static JsonResult ToJsonResult(this JsonResult jr)
//        {
//            JsonResult jpr = new JsonResult();
//            jpr.Data = jr.Data;
//            return jpr;
//        }
//    }
}