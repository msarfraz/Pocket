using Pocket.Common;
using Pocket.Extensions;
using Pocket.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Pocket.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        QDbContext db = new QDbContext();

        // GET: /Notification/Index
        public ActionResult Index()
        {
            return View();
        }
        // GET: /Notification/
        public JsonResult MList(int page, int rows)
        {
            return (JsonResult) List("NotificationDate", "desc", page, rows, ResultType.Mobile);
        }
        public JsonResult JList(string sidx, string sord, int page, int rows)
        {
            return List(sidx, sord, page, rows, ResultType.Web);
        }

        private JsonResult List(string sidx, string sord, int page, int rows, ResultType rt)
        {
                var notifications = db.Notifications.Where(notif => notif.UserID == State.UserID);

                return Util.CreateJsonResponse<Notification>(sidx, sord, page, rows, notifications, rt, (Func<IEnumerable<Notification>, Array>)delegate(IEnumerable<Notification> rd)
                {
                    if (rt == ResultType.Web)
                    {
                        return (
                       from notif in rd
                       select new
                       {
                           NotificationID = notif.NotificationID,
                           cell = new string[] { notif.NotificationID.ToString(), notif.NotificationDate.ToDateString(), notif.Text, notif.URL, notif.NotificationStatus.GetHashCode().ToString() }
                       }).ToArray();
                    }
                    else
                    {
                        return (
                       from notif in rd
                       select new
                       {
                           NotificationID = notif.NotificationID,
                           NotificationDate = notif.NotificationDate.ToDateString(),
                           Title = notif.Title,
                           Text = notif.Text,
                           URL = notif.URL,
                           MobileURL = notif.MobileURL,
                           Status = notif.NotificationStatus.GetHashCode().ToString()
                       }).ToArray();
                    }
                }
                    );

        }
        public JsonResult NotificationsCount()
        {
            int count = db.Notifications.Where(n => n.UserID == State.UserID && n.NotificationStatus == NotificationStatus.Unread).Count();
            JsonResult jr = Repository.Success<JsonResult>(count);
            jr.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jr;
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        // GET: /Notification/markread
        public JsonResult markread(int NotificationID)
        {
            Notification notif = db.Notifications.Where(n=>n.UserID == State.UserID && n.NotificationID == NotificationID && n.NotificationStatus == NotificationStatus.Unread).FirstOrDefault();
            if (notif != null)
            {
                notif.NotificationStatus = NotificationStatus.Read;
                db.Entry(notif).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
            }
            return Json(new HttpStatusCodeResult(HttpStatusCode.OK));
        }

        [HttpPost]
        public JsonResult MarkAllRead()
        {
            try
            {
                var notifs = db.Notifications.Where(n => n.UserID == State.UserID && n.NotificationStatus == NotificationStatus.Unread);
                if (notifs != null)
                {
                    notifs.ToList().ForEach(n => n.NotificationStatus = NotificationStatus.Read);

                    db.SaveChanges();
                }

            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            return Json(new HttpStatusCodeResult(HttpStatusCode.OK));
        }
        
    }
}
