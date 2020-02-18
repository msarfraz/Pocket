using System.Net.Http;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pocket.Common;

namespace Pocket.Controllers
{
    public class LoginBindingModel
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }
    [ApiController]
    public class CommonController : ControllerBase
    {
       
        [HttpGet]
        [Route("api/common/RecursionOptions")]
        [ActionName("RecursionOptions")]
        public HttpResponseMessage RecursionOptions()
        {
            string selectStr = "<select>";
            selectStr += string.Format("<option value='{0}'>{1}</option>", RepeatPattern.None.GetHashCode(), "");
            selectStr += string.Format("<option value='{0}'>{1}</option>", RepeatPattern.Daily.GetHashCode(), RepeatPattern.Daily.String());
            selectStr += string.Format("<option value='{0}'>{1}</option>", RepeatPattern.Alternate_Days.GetHashCode(), RepeatPattern.Alternate_Days.String());
            selectStr += string.Format("<option value='{0}'>{1}</option>", RepeatPattern.Weekly.GetHashCode(), RepeatPattern.Weekly.String());
            selectStr += string.Format("<option value='{0}'>{1}</option>", RepeatPattern.Bi_Weekly.GetHashCode(), RepeatPattern.Bi_Weekly.String());
            selectStr += string.Format("<option value='{0}'>{1}</option>", RepeatPattern.Monthly.GetHashCode(), RepeatPattern.Monthly.String());
            selectStr += string.Format("<option value='{0}'>{1}</option>", RepeatPattern.Bi_Monthly.GetHashCode(), RepeatPattern.Bi_Monthly.String());
            selectStr += string.Format("<option value='{0}'>{1}</option>", RepeatPattern.Quarterly.GetHashCode(), RepeatPattern.Quarterly.String());
            selectStr += string.Format("<option value='{0}'>{1}</option>", RepeatPattern.Bi_Quarterly.GetHashCode(), RepeatPattern.Bi_Quarterly.String());
            selectStr += string.Format("<option value='{0}'>{1}</option>", RepeatPattern.Yearly.GetHashCode(), RepeatPattern.Yearly.String());

            selectStr += "</select>";

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
