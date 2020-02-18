using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using Pocket.Common;
using System.Security.Claims;
using Microsoft.Owin.Security;
using Microsoft.Owin.Infrastructure;
using System.Net.Http.Headers;

namespace Pocket.Controllers
{
    public class LoginBindingModel
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }
    public class CommonController : ApiController
    {
        [HttpGet]
        [AllowAnonymous]
        //[Route("api/common/login")]
        [ActionName("Login")]
        public HttpResponseMessage Login(string UserName, string Password)
        {
            LoginBindingModel login = new LoginBindingModel { UserName = UserName, Password = Password };
            // todo: add auth
            if (login.UserName == "a" && login.Password == "a")
            {
                var identity = new ClaimsIdentity(Startup.OAuthBearerOptions.AuthenticationType);
                identity.AddClaim(new Claim(ClaimTypes.Name, login.UserName));

                AuthenticationTicket ticket = new AuthenticationTicket(identity, new AuthenticationProperties());
                var currentUtc = new SystemClock().UtcNow;
                ticket.Properties.IssuedUtc = currentUtc;
                ticket.Properties.ExpiresUtc = currentUtc.Add(TimeSpan.FromMinutes(30));

                string accessToken = Startup.OAuthBearerOptions.AccessTokenFormat.Protect(ticket);
                
                //DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ObjectContent<object>(new
                    {
                        UserName = login.UserName,
                        AccessToken = Startup.OAuthBearerOptions.AccessTokenFormat.Protect(ticket)
                    }, Configuration.Formatters.JsonFormatter)
                };
            }

            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        }

        [HttpGet]
        //[Route("api/common/RecursionOptions")]
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
        [HttpGet]
        [Authorize]
        [ActionName("GetSecureMethod")]
        public HttpResponseMessage GetSecureMethod()
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ObjectContent<object>(new
                {
                    UserName = State.CurrentUserName
                }, Configuration.Formatters.JsonFormatter)
            };
        }
        [HttpPost]
        [Authorize]
        [ActionName("PostSecureMethod")]
        public HttpResponseMessage PostSecureMethod()
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ObjectContent<object>(new
                {
                    UserName = State.CurrentUserName
                }, Configuration.Formatters.JsonFormatter)
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
