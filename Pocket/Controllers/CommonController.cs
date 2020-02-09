using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using Pocket.Common;

namespace Pocket.Controllers
{
    public class CommonController : ApiController
    {
        [HttpGet]
        public HttpResponseMessage RecursionOptions()
        {

            string selectStr = "<select>" +
                                    "<option value='0'></option>" + 
                                    "<option value='1'>Daily</option>" +
                                    "<option value='7'>Weekly</option>"+
                                    "<option value='30'>Monthly</option>"+
                                    "<option value='365'>Yearly</option>"+
                                "</select>";

            return new HttpResponseMessage()
            {
                Content = new StringContent(
                    selectStr,
                    Encoding.UTF8,
                    "text/html"
                )
            };
        }

        //[HttpGet]
        //public HttpResponseMessage TargetStatus()
        //{
            
        //    string selectStr = "<select>" +
        //                            "<option value='" + Pocket.Common.TargetStatus.InActive.GetHashCode() + "'>InActive</option>" +
        //                            "<option value='" + Pocket.Common.TargetStatus.Active.GetHashCode() + "'>Active</option>" +
        //                        "</select>";

        //    return new HttpResponseMessage()
        //    {
        //        Content = new StringContent(
        //            selectStr,
        //            Encoding.UTF8,
        //            "text/html"
        //        )
        //    };
        //}
    }
}
