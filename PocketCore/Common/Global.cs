using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Pocket.Models;
using Pocket.ViewModels;
using PocketCore.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Xml.Linq;

namespace Pocket.Common
{
    public enum ObjectType
    {
        Expense,
        Transfer,
        Income,
        Saving
    }
    public enum YesNoOptions
    {
        No = 0,
        Yes = 1
    }
    public enum ResultType
    {
        Web = 1,
        Mobile =2
    }
    public enum ExpenseDepth
    {
        MyExpenses = 1,
        OtherExpensesMySharedCategories,
        OtherExpensesOtherSharedCategories,

    }
    public enum DisplaySetting
    {
        No = 0,
        Yes = 1
    }
    public enum SharedResourceType
    {
        Event = 1,
        Account = 2,
        Category = 3,
    }
    public enum EventStatus
    {
        Active = 1,
        Expired = 2
    }
    
    public enum AccountType
    {
        Debit =1,
        Credit =2,
        Saving=3
    }
    public enum CustomerRequestType
    {
        Feature_Request=1,
        Complaint =2,
        Report_Bug =3
    }
    public enum TransactionType
    {
        Credit = 1,
        Debit = 2
    }
    public enum NotificationStatus
    {
        Unread=1,
        Read=2
    }
    public enum RepeatPattern
    {
        None = 0,
        Daily = 1,
        Alternate_Days = 2,
        Weekly = 7,
        Bi_Weekly = 14,
        Monthly = 30,
        Bi_Monthly = 60,
        Quarterly = 90,
        Bi_Quarterly = 180,
        Yearly = 365
    }
    public enum BudgetType
    {
        Sub_Category = 1,
        Event = 2,
        Target=3
    }
    public enum TargetStatus
    {
        Active = 1,
        Achieved = 2,
        Cancelled = 3,
        InActive = 4
    }
    public enum FriendStatus
    {
        Pending = 1,
        Approved = 2
    }
    public enum ScheduleStatus
    {
        Pending = 1,
        Error = 2,
        In_Process = 3,
        Complete = 4
    }
    public enum ScheduleType
    {
        Expense =1,
        Income =2,
        Event=3
    }
    public static class Global
    {
        
        public static string ApplicationName { get { return "Xpert Budget";} }
        public static string FromAddress { get { return "Support@xpertbudget.com"; } }
        public static string DeleteRow { get { return "del"; } }

        public static string SortNotRequired { get { return "SORTNOTREQUIRED"; } }

