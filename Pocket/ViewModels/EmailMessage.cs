using Postal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Pocket.ViewModels
{
    public class EmailMessage: Email
    {
        public EmailMessage(string viewName)
            : base(viewName)
        {

        }
        public string To { get; set; }
        public string From { get; set; }
        public string Body { get; set; }
        public string Subject { get; set; }
        public string CallbackURL { get; set; }
        
    }
}