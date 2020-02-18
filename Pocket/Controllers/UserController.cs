using Pocket.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Pocket.Models;

namespace Pocket.Controllers
{
    public class UserController : Controller
    {
        QDbContext db = new QDbContext();
        // GET: User
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public JsonResult GetPageSettings()
        {
            string psetting = Util.ToString(Session["PageSettings"]);
            if(string.IsNullOrEmpty(psetting))
            {
                psetting = User.ApplicationUser().PageSettings;
                Session["PageSettings"] = psetting;
            }

            return Json(new { PageSetting = psetting }) ;
        }
        [HttpPost]
        public JsonResult SetPageSettings(string PageSetting)
        {
            Session["PageSettings"] = PageSetting;
            ApplicationUser u = db.Users.Find(State.UserID);
            u.PageSettings = PageSetting;
            db.Entry(u).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();
            return Json(new HttpStatusCodeResult(HttpStatusCode.OK));
        }
    }
}