        public static string String(this Enum value)
        {
            return Enum.GetName(value.GetType(), value).Replace('_', ' ');
        }
        public static ApplicationUser ApplicationUser(this IPrincipal user, ApplicationDbContext db)
        {
            return db.Users.Find(user.Identity.GetUserId());
            //GenericPrincipal userPrincipal = (GenericPrincipal)user;
            /*UserManager<ApplicationUser> userManager = new UserManager<Models.ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));
            if (user.Identity.IsAuthenticated)
            {
                return await userManager.FindByIdAsync(user.Identity..GetUserId());
            }
            else
            {
                return null;
            }*/
        }
        public static string getNotificationCount(ApplicationDbContext db, string UserID)
        {
            //var db = new ApplicationDbContext();
            int count = db.Notifications.Where(n => n.UserID == UserID && n.NotificationStatus == NotificationStatus.Unread).Count();
            return count > 0 ? count.ToString() : "";

        }
        public static void CreateUserDefaults(string UserID, ApplicationDbContext db)
        {
            //var db = new ApplicationDbContext();
            if(db.Payees.Where(p => p.UserID == UserID).Count() > 1 ||
                db.Vendors.Where(v => v.UserID == UserID).Count() > 1 ||
                db.IncomeSources.Where(ic => ic.UserID == UserID).Count() > 1 ||
                db.Categories.Where(c => c.UserID == UserID).Count() > 1 ||
                db.Accounts.Where(a=>a.UserID == UserID).Count() > 1)
            {
                // user defaults laready created.
                return;
            }
            var catXml = @"<?xml version='1.0'?><cats><cat name='Auto'><scat>Public Transport</scat><scat>Maintenance</scat><scat>Fuel</scat></cat><cat name='Family'><scat>Shopping</scat><scat>Clothing</scat><scat>Shoes</scat><scat>Children</scat><scat>Baby Food</scat></cat><cat name='Home'><scat>Rent</scat><scat>Mortgage</scat><scat>Furniture</scat><scat>Grocery</scat><scat>Maintenance</scat><scat>Laundry</scat></cat><cat name='Entertainment'><scat>Music</scat><scat>Movies</scat><scat>Magazines</scat><scat>Games</scat><scat>Fun</scat><scat>Food</scat><scat>Restaurants</scat><scat>Gifts</scat><scat>Jewelry</scat></cat><cat name='Bills'><scat>Phone</scat><scat>Internet</scat><scat>Electricity</scat></cat><cat name='Services'><scat>Shipping</scat><scat>Repairs</scat><scat>Religion</scat><scat>Photography</scat></cat></cats>";
            XDocument doc = XDocument.Load(new StringReader(catXml));

            foreach (XElement el in doc.Root.Elements())
            {
                Category cat = new Category { Name = el.FirstAttribute.Value, UserID = UserID, Display = DisplaySetting.Yes };
                cat.Subcategories = new List<Subcategory>();
                foreach (XElement element in el.Elements())
                {
                    Subcategory scat = new Subcategory{ Name = element.Value};
                    scat.Budget = new Budget{ BudgetAmount = 0, BudgetDuration = RepeatPattern.Monthly, BudgetType = BudgetType.Sub_Category, UserID = UserID};
                    cat.Subcategories.Add(scat);
                }
                db.Categories.Add(cat);
            }
            db.Payees.Add(new Payee { Name = "Self", UserID = UserID });
            db.Payees.Add(new Payee { Name = "Wife", UserID = UserID });
            db.Payees.Add(new Payee { Name = "Driver", UserID = UserID });
            db.Payees.Add(new Payee { Name = "Friend", UserID = UserID });
            db.Payees.Add(new Payee { Name = "Colleague", UserID = UserID });

            db.Vendors.Add(new Vendor { Name = "Walmart", UserID = UserID });
            db.Vendors.Add(new Vendor { Name = "Amazon", UserID = UserID });
            db.Vendors.Add(new Vendor { Name = "EBay", UserID = UserID });

            db.IncomeSources.Add(new IncomeSource { Name = "Salary", UserID = UserID });
            db.IncomeSources.Add(new IncomeSource { Name = "Bonus", UserID = UserID });
            db.IncomeSources.Add(new IncomeSource { Name = "Shares", UserID = UserID });
            db.IncomeSources.Add(new IncomeSource { Name = "Interest", UserID = UserID });

            db.Accounts.Add(new Account { Name = "Checking", UserID = UserID, InitialAmount = 0, AccountType= AccountType.Debit });
            db.Accounts.Add(new Account { Name = "Saving", UserID = UserID, InitialAmount = 0, AccountType = AccountType.Saving });
            db.Accounts.Add(new Account { Name = "Credit Card", UserID = UserID, InitialAmount = 0, AccountType = AccountType.Credit });

            db.Notifications.Add(new Notification { GeneratedBy = UserID, UserID = UserID, NotificationDate = DateTime.Now, Text = "Default Categories created. Please add/edit as per your requirement", URL = "/Category/", MobileURL="Pages/CategoryList.html", Title="Category Added" });
            db.Notifications.Add(new Notification { GeneratedBy = UserID, UserID = UserID, NotificationDate = DateTime.Now, Text = "Default Payees created. Please add/edit as per your requirement", URL = "/Payee/", MobileURL = "Pages/PayeeList.html", Title = "Payee Added" });
            db.Notifications.Add(new Notification { GeneratedBy = UserID, UserID = UserID, NotificationDate = DateTime.Now, Text = "Default Income Sources created. Please add/edit as per your requirement", URL = "/IncomeSource/", MobileURL = "Pages/IncomeSourceList.html", Title = "Income Source Added" });
            db.Notifications.Add(new Notification { GeneratedBy = UserID, UserID = UserID, NotificationDate = DateTime.Now, Text = "Default Accounts created. Please add/edit as per your requirement", URL = "/QAccount/", MobileURL = "Pages/AccountList.html", Title = "Account Added" });
            db.Notifications.Add(new Notification { GeneratedBy = UserID, UserID = UserID, NotificationDate = DateTime.Now, Text = "Default Vendors created. Please add/edit as per your requirement", URL = "/Vendor/", MobileURL = "Pages/VendorList.html", Title = "Vendor Added" });

            db.SaveChanges();
        }
        public static double getMonthBudget(ApplicationDbContext db,string UserID, int month, int year, bool ConstantBudget)
        {
            double mbudget = 0;
            var mcats = db.Users.Find(UserID).Categories;
            var fcats = db.CategoryUsers.Where(cat => cat.UserID == UserID).Select(cat => cat.Category);
            var cats = mcats.Union(fcats).ToList();
            foreach (var cat in cats)
            {
                foreach (var scat in cat.Subcategories)
                {
                    mbudget += Repository.GetSubcatMonthlyBudget(db,UserID, scat, month, year, ConstantBudget);
                }
            }

            List<EventBudgetAmount> eventBudget = getEventMonthlyBudgetAmount(db,UserID, month, year, ConstantBudget);
            double eMonthBudget = eventBudget.Select(eb => eb.Budget).DefaultIfEmpty(0).Sum();

            List<TargetBudgetAmount> targetBudget = getTargetMonthlyBudgetAmount(db,UserID, month, year, ConstantBudget);
            double tMonthBudget = targetBudget.Select(eb => eb.Budget).DefaultIfEmpty(0).Sum();

            return mbudget + eMonthBudget + tMonthBudget;
        }
        public static double getMonthExpense(ApplicationDbContext db,string UserID, int month, int year)
        {
            var exp = db.Expenses.Where(e => e.ExpenseDate.Month == month && e.ExpenseDate.Year == year && e.UserID == UserID).
                Select(i => i.Amount).DefaultIfEmpty(0).Sum();
            return exp;
        }
        public static List<TargetBudgetAmount> getTargetMonthlyBudgetAmount(ApplicationDbContext db,string UserID, int month, int year, bool ConstantBudget)
        {
            List<TargetBudgetAmount> tbas = new List<TargetBudgetAmount>();

            DateTime dtNow = new DateTime(year, month, 1);

            var targets = db.Targets.Where(t => t.UserID == UserID && t.Budgeted == YesNoOptions.Yes && t.Status == TargetStatus.Active ).ToList();

            foreach (var t in targets)
            {
                db.Entry(t).Reference(e => e.Budget).Load();
                tbas.Add(getTargetMonthlyBudget(db, t, dtNow, ConstantBudget));
            }
            return tbas;
        }
        public static TargetBudgetAmount getTargetMonthlyBudget(ApplicationDbContext db, Target t, DateTime dtNow, bool ConstantBudget)
        {
            TargetBudgetAmount tba = new TargetBudgetAmount { Name = t.Name, Budget = 0, Amount = 0 };

            if (t.TargetDate >= dtNow) // event to come
            {
                    if (t.Budget.BudgetDuration.GetHashCode() >= RepeatPattern.Monthly.GetHashCode()) // we will consider the first/current occurrence only
                    {
                        if (ConstantBudget)
                        {
                            double budget = 0;
                            switch (t.Budget.BudgetDuration)
                            {
                                case RepeatPattern.Monthly:
                                    budget = t.Budget.BudgetAmount;
                                    break;
                                case RepeatPattern.Bi_Monthly:
                                    budget = t.Budget.BudgetAmount / 2;
                                    break;
                                case RepeatPattern.Quarterly:
                                    budget = t.Budget.BudgetAmount / 3;
                                    break;
                                case RepeatPattern.Bi_Quarterly:
                                    budget = t.Budget.BudgetAmount / 6;
                                    break;
                                case RepeatPattern.Yearly:
                                    budget = t.Budget.BudgetAmount / 12;
                                    break;
                                default:
                                    break;
                            }
                            tba.Budget = budget;
                        }
                        
                    }//repeatable but repeats multiple times in a month. we will consider them as constants
                    else
                    {
                        double mdays = ConstantBudget ? 30 : dtNow.DaysInMonth();
                        double budget = 0;
                        if (ConstantBudget)
                        {
                            switch (t.Budget.BudgetDuration)
                            {
                                case RepeatPattern.Daily:
                                    budget = t.Budget.BudgetAmount * mdays;
                                    break;
                                case RepeatPattern.Alternate_Days:
                                    budget = t.Budget.BudgetAmount * mdays / 2;
                                    break;
                                case RepeatPattern.Weekly:
                                    budget = t.Budget.BudgetAmount * ((double)mdays / 7);
                                    break;
                                case RepeatPattern.Bi_Weekly:
                                    budget = t.Budget.BudgetAmount * ((double)mdays / 14);
                                    break;
                            }
                            tba.Budget = budget;
                        }
                    }
            }

            return tba;
        }
        public static List<EventBudgetAmount> getEventMonthlyBudgetAmount(ApplicationDbContext db,string UserID, int month, int year, bool ConstantBudget)
        {
            List<EventBudgetAmount> ebas = new List<EventBudgetAmount>();

            DateTime dtNow = new DateTime(year, month, 1);

            var events = db.Events.Where(e => e.UserID == UserID && e.Budgeted == YesNoOptions.Yes && e.EventDate >= dtNow);
            var fevents = db.EventUsers.Where(eu => eu.UserID == UserID && eu.Event.Budgeted == YesNoOptions.Yes && eu.Event.EventDate >= dtNow).Select(eu => eu.Event);
            var allevents = events.Union(fevents).ToList();

            foreach (var ev in allevents)
            {
                db.Entry(ev).Reference(e => e.Budget).Load();
                ebas.Add(getEventMonthlyBudget(db,UserID, ev, dtNow, ConstantBudget));
            }
            return ebas;
        }
        public static EventBudgetAmount getEventMonthlyBudget(ApplicationDbContext db,string UserID, Event ev, DateTime dtNow, bool ConstantBudget)
        {
            EventBudgetAmount eba = new EventBudgetAmount { Name = ev.Name, Budget = 0, Amount = 0 };

            if (ev.EventDate >= dtNow) // event to come
            {
                if (ev.Budget.BudgetDuration == RepeatPattern.None ) // no repeatition or repeatition after current month ( we will not consider the next occurrence for now)
                {
                    if (ConstantBudget)
                    {
                        var months = Util.GetMonthsBetweenDates(ev.CreatedDate, ev.EventDate);
                        eba.Budget = ev.Budget.BudgetAmount / months;
                    }
                    else
                    {
                        var prevMonExpenses = db.Expenses.Where(ex => ex.UserID == UserID && ex.EventID == ev.EventID && ex.ExpenseDate < dtNow).Select(ex => ex.Amount).DefaultIfEmpty(0).Sum(); // expenses already occurred
                        var evExpenses = db.Expenses.Where(ex => ex.UserID == UserID && ex.EventID == ev.EventID ).Select(ex => ex.Amount).DefaultIfEmpty(0).Sum(); // expenses already occurred

                        
                        if (ev.EventDate.Month == dtNow.Month) // it will occur in the current month
                        {
                            eba.Budget = ev.Budget.BudgetAmount;
                            eba.Amount = db.Expenses.Where(ex => ex.UserID == UserID && ex.EventID == ev.EventID).Select(ex => ex.Amount).DefaultIfEmpty(0).Sum(); // expenses already occurred
                        }
                        else
                        {
                            var tmonths = Util.GetMonthsBetweenDates(ev.CreatedDate, ev.EventDate);
                            var perMonthBudget = ev.Budget.BudgetAmount / tmonths;
                            var months = Util.GetMonthsBetweenDates(ev.CreatedDate, dtNow);
                            var currMonthBudget = perMonthBudget * months;
                            eba.Budget = currMonthBudget - prevMonExpenses;
                            eba.Amount = evExpenses;
                        }
                    }
                    
                }
                else // repeatable events
                {
                    if (ev.Budget.BudgetDuration.GetHashCode() >= RepeatPattern.Monthly.GetHashCode()) // we will consider the first/current occurrence only
                    {
                        if (ConstantBudget)
                        {
                            double budget = 0;
                            switch (ev.Budget.BudgetDuration)
                            {
                                case RepeatPattern.Monthly:
                                    budget = ev.Budget.BudgetAmount;
                                    break;
                                case RepeatPattern.Bi_Monthly:
                                    budget = ev.Budget.BudgetAmount/2;
                                    break;
                                case RepeatPattern.Quarterly:
                                    budget = ev.Budget.BudgetAmount/3;
                                    break;
                                case RepeatPattern.Bi_Quarterly:
                                    budget = ev.Budget.BudgetAmount/6;
                                    break;
                                case RepeatPattern.Yearly:
                                    budget = ev.Budget.BudgetAmount/12;
                                    break;
                                default:
                                    break;
                            }
                            eba.Budget = budget ;
                        }
                        else
                        {
                            var prevMonExpenses = db.Expenses.Where(ex => ex.UserID == UserID && ex.EventID == ev.EventID && ex.ExpenseDate < dtNow).Select(ex => ex.Amount).DefaultIfEmpty(0).Sum(); // expenses already occurred
                            var evExpenses = db.Expenses.Where(ex => ex.UserID == UserID && ex.EventID == ev.EventID).Select(ex => ex.Amount).DefaultIfEmpty(0).Sum(); // expenses already occurred

                            if (ev.EventDate.Month == dtNow.Month) // it will occur in the current month
                            {
                                eba.Budget = ev.Budget.BudgetAmount;
                                eba.Amount = db.Expenses.Where(ex => ex.UserID == UserID && ex.EventID == ev.EventID).Select(ex => ex.Amount).DefaultIfEmpty(0).Sum(); // expenses already occurred
                            }
                            else
                            {
                                DateTime PrevOccurrence = ev.CreatedDate;
                                var tmonths = Util.GetMonthsBetweenDates(ev.CreatedDate, ev.EventDate);
                                switch (ev.Budget.BudgetDuration)
                                {
                                    case RepeatPattern.Monthly:
                                        PrevOccurrence = ev.EventDate.AddMonths(-1); //subtract one month
                                        tmonths = 1;
                                        break;
                                    case RepeatPattern.Bi_Monthly:
                                        PrevOccurrence = ev.EventDate.AddMonths(-2); //subtract one month
                                        tmonths = 2;
                                        break;
                                    case RepeatPattern.Quarterly:
                                        PrevOccurrence = ev.EventDate.AddMonths(-3); //subtract one month
                                        tmonths = 3;
                                        break;
                                    case RepeatPattern.Bi_Quarterly:
                                        PrevOccurrence = ev.EventDate.AddMonths(-6); //subtract one month
                                        tmonths = 6;
                                        break;
                                    case RepeatPattern.Yearly:
                                        PrevOccurrence = ev.EventDate.AddYears(-1); //subtract one year
                                        tmonths = 12;
                                        break;
                                    default:
                                        break;
                                }
                                
                                var perMonthBudget = ev.Budget.BudgetAmount / tmonths;
                                double currMonthBudget = 0;

                                if (PrevOccurrence > dtNow) // special case where a future event is created like event date = after 6 month with repeat = 1 month
                                {
                                    PrevOccurrence = ev.CreatedDate;
                                }
                                else
                                {
                                    
                                }
                                var months = Util.GetMonthsBetweenDates(PrevOccurrence, dtNow);
                                if(months ==0) // no time is spent, all is remaining
                                {
                                    months = Util.GetMonthsBetweenDates(dtNow, ev.EventDate);
                                    currMonthBudget = ev.Budget.BudgetAmount / months;
                                }
                                else
                                 currMonthBudget = perMonthBudget * months;

                                eba.Budget = currMonthBudget - prevMonExpenses;
                                eba.Amount = evExpenses;
                            }
                        }
                    }//repeatable but repeats multiple times in a month. we will consider them as constants
                    else
                    {
                        double mdays = ConstantBudget ? 30 : dtNow.DaysInMonth();
                        double budget = 0;
                        if (ConstantBudget)
                        {
                            switch (ev.Budget.BudgetDuration)
                            {
                                case RepeatPattern.Daily:
                                    budget = ev.Budget.BudgetAmount * mdays;
                                    break;
                                case RepeatPattern.Alternate_Days:
                                    budget = ev.Budget.BudgetAmount * mdays / 2;
                                    break;
                                case RepeatPattern.Weekly:
                                    budget = ev.Budget.BudgetAmount * ((double)mdays / 7);
                                    break;
                                case RepeatPattern.Bi_Weekly:
                                    budget = ev.Budget.BudgetAmount * ((double)mdays / 14);
                                    break;
                            }
                            eba.Budget = budget;
                        }
                        else
                        {
                            if (ev.EventDate.Month == dtNow.Month)
                            {
                                int recurrence = getEventNextOccurenceCount(ev);
                                eba.Budget = ev.Budget.BudgetAmount * recurrence;
                            }
                            else
                                eba.Budget = 0;
                            var expenses = db.Expenses.Where(ex => ex.UserID == UserID && ex.EventID == ev.EventID).Select(ex => ex.Amount).DefaultIfEmpty(0).Sum(); // expenses already occurred
                            eba.Amount = expenses;
                        }

                        
                    }

                }
            }
           

            return eba;
        }
        private static int getEventNextOccurenceCount(Event ev)
        {
            int days = ev.EventDate.Day - ev.EventDate.DaysInMonth();
            int recurrence = days / ev.Budget.BudgetDuration.GetHashCode();
            return recurrence + 1;
        }
        private static int getEventOccurenceCount(Event ev, int remainingdays)
        {
            int count = 0;
            switch (ev.Budget.BudgetDuration)
            {
                case RepeatPattern.None:
                    break;
                case RepeatPattern.Daily:
                    count = remainingdays;
                    break;
                case RepeatPattern.Alternate_Days:
                    count = (int) Math.Floor(((double)remainingdays) / 2);;
                    break;
                case RepeatPattern.Weekly:
                    count = (int) Math.Floor(((double)remainingdays) / 7);
                    break;
                case RepeatPattern.Bi_Weekly:
                    count = (int)Math.Floor(((double)remainingdays) / 14);
                    break;
                case RepeatPattern.Monthly:
                    break;
                case RepeatPattern.Bi_Monthly:
                    break;
                case RepeatPattern.Quarterly:
                    break;
                case RepeatPattern.Bi_Quarterly:
                    break;
                case RepeatPattern.Yearly:
                    break;
                default:
                    break;
            }
            return count;
        }

