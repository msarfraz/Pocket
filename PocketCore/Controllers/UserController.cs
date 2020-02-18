using Pocket.Models;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pocket.Common;

namespace Pocket.Controllers
{
    public class UserController : ApplicationController
    {
        public UserController(ApplicationDbContext
            context) : base(context)
        {
            
        }
        // GET: User
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public JsonResult GetPageSettings()
        {
            string psetting = Util.ToString(PageSettings);
            if(string.IsNullOrEmpty(psetting))
            {
                psetting = User.ApplicationUser(db).PageSettings;
                PageSettings = psetting;
            }

            return Json(new { PageSetting = psetting }) ;
        }
        [HttpPost]
        public JsonResult SetPageSettings(string PageSetting)
        {
            PageSettings = PageSetting;
            ApplicationUser u = db.Users.Find(UserID);
            u.PageSettings = PageSetting;
            db.Entry(u).State = EntityState.Modified;
            db.SaveChanges();
            return Json(Ok());
        }
    }
}