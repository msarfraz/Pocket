using Pocket.Common;
using Pocket.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Pocket.Controllers
{
    public class NotificationController : Controller
    {
        QDbContext db = new QDbContext();

        // GET: /Notification/List
        public ActionResult List()
        {
            return View();
        }
        // GET: /Notification/
        public JsonResult JList(string sidx, string sord, int page, int rows)
        {
            if (Request.IsAjaxRequest())
            {
                var notifications = db.Users.Find(State.UserID).Notifications;

                return Util.CreateJsonResponse<Notification>(sidx, sord, page, rows, notifications, (Func<IEnumerable<Notification>, Array>)delegate(IEnumerable<Notification> rd)
                {
                    return (
                        from notif in rd
                        select new
                        {
                            NotificationID = notif.NotificationID,
                            cell = new string[] { notif.NotificationID.ToString(), notif.Text, notif.URL }
                        }).ToArray();
                }
                    );
            }
            else
                return Json(new HttpStatusCodeResult(HttpStatusCode.BadRequest));

        }
        public static void AddNotification(QDbContext dbc, Notification notification)
        {
            dbc.Notifications.Add(notification);
            dbc.SaveChanges();
        }
        public static void AddExpenseNotification(QDbContext db, int ExpenseID, bool AddExpense)
        {
            Expense exp = db.Expenses.Find(ExpenseID);
            if (exp.EventID.HasValue)
            {
                Event evt = db.Events.Find(exp.EventID.Value);
                foreach (var fr in evt.SharedFriends)
                {
                    Notification notif = new Notification();
                    notif.UserID = fr.UserID;
                    notif.GeneratedBy = State.UserID;
                    notif.NotificationDate = DateTime.Now;
                    notif.Text = string.Format("An expense '{0} -- {1}' is {2} by '{3}' in '{4}'",
                        string.IsNullOrEmpty(exp.Description) ? exp.Subcategory.Category.Name + "-" + exp.Subcategory.Name : exp.Description,
                        exp.ExpenseDate.ToShortDateString(), AddExpense ? "added" : "updated", Util.GetUserName(exp.User), evt.Name);
                    notif.URL = string.Format("/Report/EventReport/{0}", exp.EventID);
                    db.Notifications.Add(notif);
                }
                if(evt.SharedFriends.Count > 0)
                db.SaveChanges();
            }
        }

        internal static void AddEventShareNotification(int eventID, string eventName, int friendID, QDbContext db)
        {
            Notification notif = new Notification();
            notif.UserID = friendID;
            notif.GeneratedBy = State.UserID;
            notif.NotificationDate = DateTime.Now;
            notif.Text = string.Format("An event '{0}' is shared with you by '{1}'",
                eventName, State.CurrentUserName);
            notif.URL = string.Format("/Report/EventReport/{0}", eventID);
            db.Notifications.Add(notif);
            db.SaveChanges();
        }

        internal static void AddExpenseCommentNotification(string comment, int ExpenseID, QDbContext db)
        {
            Expense exp = db.Expenses.Find(ExpenseID);
            if (exp.EventID.HasValue)
            {
                Event evt = db.Events.Find(exp.EventID.Value);
                foreach (var fr in evt.SharedFriends)
                {
                    Notification notif = new Notification();
                    notif.UserID = fr.UserID;
                    notif.GeneratedBy = State.UserID;
                    notif.NotificationDate = DateTime.Now;
                    notif.Text = string.Format("A comment '{0}' is added by '{1}' in '{2}'",
                       comment, State.CurrentUserName, evt.Name);
                    notif.URL = string.Format("/Report/EventReport?id={0}&ExpenseID={1}", exp.EventID, ExpenseID);
                    db.Notifications.Add(notif);
                }
                if (evt.SharedFriends.Count > 0)
                    db.SaveChanges();
            }
        }
    }
}
