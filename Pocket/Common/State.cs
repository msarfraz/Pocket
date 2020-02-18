using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.Owin.Helpers;

namespace Pocket.Common
{
    
    public enum UserType
    {
        FB = 1,
        Google = 2,
        Application = 3,
    }

    public class State
    {
        static State()
        {
            
        }
        public static string UserID
        {
            get
            {
                return HttpContext.Current.User.Identity.GetUserId();
            }
        }
        public static string CurrentUserName
        {
            get
            {
                return HttpContext.Current.User.Identity.GetUserName();
            }
        }
    }
    
}