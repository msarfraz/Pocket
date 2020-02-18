using Pocket.Common;
using Pocket.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using PocketCore.Extensions;

namespace Pocket.Controllers
{
    [Authorize]
    public class ApplicationController : Controller
    {
        protected readonly ApplicationDbContext db;
        public ApplicationController(ApplicationDbContext
            context)
        {
            db = context;
        }
        public String PageSettings
        {
            get { return HttpContext.Session.GetString("PageSettings"); }
            set { HttpContext.Session.SetString("PageSettings", value); }
        }

        public string UserID
        {
            get
            {

                return User.Identity.GetUserId();
            }
        }
        public string UserName
        {
            get
            {
                return User.Identity.GetUserName();
            }
        }
    }
}
