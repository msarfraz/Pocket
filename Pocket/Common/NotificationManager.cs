using Pocket.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Pocket.Common
{
    public static class NotificationManager
    {
        private static void AddNotification(QDbContext dbc, Notification notification)
        {
            dbc.Notifications.Add(notification);
            dbc.SaveChanges();
        }

        public static void AddExpenseNotification(QDbContext db, Expense exp, bool AddExpense, string generatedBy)
        {
            db.Entry(exp).Reference(u => u.User).Load();
            db.Entry(exp).Reference(u => u.Subcategory).Load();
            db.Entry(exp).Reference(u => u.Account).Load();

            if (exp.Account.SharedFriends.Count > 0)
            {
                var account = exp.Account;
                var lstUsers = account.SharedFriends.Select(eu => eu.UserID).ToList();
                lstUsers.Add(account.UserID);
                lstUsers.Remove(generatedBy);// remove current user
                AddExpenseNotification(db, exp, AddExpense, generatedBy, lstUsers, SharedResourceType.Account);
            }
            if (exp.Subcategory.Category.SharedContacts.Count > 0)
            {
                var cat = exp.Subcategory.Category;
                var lstUsers = cat.SharedContacts.Select(eu => eu.UserID).ToList();
                lstUsers.Add(cat.UserID);
                lstUsers.Remove(generatedBy);// remove current user
                AddExpenseNotification(db, exp, AddExpense, generatedBy, lstUsers, SharedResourceType.Category);
            }
            if (exp.EventID.HasValue)
            {
                db.Entry(exp).Reference(u => u.Event).Load();
                Event evt = exp.Event;

                var lstUsers = evt.SharedFriends.Select(eu => eu.UserID).ToList();
                lstUsers.Add(evt.UserID);
                lstUsers.Remove(generatedBy);// remove current user
                AddExpenseNotification(db, exp, AddExpense, generatedBy, lstUsers, SharedResourceType.Event);
            }
        }
        private static void AddExpenseNotification(QDbContext db, Expense exp, bool AddExpense, string generatedBy, List<string> lstUsers, SharedResourceType rtype)
        {
            string resource = string.Empty;
            int resid = 0;
            switch (rtype)
            {
                case SharedResourceType.Event:
                    resource = exp.Event.Name;
                    resid = exp.EventID.Value;
                    break;
                case SharedResourceType.Account:
                    resource = exp.Account.Name;
                    resid = exp.AccountID;
                    break;
                case SharedResourceType.Category:
                    resource = exp.Subcategory.Category.Name;
                    resid = exp.Subcategory.CategoryID;
                    break;
                default:
                    break;
            }
            ApplicationUser euser = exp.User;// == null ? qdb.Users.Find(exp.UserID) : exp.User;
            Subcategory scat = exp.Subcategory;//== null ? qdb.Subcategories.Find(exp.SubcategoryID) : exp.Subcategory;
            foreach (var frUserID in lstUsers)
            {
                Notification notif = new Notification();
                notif.UserID = frUserID;
                notif.GeneratedBy = generatedBy;
                notif.NotificationDate = DateTime.Now;
                notif.Title = "Expense " + (AddExpense ? "Added" : "Updated");
                notif.Text = string.Format("An expense 'Dated:{0}, Amount:{1}, Details:{2}' is {3} by '{4}' in '{5}:{6}'",
                    exp.ExpenseDate.ToDateString(), exp.Amount.ToString(), scat.Category.Name + "-" + scat.Name,
                     AddExpense ? "added" : "updated", Util.GetUserName(euser), rtype.String(), resource);
                notif.URL = string.Format("/Report/{0}Report/{0}", rtype.String(), resid);
                notif.MobileURL = string.Format("Pages/Expense.html?id={0}", exp.ExpenseID);
                db.Notifications.Add(notif);
            }
        }
        internal static void AddEventShareNotification(int eventID, string eventName, string friendID, QDbContext db, string generatedBy, string genUserName)
        {
            Notification notif = new Notification();
            notif.UserID = friendID;
            notif.GeneratedBy = generatedBy;
            notif.NotificationDate = DateTime.Now;
            notif.Title = "Event Shared";
            notif.Text = string.Format("An event '{0}' is shared with you by '{1}'",
                eventName, genUserName);
            notif.URL = string.Format("/Report/EventReport/{0}", eventID);
            notif.MobileURL = string.Format("Pages/Event.html?id={0}", eventID);
            db.Notifications.Add(notif);
        }
        internal static void RemoveEventShareNotification(int eventID, string eventName, string friendID, QDbContext db, string generatedBy, string genUserName)
        {
            Notification notif = new Notification();
            notif.UserID = friendID;
            notif.GeneratedBy = generatedBy;
            notif.NotificationDate = DateTime.Now;
            notif.Title = "Event Revoked";
            notif.Text = string.Format("An event '{0}' sharing is revoked by '{1}'",
                eventName, genUserName);
            notif.URL = string.Format("/Report/EventReport", eventID);
            notif.MobileURL = string.Format("Pages/EventList.html?id={0}", eventID);
            db.Notifications.Add(notif);
        }
        internal static void AddFriendNotification(string friendID, QDbContext db)
        {
            Notification notif = new Notification();
            notif.UserID = friendID;
            notif.GeneratedBy = State.UserID;
            notif.NotificationDate = DateTime.Now;
            notif.Title = "Friend Request";
            notif.Text = string.Format("'{0}' sent you a friend request.", State.CurrentUserName);
            notif.URL = string.Format("/Friend/Index");
            notif.MobileURL = string.Format("Pages/FriendList.html");
            db.Notifications.Add(notif);
        }
        internal static void AddExpenseCommentNotification(string comment, int ExpenseID, QDbContext db)
        {
            Expense exp = db.Expenses.Find(ExpenseID);
            if (exp.EventID.HasValue)
            {
                Event evt = db.Events.Find(exp.EventID.Value);
                var lstUsers = evt.SharedFriends.Select(eu => eu.UserID).ToList();
                lstUsers.Add(evt.UserID);

                foreach (var frUserID in lstUsers)
                {
                    if (frUserID != State.UserID) // don't generate a notification for myself
                    {
                        Notification notif = new Notification();
                        notif.UserID = frUserID;
                        notif.GeneratedBy = State.UserID;
                        notif.NotificationDate = DateTime.Now;
                        notif.Title = "Comment Added";
                        notif.Text = string.Format("A comment '{0}' is added by '{1}' in '{2}'",
                           comment, State.CurrentUserName, evt.Name);
                        notif.URL = string.Format("/Report/EventReport?id={0}&ExpenseID={1}", exp.EventID, ExpenseID);
                        notif.MobileURL = string.Format("Pages/ExCommentList.html?expenseid={0}", ExpenseID);
                        db.Notifications.Add(notif);
                    }

                }
                if (evt.SharedFriends.Count > 0)
                    db.SaveChanges();
            }
        }

        internal static bool AddExpenseDelNotification(QDbContext db, Expense exp)
        {
            if (exp.EventID.HasValue)
            {
                Event evt = exp.Event;
                var lstUsers = evt.SharedFriends.Select(eu => eu.UserID).ToList();
                lstUsers.Add(evt.UserID);
                foreach (var frUserID in lstUsers)
                {
                    Notification notif = new Notification();
                    notif.UserID = frUserID;
                    notif.GeneratedBy = State.UserID;
                    notif.NotificationDate = DateTime.Now;
                    notif.Title = "Expense Removed";
                    notif.Text = string.Format("An expense '{0} -- {1}' is removed by '{2}' from '{3}'",
                        string.IsNullOrEmpty(exp.Description) ? exp.Subcategory.Category.Name + "-" + exp.Subcategory.Name : exp.Description,
                        exp.ExpenseDate.ToDateString(), Util.GetUserName(exp.User), evt.Name);
                    notif.URL = string.Format("/Report/Report/{0}", exp.EventID);
                    notif.MobileURL = string.Format("Pages/ReportList.html?name=eventreport&eventid={0}", exp.EventID);
                        db.Notifications.Add(notif);
                }
                return evt.SharedFriends.Count > 0;
            }
            return false;
        }

        internal static void AddAccountShareNotification(int accountID, string accountName, string friendID, QDbContext db, string generatedBy, string genUserName)
        {
            Notification notif = new Notification();
            notif.UserID = friendID;
            notif.GeneratedBy = generatedBy;
            notif.NotificationDate = DateTime.Now;
            notif.Title = "Account Shared";
            notif.Text = string.Format("An account '{0}' is shared with you by '{1}'",
                accountName, genUserName);
            notif.URL = string.Format("/Report/AccountReport/{0}", accountID);
            notif.MobileURL = string.Format("Pages/ReportList.html?name=accountreport&accountid={0}", accountID);

            db.Notifications.Add(notif);
        }

        internal static void RemoveAccountShareNotification(int accountid, string accName, string FriendID, QDbContext db, string generatedBy, string genUserName)
        {
            Notification notif = new Notification();
            notif.UserID = FriendID;
            notif.GeneratedBy = generatedBy;
            notif.NotificationDate = DateTime.Now;
            notif.Title = "Account Revoked";
            notif.Text = string.Format("An account '{0}' sharing is revoked by '{1}'",
                accName, genUserName);
            notif.URL = string.Format("/Report/AccountReport", accountid);
            notif.MobileURL = string.Format("Pages/AccountList.html?id={0}", accountid);
            db.Notifications.Add(notif);
        }

        internal static void AddCategoryShareNotification(int categoryID, string categoryName, string friendID, QDbContext db, string generatedBy, string genUserName)
        {
            Notification notif = new Notification();
            notif.UserID = friendID;
            notif.GeneratedBy = generatedBy;
            notif.NotificationDate = DateTime.Now;
            notif.Title = "Category Shared";
            notif.Text = string.Format("A Category '{0}' is shared with you by '{1}'",
                categoryName, genUserName);
            notif.URL = string.Format("/Category/Index/{0}", categoryID);
            notif.MobileURL = string.Format("Pages/Category.html?id={0}", categoryID);
            db.Notifications.Add(notif);
        }

        internal static void RemoveCategoryShareNotification(int categoryID, string categoryName, string FriendID, QDbContext db, string generatedBy, string genUserName)
        {
            Notification notif = new Notification();
            notif.UserID = FriendID;
            notif.GeneratedBy = generatedBy;
            notif.NotificationDate = DateTime.Now;
            notif.Title = "Category Revoked";
            notif.Text = string.Format("A Category '{0}' sharing is revoked by '{1}'",
                categoryName, genUserName);
            notif.URL = string.Format("/Category/Index", categoryID);
            notif.MobileURL = string.Format("Pages/CategoryList.html?id={0}", categoryID);
            db.Notifications.Add(notif);
        }
    }
}