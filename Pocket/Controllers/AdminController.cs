using Microsoft.AspNet.Identity.EntityFramework;
using Pocket.Common;
using Pocket.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;

namespace Pocket.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private QDbContext db = new QDbContext();

        // GET: /Admin/
        public ActionResult Index()
        {
            return View();
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Assign()
        {
            var au = db.Users.Where(u => u.UserName == "Admin").SingleOrDefault();
            var ar = db.Roles.Where(r => r.Name == "Admin").SingleOrDefault();
            if (au != null && ar != null)
            {
                var ur = db.UserRoles.Where(aur => aur.UserId == au.Id && aur.RoleId == ar.Id).SingleOrDefault();
                if (ur == null) // Add if not already added.
                {
                    ur = new IdentityUserRole { UserId = au.Id, RoleId = ar.Id };
                    db.UserRoles.Add(ur);
                    db.SaveChanges();
                }
                
            }
            ViewBag.Message = "Admin role assigned.";

            return View("Index", null);
        }
        [ValidateAntiForgeryToken]
        [HttpPost]
        [Authorize(Roles="Admin")]
        public async Task<ActionResult> InitTestData()
        {
            
            await CreateTestData();
            ViewBag.Message = "Test Data created successfully.";
            return View("Index", null);
        }
        public JsonResult crequests(string sidx, string sord, int page, int rows)
        {
            
                var schedules = db.CustomerRequests;

                return Util.CreateJsonResponse<CustomerRequest>(sidx, sord, page, rows, schedules, (Func<IEnumerable<CustomerRequest>, Array>)delegate(IEnumerable<CustomerRequest> rd)
                {
                    return (
                        from cust in rd
                        select new
                        {
                            ScheduleID = cust.CustomerRequestID,
                            cell = new string[] { cust.CustomerRequestID.ToString(), cust.Name.ToString(), cust.Email, cust.CreatedDate.ToDateString(),
                                                  cust.Title,cust.Body}
                        }).ToArray();
                }
                    );
            

        }
        
        private async Task<bool> CreateTestData()
        {
            //Create users

            string user1 = await createUser();
            string user2 = await createUser();
            string user3 = await createUser();

            createUDefaults(user1);
            createUDefaults(user2);
            createUDefaults(user3);

            createCategories("u1 Category1", user1);
            createCategories("u1 shared Category", user1, user2 + "," + user3);
            createCategories("u1 subscribed Category", user2, user1);

            createAccounts("u1 account1", user1);
            createAccounts("u1 shared account",user1, user2 + "," + user3);
            createAccounts("u1 subscribed account", user2, user1);

            createEvents("u1 ev", user1);
            createEvents("u1 shared ev", user1, user2 + "," + user3);
            createEvents("u1 subscribed ev" , user2, user1);

            createExpenses(user1);
            createExpenses(user2);

            createIncomes(user1);
            createIncomes(user2);

            return true;
            //create targets
        }
        private async Task<string> createUser()
        {
            var UserManager = new ApplicationUserManager(new UserStore<ApplicationUser>(new QDbContext()));
            for (int i = 0; i < 100; i++)
            {
                string uname = "user" + i;
                if ((await UserManager.FindByNameAsync(uname)) == null)
                {
                    var user = new ApplicationUser() { UserName = uname, Email = string.Format("{0}@{0}.com", uname), EmailConfirmationSent = DateTime.Today.AddDays(-2) };
                    var result = await UserManager.CreateAsync(user, uname);
                    if (result.Succeeded)
                    {
                        return user.Id;
                    }
                }

            }
            throw new Exception("All user names occupied.");
        }
        private void createIncomes(string UserID)
        {
            var sources = db.IncomeSources.Where(s => s.UserID == UserID).ToList();

            var maccounts = db.Accounts.Where(a => a.UserID == UserID).ToList();
            var uaccaounts = db.AccountUsers.Where(ua => ua.UserID == UserID).Select(ua => ua.Account).ToList();
            var accounts = maccounts.Union(uaccaounts).ToList();

            foreach (var account in accounts)
            {
                foreach (var source in sources)
                {
                    Income inc = createIncome(UserID, account.AccountID, 10, DateTime.Now, source.SourceID);
                    Income prevInc = createIncome(UserID, account.AccountID, 10, DateTime.Now.AddMonths(-1), source.SourceID);
                    db.Income.Add(inc);
                    db.Income.Add(prevInc);
                }
            }

            db.SaveChanges();
        }
        private Income createIncome(string UserID, int AccountID, double amount, DateTime incomeDate, int SourceID)
        {
            Income inc = new Income
            {
                AccountID = AccountID,
                Amount = 10,
                IncomeDate = DateTime.Now,
                Repeat = RepeatPattern.None,
                SourceID = SourceID,
                UserID = UserID
            };
            return inc;
        }
        private void createExpenses(string UserID)
        {
            Payee payee = db.Payees.Where(p => p.UserID == UserID).FirstOrDefault();
            Vendor vendor = db.Vendors.Where(v => v.UserID == UserID).FirstOrDefault();

            var maccounts = db.Accounts.Where(a => a.UserID == UserID).ToList();
            var uaccaounts = db.AccountUsers.Where(ua => ua.UserID == UserID).Select(ua=>ua.Account).ToList();
            var accounts = maccounts.Union(uaccaounts).ToList();

            var mcats = db.Categories.Where(c => c.UserID == UserID).ToList();
            var ucats = db.CategoryUsers.Where(cu => cu.UserID == UserID).Select(cu=>cu.Category).ToList();
            var cats = mcats.Union(ucats).ToList();

            var mevents = db.Events.Where(e => e.UserID == UserID).ToList();
            var uevents = db.EventUsers.Where(eu => eu.UserID == UserID).Select(eu=>eu.Event).ToList();
            var events = mevents.Union(uevents).ToList();

            foreach (var cat in cats)
            {
                foreach (var scat in cat.Subcategories.ToList())
                {
                    foreach (var account in accounts)
                    {
                        // current month expense
                        db.Expenses.Add( CreateExpense(account.AccountID, "cat expense", null, DateTime.Now, payee.PayeeID, scat.SubcategoryID, UserID, vendor.VendorID, RepeatPattern.None));
                        // last month expense
                        db.Expenses.Add(CreateExpense(account.AccountID, "cat expense", null, DateTime.Now.AddMonths(-1), payee.PayeeID, scat.SubcategoryID, UserID, vendor.VendorID, RepeatPattern.None));
                        foreach (var ev in events)
                        {
                            // current month expense
                            db.Expenses.Add(CreateExpense(account.AccountID, "event expense", ev.EventID, DateTime.Now, payee.PayeeID, scat.SubcategoryID, UserID, vendor.VendorID, RepeatPattern.None));
                            // last month expense
                            db.Expenses.Add(CreateExpense(account.AccountID, "event expense", ev.EventID, DateTime.Now.AddMonths(-1), payee.PayeeID, scat.SubcategoryID, UserID, vendor.VendorID, RepeatPattern.None));
                        }
                    }
                }
            }

            db.SaveChanges();
        }
        private Expense CreateExpense(int AccountID,string description, int? eventID, DateTime expenseDate, int payeeID, int subcatID, string UserID, int vendorID, RepeatPattern repeat)
        {
            Expense ex = new Expense
            {
                AccountID = AccountID,
                Amount = 1,
                CreatedDate = DateTime.Today,
                Description = description,
                EventID = eventID,
                 ExpenseDate = expenseDate,
                 ModifiedDate = DateTime.Now,
                 PayeeID = payeeID,
                 Repeat = repeat,
                SubcategoryID = subcatID,
                UserID = UserID,
                 VendorID = vendorID
            };
            return ex;
        }
        private void createEvents(string name, string UserID, string sharedWith = "")
        {
            List<Event> events = new List<Event>();
            events.Add( new Event { Name = name + " B", UserID = UserID, Amount=100, Budgeted= YesNoOptions.Yes, CreatedDate = DateTime.Now, EventDate = DateTime.Now.AddMonths(1), EventStatus= EventStatus.Active });
            events.Add(new Event { Name = name + " NB", UserID = UserID, Amount = 100, Budgeted = YesNoOptions.No, CreatedDate = DateTime.Now, EventDate = DateTime.Now.AddMonths(1), EventStatus = EventStatus.Active });
            events.Add(new Event { Name = name + " B Expired", UserID = UserID, Amount = 100, Budgeted = YesNoOptions.Yes, CreatedDate = DateTime.Now, EventDate = DateTime.Today.AddDays(-1), EventStatus = EventStatus.Expired});
            events.Add(new Event { Name = name + " B Expired", UserID = UserID, Amount = 100, Budgeted = YesNoOptions.Yes, CreatedDate = DateTime.Now, EventDate = DateTime.Today.AddDays(-1), EventStatus = EventStatus.Active });


            
            foreach (var ev in events)
            {
                db.Events.Add(ev);
                foreach (var friendid in sharedWith.Split(','))
                {
                    EventUser eu = new EventUser { Event = ev, UserID = friendid };
                    db.EventUsers.Add(eu);
                }
            }

            db.SaveChanges();
        }
        private void createAccounts(string name, string UserID, string sharedWith = "")
        {
            Account acc = new Account { Name = name, AccountType = AccountType.Debit, InitialAmount = 0, UserID = UserID };
            db.Accounts.Add(acc);

            foreach (var friendid in sharedWith.Split(','))
            {
                AccountUser au = new AccountUser { Account = acc, UserID = friendid };
                db.AccountUsers.Add(au);
            }
            db.SaveChanges();
        }
        private void createCategories(string name, string UserID, string sharedWith = "")
        {
            Category cat = new Category { Name = name, UserID = UserID, Display = DisplaySetting.Yes };
            cat.Subcategories = new List<Subcategory>();

            foreach (var item in Enum.GetValues(typeof(RepeatPattern)).Cast<RepeatPattern>())
            {
                Subcategory scat = new Subcategory { Name = item.String() };
                scat.Budget = new Budget { BudgetAmount = item.GetHashCode(), BudgetDuration = item, BudgetType = BudgetType.Sub_Category, UserID = UserID };
                cat.Subcategories.Add(scat);
            }
            db.Categories.Add(cat);
            foreach (var friendid in sharedWith.Split(','))
            {
                CategoryUser cu = new CategoryUser { Category = cat, UserID = friendid };
                db.CategoryUsers.Add(cu);
            }
            db.SaveChanges();
        }

        private void createUDefaults(string UserID)
        {
            
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

            db.Accounts.Add(new Account { Name = "Checking", UserID = UserID, InitialAmount = 0, AccountType = AccountType.Debit });
            db.Accounts.Add(new Account { Name = "Saving", UserID = UserID, InitialAmount = 0, AccountType = AccountType.Saving });
            db.Accounts.Add(new Account { Name = "Credit Card", UserID = UserID, InitialAmount = 0, AccountType = AccountType.Credit });

            db.Notifications.Add(new Notification { GeneratedBy = UserID, UserID = UserID, NotificationDate = DateTime.Now, Text = "Default Categories created. Please add/edit as per your requirement", URL = "/Category/", MobileURL = "/Mobile/CategoryList", Title = "Category Added" });
            db.Notifications.Add(new Notification { GeneratedBy = UserID, UserID = UserID, NotificationDate = DateTime.Now, Text = "Default Payees created. Please add/edit as per your requirement", URL = "/Payee/", MobileURL = "/Mobile/PayeeList", Title = "Payee Added" });
            db.Notifications.Add(new Notification { GeneratedBy = UserID, UserID = UserID, NotificationDate = DateTime.Now, Text = "Default Income Sources created. Please add/edit as per your requirement", URL = "/IncomeSource/", MobileURL = "/Mobile/IncomeSourceList", Title = "Income Source Added" });
            db.Notifications.Add(new Notification { GeneratedBy = UserID, UserID = UserID, NotificationDate = DateTime.Now, Text = "Default Accounts created. Please add/edit as per your requirement", URL = "/QAccount/", MobileURL = "/Mobile/AccountList", Title = "Account Added" });
            db.Notifications.Add(new Notification { GeneratedBy = UserID, UserID = UserID, NotificationDate = DateTime.Now, Text = "Default Vendors created. Please add/edit as per your requirement", URL = "/Vendor/", MobileURL = "/Mobile/VendorList", Title = "Vendor Added" });

            db.SaveChanges();
        }
	}
}