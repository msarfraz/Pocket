using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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
        public static int UserID { get; set; }
        static State()
        {
            UserID = 1;
        }

        public static string CurrentUserName
        {
            get
            {
                return "Admin admin";
            }
        }
    }
    
}