        public static IEnumerable<Account> geAllUserAccounts(ApplicationDbContext db,string UserID, int? accountid = null)
        {
            var accounts = db.Accounts.Where(acc => (acc.UserID == UserID || acc.SharedFriends.FirstOrDefault(sf=>sf.UserID == UserID) != null) && (!accountid.HasValue || acc.AccountID == accountid));

            return accounts;
        }
        public static IEnumerable<Account> getAllUserAccountsWithCurrentAmount(ApplicationDbContext db,string UserID, bool considerInitialAmount = true)
        {
            var accounts = geAllUserAccounts(db, UserID).ToList();

            SetAccountCurrentAmount(db, accounts, considerInitialAmount);

            return accounts;
        }
        public static IEnumerable<Account> SetAccountCurrentAmount(ApplicationDbContext db, List<Account> accounts, bool considerInitialAmount = true)
        {
            //var accounts = db.Accounts.Where(acc => acc.UserID == State.UserID || acc.SharedFriends.FirstOrDefault(sf => sf.UserID == State.UserID) != null).ToList();
            foreach (var account in accounts)
            {
                var tExpenses = db.Expenses.Where(ex => ex.AccountID == account.AccountID).
                                            Select(ex => ex.Amount).DefaultIfEmpty(0).Sum();
                var tIncomes = db.Income.Where(inc => inc.AccountID == account.AccountID).
                                            Select(i => i.Amount).DefaultIfEmpty(0).Sum();
                var transferIns = db.AccountTransfers.Where(t => t.TargetAccountID == account.AccountID).
                                            Select(i => i.Amount).DefaultIfEmpty(0).Sum();
                var transferOuts = db.AccountTransfers.Where(t => t.SourceAccountID == account.AccountID).
                                            Select(i => i.Amount).DefaultIfEmpty(0).Sum();
                var savings = db.Savings.Where(s => s.AccountID == account.AccountID).
                                            Select(s => s.Amount).DefaultIfEmpty(0).Sum();

                account.CurrentAmount = tIncomes + transferIns - tExpenses - transferOuts - savings;
                if (considerInitialAmount)
                    account.CurrentAmount += account.InitialAmount;
            }
            return accounts;
        }

        public static T BadRequest<T>() where T : JsonResult
        {
            T jr = Activator.CreateInstance<T>();
            jr.Value = new StatusCodeResult(StatusCodes.Status400BadRequest);
            //jr.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jr;
        }
        public static JsonResult BadRequest(ResultType rt)
        {
            JsonResult jr = null;
            if (rt == ResultType.Web)
            {
                jr = new JsonResult(new StatusCodeResult(StatusCodes.Status400BadRequest));
            }
            else
            {
                jr = new JsonResult(new StatusCodeResult(StatusCodes.Status400BadRequest));
            }
            //jr.Data = ;
            return jr;
        }
    }
}