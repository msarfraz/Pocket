using Pocket.Common;
using Pocket.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Pocket.ViewModels;

namespace Pocket.Controllers
{
    [Authorize]
    public class ReportController : ApplicationController
    {
        public ReportController(ApplicationDbContext
            context) : base(context)
        {
            
        }
        // GET: /Report/AdvanceFilter
        public ActionResult AdvanceFilter()
        {
            ApplicationUser u = db.Users.Find(UserID);
            var fevents = db.EventUsers.Where(eu => eu.UserID == UserID).Select(eu => eu.Event).ToList();
            ReportData model = new ReportData
            {
                Accounts = u.Accounts,
                OtherAccounts = u.OtherAccounts.Select(oa=>oa.Account).ToList(),
                Categories = u.Categories,
                OtherCategories = u.OtherCategories.Select(oc=>oc.Category).ToList(),
                Payees = u.Payees,
                Vendors = u.Vendors,
                MyEvents = u.Events,
                SharedEvents = fevents
            };
            return View(model);
        }
        private IQueryable<Expense> GetAllAccessibleExpenses()
        {
            IQueryable<Expense> expenses = db.Expenses;
            var accounts = db.Accounts.Where(acc => acc.UserID == UserID).Union(db.AccountUsers.Where(acc => acc.UserID == UserID).Select(acc => acc.Account));
            var aexpenses = expenses.Join(accounts, acc => acc.AccountID, ex => ex.AccountID, (ex, acc) => ex);

            var cats = db.Categories.Where(cat => cat.UserID == UserID).Union(db.CategoryUsers.Where(cat => cat.UserID == UserID).Select(cat => cat.Category));
            
            var cexpenses = expenses.Join(cats, ex => ex.Subcategory.CategoryID, cat => cat.CategoryID, (ex, cat) => ex);

            var events = db.Events.Where(ev => ev.UserID == UserID).Union(db.EventUsers.Where(ev => ev.UserID == UserID).Select(ev => ev.Event));
            var eexpenses = expenses.Join(events, ex => ex.EventID, ev => ev.EventID, (ex, ev) => ex);
            expenses = aexpenses.Union(cexpenses).Union(eexpenses);
            return expenses;
        }
        public JsonResult MAdvanceReport(int page, int rows,
            int? AccountID, int? CatID, int? SubcatID, int? PayeeID, int? VendorID, int? EventID, DateTime? FromDate,
            DateTime? ToDate, bool? AllUsers)
        {
            IQueryable<Expense> expenses = GetAdvanceExpenses(page, rows, AccountID, CatID, SubcatID, PayeeID, VendorID,
                    EventID, FromDate, ToDate, AllUsers.HasValue ? AllUsers.Value : true);
            var gexp = from expense in expenses
                       group expense by expense.ExpenseDate into g
                       orderby g.Key descending
                       select new
                       {
                           ExpenseDateText = g.Key.Day + "-" + g.Key.Month + "-" + g.Key.Year,
                           TotalAmount = g.Select(ex => ex.Amount).DefaultIfEmpty(0).Sum(),
                           Expenses = g.Select
                           (ex => new
                           {
                               ExpenseDate = ex.ExpenseDate.Day + "-" + ex.ExpenseDate.Month + "-" + ex.ExpenseDate.Year,
                               Amount = ex.Amount,
                               AccountText = ex.Account.Name,
                               CategoryText = ex.Subcategory.Category.Name,
                               SubcategoryText = ex.Subcategory.Name,
                               ExpenseID = ex.ExpenseID,
                               Editable = ex.UserID == UserID,
                               UserName = ex.UserID == UserID ? "" : ex.User.UserName
                           })
                       };
            int totalRecords = gexp.Count();
            int pageIndex = (page > 0 ? page : 1) - 1;
            int pageSize = rows > 0 ? rows : 5;
            int totalPages = (int)Math.Ceiling((float)totalRecords / (float)pageSize);

            var result = gexp.Skip(pageIndex * pageSize).Take(pageSize).ToList();
            return Util.Package<JsonResult>(result, totalRecords, totalPages, pageIndex);

            
        }
        public JsonResult JAdvanceFilter(string sidx, string sord, int page, int rows)
        {
            int? AccountID = Util.ToInt(Request.Form["AccountID"]);
            int? CatID = Util.ToInt(Request.Form["CatID"]);
            int? SubcatID = Util.ToInt(Request.Form["SubcatID"]);
            int? PayeeID = Util.ToInt(Request.Form["PayeeID"]);
            int? VendorID = Util.ToInt(Request.Form["VendorID"]);
            int? EventID = Util.ToInt(Request.Form["EventID"]);
            DateTime? FromDate = Util.ToDateTime(Request.Form["ExpenseFrom"]);
            DateTime? ToDate = Util.ToDateTime(Request.Form["ExpenseTo"]);
            bool AllUsers = Util.ToBool(Request.Form["AllUsers"]);

            return AdvanceReport(sidx, sord, page, rows, AccountID, CatID, SubcatID, PayeeID, VendorID,
                EventID, FromDate, ToDate, AllUsers, ResultType.Web);
        }
        private JsonResult AdvanceReport(string sidx, string sord, int page, int rows,
            int? AccountID,int? CatID, int? SubcatID, int? PayeeID, int? VendorID, int? EventID, DateTime? FromDate,
            DateTime? ToDate, bool AllUsers, ResultType rt)
        {
                string userid = UserID;

                IQueryable<Expense> expenses = GetAdvanceExpenses(page, rows, AccountID, CatID, SubcatID, PayeeID, VendorID,
                    EventID, FromDate, ToDate, AllUsers);
            

            return Util.CreateJsonResponse<Expense>(sidx, sord, page, rows, expenses,rt, (Func<IEnumerable<Expense>, Array>)delegate(IEnumerable<Expense> rd)
            {
                if (rt == ResultType.Web)
                {
                    return (
                    from expense in rd
                    select new
                    {
                        ExpenseID = expense.ExpenseID,
                        cell = new string[] { expense.ExpenseID.ToString(), expense.ExpenseDate.ToDateString(), expense.Subcategory.Category.Name + " - " + expense.Subcategory.Name + (expense.Subcategory.Category.UserID == UserID ? "" : string.Format(" [{0}]", expense.Subcategory.Category.User.UserName))
                            , expense.Account.Name + (expense.Account.UserID == UserID ? "" : string.Format(" [{0}]", expense.Account.User.UserName)), 
                            expense.Amount.ToString(), 
                                expense.PayeeID.HasValue ? expense.Payee.Name : "",
                            expense.VendorID.HasValue ? expense.Vendor.Name : "",
                            expense.EventID.HasValue ? expense.Event.Name + (expense.Account.UserID == UserID ? "" : string.Format(" [{0}]", expense.Account.User.UserName)) : "",
                            expense.Repeat.String(), expense.Comments.Count + " comments", expense.User.UserName }
                    }).ToArray();
                }
                else
                {
                    return (
                    from ex in rd
                    select new
                    {
                        ExpenseID = ex.ExpenseID,
                        AccountText = ex.Account.Name,
                        Amount = ex.Amount,
                        Description = ex.Description,
                        EventText = ex.EventID.HasValue ? ex.Event.Name : "",
                        ExpenseDate = ex.ExpenseDate.ToDateDisplayString(),
                        PayeeText = ex.PayeeID.HasValue ? ex.Payee.Name : "",
                        CategoryText = ex.Subcategory.Category.Name,
                        SubcategoryText = ex.Subcategory.Name,
                        VendorText = ex.VendorID.HasValue ? ex.Vendor.Name : "",
                        RepeatText = ex.Repeat.String(),
                        Editable = ex.UserID == userid
                    }).ToArray();
                }
            }
               );
        }
        private IQueryable<Expense> GetAdvanceExpenses(int page, int rows,
            int? AccountID, int? CatID, int? SubcatID, int? PayeeID, int? VendorID, int? EventID, DateTime? FromDate,
            DateTime? ToDate, bool AllUsers)
        {
            string userid = UserID;

            IQueryable<Expense> expenses = null;
            if (!AllUsers)
                expenses = db.Expenses.Where(exp => exp.UserID == UserID);
            else
                expenses = GetAllAccessibleExpenses();


            if (AccountID.HasValue)
            {
                expenses = expenses.Where(exp => exp.AccountID == AccountID);

            }

            if (CatID.HasValue)
            {
                expenses = expenses.Where(exp => exp.Subcategory.CategoryID == CatID);
            }
            if (SubcatID.HasValue)
            {
                expenses = expenses.Where(exp => exp.SubcategoryID == SubcatID);
            }


            if (EventID.HasValue)
            {
                expenses = expenses.Where(exp => exp.EventID == EventID);
            }

            if (PayeeID.HasValue)
                expenses = expenses.Where(exp => exp.PayeeID == PayeeID);
            if (VendorID.HasValue)
                expenses = expenses.Where(exp => exp.VendorID == VendorID);
            if (FromDate.HasValue)
                expenses = expenses.Where(exp => exp.ExpenseDate >= FromDate);
            if (ToDate.HasValue)
                expenses = expenses.Where(exp => exp.ExpenseDate <= ToDate);

            return expenses;
        }
        // GET: /Report/Category
        public ActionResult GroupReport()
        {
            return View(db.Users.Find(UserID).Expenses);
        }
        // GET: /JCategory/
        public JsonResult JGroupReport(string sidx, string sord, int page, int rows)
        {
                int month = Util.ParseInt(Request.Form["Month"]);
                int year = Util.ParseInt(Request.Form["Year"]);

                var expenses = GetAllAccessibleExpenses();
                expenses = expenses.Where(ex => ex.ExpenseDate.Month == month && ex.ExpenseDate.Year == year);

                return Util.CreateJsonResponse<Expense>(sidx, sord, page, rows, expenses, (Func<IEnumerable<Expense>, Array>)delegate(IEnumerable<Expense> rd)
                {
                    return (
                        from expense in rd
                        select new
                        {
                            ExpenseID = expense.ExpenseID,
                            cell = new string[] { expense.ExpenseID.ToString(),string.Empty, expense.ExpenseDate.ToDateString(), 
                                expense.Subcategory.Category.Name + " - " + expense.Subcategory.Name + (expense.Subcategory.Category.UserID == UserID ? "" : string.Format(" [{0}]", expense.Subcategory.Category.User.UserName)), 
                                expense.Account.Name + (expense.Account.UserID == UserID ? "" : string.Format(" [{0}]", expense.Account.User.UserName)), 
                                expense.Amount.ToString(), 
                                expense.PayeeID.HasValue ? expense.Payee.Name : "",
                            expense.VendorID.HasValue ? expense.Vendor.Name : "",
                            expense.EventID.HasValue ? expense.Event.Name + (expense.Event.UserID == UserID ? "" : string.Format(" [{0}]", expense.Event.User.UserName)) : "",
                            expense.User.UserName
                            }
                        }).ToArray();
                }
                    );

        }

        // GET: /Report/CategoryReport
        public ActionResult CategoryReport()
        {
            return View();
        }
        // GET: /JCategory/
        public JsonResult MCategoryReport(int Month, int Year)
        {
            string userid = UserID;

            List<CategoryExpense> catsba = new List<CategoryExpense>();
            var cats = db.Categories.Where(c => c.UserID == UserID); // db.Users.Find(UserID).Categories;
            var ucats = db.CategoryUsers.Where(uc => uc.UserID == UserID).Select(uc => uc.Category);
            cats = cats.Union(ucats);

            foreach (var cat in cats.ToList())
            {
                var cba = new CategoryExpense();
                cba.Name = cat.Name + (cat.UserID == UserID ? "" : string.Format(" [{0}]", cat.User.UserName));
                cba.CategoryID = cat.CategoryID;
                foreach (var scat in cat.Subcategories)
                {
                    double sbudget = Repository.GetSubcatMonthlyBudget(db, UserID,scat, Month, Year, false);
                    var expenses = Repository.GetSubcatExpenses(db, Month, Year, scat.SubcategoryID).ToList();
                    double samount = expenses.Select(ex => ex.Amount).DefaultIfEmpty(0).Sum();
                    if (sbudget > 0 || samount > 0)
                        cba.Subcategories.Add(new SubcategoryExpense { Name = scat.Name, Budget = sbudget, Amount = samount, Expenses=  expenses});
                }
                cba.Calculate();
                catsba.Add(cba);
            }
            return Util.Package<JsonResult>(catsba.Select(cat => new
            {
                Name = cat.Name,
                Budget = cat.Budget,
                Amount = cat.Amount,
                Subcategories = cat.Subcategories.Select(scat => new
                {
                    Name = scat.Name,
                    Budget = scat.Budget,
                    Amount = scat.Amount,
                    Expenses = scat.Expenses.Select(ex => new {
                        ExpenseID = ex.ExpenseID,
                        AccountID = ex.AccountID,
                        AccountText = ex.Account.Name,
                        Amount = ex.Amount,
                        Description = ex.Description,
                        EventID = ex.EventID,
                        EventText = ex.EventID.HasValue ? ex.Event.Name : "",
                        ExpenseDate = ex.ExpenseDate.ToUTCDateString(),
                        ExpenseDateText = ex.ExpenseDate.ToDateDisplayString(),
                        PayeeID = ex.PayeeID,
                        PayeeText = ex.PayeeID.HasValue ? ex.Payee.Name : "",
                        CategoryID = ex.Subcategory.CategoryID,
                        CategoryText = ex.Subcategory.Category.Name,
                        SubcategoryID = ex.SubcategoryID,
                        SubcategoryText = ex.Subcategory.Name,
                        VendorID = ex.VendorID,
                        VendorText = ex.VendorID.HasValue ? ex.Vendor.Name : "",
                        Repeat = ex.Repeat.GetHashCode(),
                        RepeatText = ex.Repeat.String(),
                        Editable = ex.UserID == userid
                    })
                })
            })); ;

            //var result = expenses.GroupBy(ex => new { ex.Subcategory.CategoryID, ex.SubcategoryID }, ex => ex, (key, g) => new { cid = key.CategoryID, sid = key.SubcategoryID, exps = g.ToList() }).ToList();
            
        }
        public JsonResult JCategoryReport(string sidx, string sord, int page, int rows)
        {
                int month = Util.ParseInt(Request.Form["Month"]);
                int year = Util.ParseInt(Request.Form["Year"]);
                ExpenseDepth expenseDepth = (ExpenseDepth) Util.ParseInt(Request.Form["ExpenseDepth"]);

                var expenses = db.Expenses.Where(ex => ex.ExpenseDate.Month == month && ex.ExpenseDate.Year == year);
                var categories = db.Categories.Where(c => c.UserID == UserID); // User Categories
                var ucategories = db.CategoryUsers.Where(cu => cu.UserID == UserID).Select(cu => cu.Category); // Categories shared by other users
                

                
                
                switch (expenseDepth)
                {
                    case ExpenseDepth.MyExpenses: // All expenses made by me either in my categories or shared categories
                        expenses = expenses.Where(ex => ex.UserID == UserID);
                        break;
                    case ExpenseDepth.OtherExpensesMySharedCategories: // Expenses made by other usres in my shared categories
                        expenses = expenses.Join(categories, ex => ex.Subcategory.CategoryID, c => c.CategoryID, (ex, c) => ex); // all expenses made by different users
                        break;
                    case ExpenseDepth.OtherExpensesOtherSharedCategories: // Expenses made by other users in the categories which I subscribed
                        categories = categories.Union(ucategories); // all categories on which current user has access
                        expenses = expenses.Join(categories, ex => ex.Subcategory.CategoryID, c => c.CategoryID, (ex, c) => ex); // all expenses made by different users
                        break;
                    default:
                        break;
                }

                //'level', 'parent', 'isLeaf', 'expanded', 'loaded'
                return Util.CreateJsonResponse<Expense>(sidx, sord, page, rows, expenses, (Func<IEnumerable<Expense>, Array>)delegate(IEnumerable<Expense> rd)
                {
                    var result = Enumerable.Empty<object>().Select(r => new { id = "0", cell = new string[] { } }).ToList();
                    
                    List<int> cats = new List<int>();
                    List<int> scats = new List<int>();
                    int id=1;
                    KeyValuePair<string, int> kp = new KeyValuePair<string, int>();
                    Hashtable ht = new Hashtable();
                    rd = rd.OrderBy(exp => exp.Subcategory.CategoryID).ThenBy(exp => exp.SubcategoryID);

                    for (int i = 0; i < rd.Count(); i++)
                    {

                        Expense expense = rd.ElementAt(i);
                        if (!cats.Contains(expense.Subcategory.CategoryID))
                        {
                            ht.Add(expense.Subcategory.CategoryID.ToString(), id++);
                            cats.Add(expense.Subcategory.CategoryID); 
                            result.Add(new
                            {
                                id = ht[expense.Subcategory.CategoryID.ToString()].ToString(),
                                cell = new string[] { ht[expense.Subcategory.CategoryID.ToString()].ToString(), 
                                                    expense.Subcategory.Category.Name + (expense.Subcategory.Category.UserID == UserID ? "" : string.Format(" [{0}]", expense.Subcategory.Category.User.UserName)), 
                                                    string.Empty, 
                                                    string.Empty, 
                                                    string.Empty,
                                                    string.Empty,
                                                    string.Empty,
                                                    string.Empty,
                                                    string.Empty, //User Name
                                                    string.Empty, // Catsum
                                                    "0", "null", "false", "true"
                            
                                                }
                            });
                            
                        }
                        if (!scats.Contains(expense.SubcategoryID))
                        {
                            ht.Add(expense.Subcategory.CategoryID + "_" + expense.SubcategoryID, id++);
                            scats.Add(expense.SubcategoryID);

                            result.Add(new
                            {
                                id = ht[expense.Subcategory.CategoryID + "_" + expense.SubcategoryID].ToString(),
                                cell = new string[] { ht[expense.Subcategory.CategoryID + "_" + expense.SubcategoryID].ToString(), 
                                                    expense.Subcategory.Name, 
                                                    string.Empty, 
                                                    string.Empty, 
                                                    expense.Subcategory.Budget.BudgetAmount.ToString(),
                                                    string.Empty,
                                                    string.Empty,
                                                    string.Empty,
                                                    string.Empty,//User Name
                                                    string.Empty,// Catsum
                                                    "1", ht[expense.Subcategory.CategoryID.ToString()].ToString(), "false", "false"
                            
                                                }
                            });

                            var categ = result.Find(a => a.id.Equals(ht[expense.Subcategory.CategoryID.ToString()].ToString()));
                            categ.cell[4] = (Util.ParseInt(categ.cell[4]) + expense.Subcategory.Budget.BudgetAmount).ToString();
                        }
                        ht.Add(expense.Subcategory.CategoryID + "_" + expense.SubcategoryID + "_" + expense.ExpenseID, id++);
                        result.Add(new
                                            {
                                                id = ht[expense.Subcategory.CategoryID + "_" + expense.SubcategoryID + "_" + expense.ExpenseID].ToString(),
                                                cell = new string[] { ht[expense.Subcategory.CategoryID + "_" + expense.SubcategoryID + "_" + expense.ExpenseID].ToString(), 
                                                    expense.ExpenseDate.ToDateString(), 
                                                    expense.Account.Name, 
                                                    expense.Amount.ToString(), 
                                                    string.Empty,
                                                    expense.PayeeID.HasValue ? expense.Payee.Name : "",
                                                    expense.VendorID.HasValue ? expense.Vendor.Name : "",
                                                    expense.EventID.HasValue ? expense.Event.Name : "",
                                                    expense.User.UserName,
                                                    expense.Amount.ToString(), // its used for sum purpose only
                                                    "2", ht[expense.Subcategory.CategoryID + "_"  + expense.SubcategoryID].ToString(), "true", "false"
                            
                                                }
                                            });
                        var cat = result.Find(a => a.id.Equals(ht[expense.Subcategory.CategoryID.ToString()].ToString()));
                        cat.cell[3] = (Util.ParseInt(cat.cell[3]) + expense.Amount).ToString();
                        var scat = result.Find(a => a.id.Equals(ht[expense.Subcategory.CategoryID + "_" + expense.SubcategoryID.ToString()].ToString()));
                        scat.cell[3] = (Util.ParseInt(scat.cell[3]) + expense.Amount).ToString();
                    }
                    return result.ToArray();
                }
                    );

        }
        // Get: Report/Event/id
        public ActionResult EventReport(int? id, int? ExpenseID)
        {
            var myevents = db.Users.Find(UserID).Events;
            var fevents = db.EventUsers.Where(eu => eu.UserID == UserID).Select(eu => eu.Event).ToList();

            Tuple<List<Event>, List<Event>,int?, int?> model = new Tuple<List<Event>, List<Event>,int?,int?>(myevents, fevents, id, ExpenseID);
            
            return View(model);
        }
        private List<Event> GetAllEvents()
        {
            var mevents = db.Events.Where(e=>e.UserID == UserID);
            var fevents = db.EventUsers.Where(eu => eu.UserID == UserID).Select(eu => eu.Event);
            return mevents.Union(fevents).ToList();
        }
        public JsonResult MEventReport(int page, int rows, int eventid)
        {
            var expenses = db.Expenses.Where(ex => ex.EventID == eventid);
            var gexp = from expense in expenses
                       group expense by expense.ExpenseDate into g
                       orderby g.Key descending
                       select new
                       {
                           ExpenseDateText = g.Key.Day + "-" + g.Key.Month + "-" + g.Key.Year,
                           TotalAmount = g.Select(ex => ex.Amount).DefaultIfEmpty(0).Sum(),
                           Expenses = g.Select
                           (ex => new
                           {
                               ExpenseDate = ex.ExpenseDate.Day + "-" + ex.ExpenseDate.Month + "-" + ex.ExpenseDate.Year,
                               Amount = ex.Amount,
                               AccountText = ex.Account.Name,
                               CategoryText = ex.Subcategory.Category.Name,
                               SubcategoryText = ex.Subcategory.Name,
                               ExpenseID = ex.ExpenseID,
                               Editable = ex.UserID == UserID,
                               UserName = ex.UserID == UserID ? "" : ex.User.UserName
                           })
                       };
            int totalRecords = gexp.Count();
            int pageIndex = (page>0 ? page : 1) - 1;
            int pageSize = rows > 0 ? rows : 5;
            int totalPages = (int)Math.Ceiling((float)totalRecords / (float)pageSize);

            var result = gexp.Skip(pageIndex * pageSize).Take(pageSize).ToList();
            return Util.Package<JsonResult>(result, totalRecords, totalPages, pageIndex);
        }
        public JsonResult JEventReport(string sidx, string sord, int page, int rows)
        {
            int eventid = Util.ParseInt(Request.Form["EventID"]);
            if (GetAllEvents().Find(ev => ev.EventID == eventid) == null)
            {
                eventid = 0;
            }
            return EventReport(sidx, sord, page, rows, eventid, ResultType.Web);
        }

        private JsonResult EventReport(string sidx, string sord, int page, int rows, int eventid, ResultType rt)
        {
                string userid = UserID;

                var expenses = db.Expenses.Where(ex => ex.EventID == eventid);

                return Util.CreateJsonResponse<Expense>(sidx, sord, page, rows, expenses,rt, (Func<IEnumerable<Expense>, Array>)delegate(IEnumerable<Expense> rd)
                {
                    if (rt == ResultType.Web)
                    {
                        return (
                        from expense in rd
                        select new
                        {
                            ExpenseID = expense.ExpenseID,
                            cell = new string[] { expense.ExpenseID.ToString(), expense.ExpenseDate.ToDateString(), expense.Subcategory.Category.Name + " -- " + expense.Subcategory.Name, expense.Account.Name, expense.Amount.ToString(), 
                                expense.PayeeID.HasValue ? expense.Payee.Name : "",
                            expense.VendorID.HasValue ? expense.Vendor.Name : "", expense.Comments.Count + " comment(s)", expense.User.UserName}
                        }).ToArray();
                    }
                    else
                    {
                        return (
                        from ex in rd
                        select new
                        {
                            ExpenseID = ex.ExpenseID,
                            AccountID = ex.AccountID,
                            AccountText = ex.Account.Name,
                            Amount = ex.Amount,
                            Description = ex.Description,
                            EventID = ex.EventID,
                            EventText = ex.EventID.HasValue ? ex.Event.Name : "",
                            ExpenseDate = ex.ExpenseDate.ToDateDisplayString(),
                            PayeeID = ex.PayeeID,
                            PayeeText = ex.PayeeID.HasValue ? ex.Payee.Name : "",
                            CategoryID = ex.Subcategory.CategoryID,
                            CategoryText = ex.Subcategory.Category.Name,
                            SubcategoryID = ex.SubcategoryID,
                            SubcategoryText = ex.Subcategory.Name,
                            VendorID = ex.VendorID,
                            VendorText = ex.VendorID.HasValue ? ex.Vendor.Name : "",
                            Repeat = ex.Repeat.GetHashCode(),
                            RepeatText = ex.Repeat.String(),
                            Editable = ex.UserID == userid
                        }).ToArray();
                    }
                }
                    );

        }
        public JsonResult JEventShareReport(string sidx, string sord, int page, int rows)
        {
                int eventid = Util.ParseInt(Request.Form["EventID"]);
                if (GetAllEvents().Find(ev => ev.EventID == eventid) == null)
                {
                    eventid = 0;
                }
                var expenses = db.Expenses.Where(ex => ex.EventID == eventid);
                
                expenses.GroupBy(ex => ex.UserID);

                return Util.CreateJsonResponse<Expense>(sidx, sord, page, rows, expenses, (Func<IEnumerable<Expense>, Array>)delegate(IEnumerable<Expense> rd)
                {
                    var result = from ex in rd
                                 join u in db.Users on ex.UserID equals u.Id
                                 group ex.Amount by new { u.UserName, u.Id } into g
                                 select new
                                 {
                                     UserID = g.Key.Id,
                                     cell = new string[]{g.Key.Id, g.Key.UserName, g.Sum(s => s).ToString()}
                                 };
                    return result.ToArray();
                    /*return (
                        from expense in rd
                        select new
                        {
                            ExpenseID = expense.ExpenseID,
                            cell = new string[] { expense.ExpenseID.ToString(), expense.ExpenseDate.ToDateString(), expense.Subcategory.Category.Name + " -- " + expense.Subcategory.Name, expense.Account.Name, expense.Amount.ToString(), 
                                expense.PayeeID.HasValue ? expense.Payee.Name : "",
                            expense.VendorID.HasValue ? expense.Vendor.Name : "", expense.Comments.Count + " comments"}
                        }).ToArray();*/
                }
                    );

        }
        // Get: Report/AccountReport/id
        public ActionResult AccountReport(int? id)
        {
            ApplicationUser u = db.Users.Find(UserID);
            
            List<Account> myAccounts = u.Accounts.ToList();
            var otherAccounts = u.OtherAccounts.Select(oa => oa.Account).ToList();

            if (!id.HasValue) //no value specified
                    id = myAccounts.Count > 0 ? myAccounts.First().AccountID : (otherAccounts.Count > 0 ? otherAccounts.First().AccountID : 0);

            Tuple<List<Account>,List<Account>, int> model = new Tuple<List<Account> ,List<Account>,int>(myAccounts, otherAccounts, id.Value);

            return View(model);
        }
        public JsonResult MAccountReport(string sidx, string sord, int page, int rows, int AccountID, DateTime From, DateTime To)
        {
            return (JsonResult) AccountReport("", "", page, rows, ResultType.Mobile, AccountID, From, To);
        }
        public JsonResult JAccountReport(string sidx, string sord, int page, int rows)
        {
            int accountid = Util.ParseInt(Request.Form["AccountID"]);
            int Month = Util.ParseInt(Request.Form["Month"]);
            int Year = Util.ParseInt(Request.Form["Year"]);
            DateTime From = new DateTime(Year,Month, 1);

            return AccountReport(sidx, sord, page, rows, ResultType.Web, accountid, From, From.MonthLastDate());
        }

        private JsonResult AccountReport(string sidx, string sord, int page, int rows, ResultType rt, int? accountid, DateTime FromDate, DateTime ToDate) 
        {
                var accounts = Global.geAllUserAccounts(db,UserID, accountid);
                /*if (accountid.HasValue && accountid > 0)
                {
                    accounts = accounts.Where(acc => acc.AccountID == accountid).ToList();
                    if(accounts.Count() < 1) // no access to account
                        return Global.BadRequest(rt);
                }
                else
                {
                    return Global.BadRequest(rt);
                }*/
                var tExpenses = db.Expenses.Join(accounts, ex=>ex.AccountID, acc=>acc.AccountID, (ex,acc)=> ex)
                                .Where(ex => ex.ExpenseDate < FromDate).Select(ex=>ex.Amount).DefaultIfEmpty(0).Sum();
                var tIncomes = db.Income.Join(accounts, inc=>inc.AccountID, acc=>acc.AccountID, (inc,acc)=>inc)
                                .Where(inc => inc.IncomeDate < FromDate).Select(i=>i.Amount).DefaultIfEmpty(0).Sum();
                var trins = db.AccountTransfers.Join(accounts, ti=>ti.TargetAccountID, acc=>acc.AccountID, (ti,acc)=>ti)
                                                .Where(t => t.TransferDate < FromDate).Select(i => i.Amount).DefaultIfEmpty(0).Sum();

                var trouts = db.AccountTransfers.Join(accounts, to=>to.SourceAccountID, acc=>acc.AccountID, (to,acc)=>to)
                                            .Where(t => t.TransferDate < FromDate).Select(i => i.Amount).DefaultIfEmpty(0).Sum();
                var tsavings = db.Savings.Join(accounts, s => s.AccountID, acc => acc.AccountID, (s, acc) => s)
                                            .Where(s => s.SavingDate < FromDate).Select(s => s.Amount).DefaultIfEmpty(0).Sum();

                double initialAmounts = accounts.Select(acc => acc.InitialAmount).DefaultIfEmpty(0).Sum();
                
                double balance = tIncomes + trins + initialAmounts - tExpenses - trouts - tsavings;

                var expenses = db.Expenses.Join(accounts, ex=>ex.AccountID, acc=>acc.AccountID, (ex,acc)=> ex).
                                            Where(ex => 
                                                ex.ExpenseDate >= FromDate && ex.ExpenseDate <= ToDate).AsEnumerable().ToList().Select(
                                                ex => new AccountTransaction
                                                {
                                                    TransactionID = ex.ExpenseID,
                                                    Name = ex.UserID == UserID ? "" : ex.User.UserName,
                                                    TransactionDate = ex.ExpenseDate,
                                                    Description = string.Format("Expense for: {0} - {1}{2}", ex.Subcategory.Category.Name, ex.Subcategory.Name, string.IsNullOrEmpty( ex.Description)? string.Empty : ", " + ex.Description),
                                                    Type = TransactionType.Credit,
                                                    Amount = ex.Amount,
                                                    ObjectType = ObjectType.Expense
                                                }
                                                );
                var incomes = db.Income.Join(accounts, inc=>inc.AccountID, acc=>acc.AccountID, (inc,acc)=>inc).
                                Where(inc => 
                                                inc.IncomeDate >= FromDate && inc.IncomeDate <= ToDate).AsEnumerable().ToList().Select(
                                                inc => new AccountTransaction
                                                {
                                                    TransactionID = inc.IncomeID,
                                                    Name = inc.UserID == UserID ? "" : inc.User.UserName,
                                                    TransactionDate = inc.IncomeDate,
                                                    Description = string.Format("Income from: {0}{1}", inc.IncomeSource.Name, string.IsNullOrEmpty(inc.Description) ? string.Empty : ", " + inc.Description),
                                                    Type = TransactionType.Debit,
                                                    Amount = inc.Amount,
                                                    ObjectType = ObjectType.Income
                                                    
                                                }
                                                );
                var transferIns = db.AccountTransfers.Join(accounts, ti=>ti.TargetAccountID, acc=>acc.AccountID, (ti,acc)=>ti).
                                               Where(t => 
                                               t.TransferDate >= FromDate && t.TransferDate <= ToDate).ToList().Select(
                                               t => new AccountTransaction
                                               {
                                                   TransactionID = t.TransferID,
                                                   Name = t.UserID == UserID ? "" : t.User.UserName,
                                                   TransactionDate = t.TransferDate,
                                                   Description = string.Format("Transfer from: {0}{1}", t.SourceAccount.Name, string.IsNullOrEmpty(t.Description) ? string.Empty : ", " + t.Description),
                                                   Type = TransactionType.Debit,
                                                   Amount = t.Amount,
                                                   ObjectType = ObjectType.Transfer

                                               }
                                               );
                var transferOuts = db.AccountTransfers.Join(accounts, to=>to.SourceAccountID, acc=>acc.AccountID, (to,acc)=>to).
                                                Where(t => 
                                               t.TransferDate >= FromDate && t.TransferDate <= ToDate).ToList().Select(
                                               t => new AccountTransaction
                                               {
                                                   TransactionID = t.TransferID,
                                                   Name = t.UserID == UserID ? "" : t.User.UserName,
                                                   TransactionDate = t.TransferDate,
                                                   Description = string.Format("Transfer to: {0}{1}", t.TargetAccount.Name, string.IsNullOrEmpty(t.Description) ? string.Empty : ", " + t.Description),
                                                   Type = TransactionType.Credit,
                                                   Amount = t.Amount,
                                                   ObjectType = ObjectType.Transfer
                                               }
                                               );
                var savings = db.Savings.Join(accounts, s => s.AccountID, acc => acc.AccountID, (s, acc) => s).
                                                    Where(s =>
                                                   s.SavingDate >= FromDate && s.SavingDate <= ToDate).ToList().Select(
                                                   s => new AccountTransaction
                                                   {
                                                       TransactionID = s.SavingID,
                                                       Name = s.UserID == UserID ? "" : s.User.UserName,
                                                       TransactionDate = s.SavingDate,
                                                       Description = string.Format("Saving for Target: {0}", s.Target.Name),
                                                       Type = TransactionType.Credit,
                                                       Amount = s.Amount,
                                                       ObjectType = ObjectType.Saving
                                                   }
                                                   );

                var transactions = incomes.Union(expenses).Union(transferIns).Union(transferOuts).Union(savings).OrderBy(at => at.TransactionDate).ToList();
                foreach(var item in transactions)
                {
                    item.Balance = balance;
                    if (item.Type == TransactionType.Debit)
                        balance = balance + item.Amount;
                    else
                        balance = balance - item.Amount;
                }
                transactions.Add(new AccountTransaction { // Add a dummy transaction to display the final balance
                    TransactionID = 0,
                    Name = "-",
                    TransactionDate = ToDate,
                    Description = "Final Balance",
                    Type = TransactionType.Debit,
                    Amount = 0,
                    Balance = balance
                });
                return Util.CreateJsonResponse<AccountTransaction>(Global.SortNotRequired, sord, page, rows, transactions,rt, (Func<IEnumerable<AccountTransaction>, Array>)delegate(IEnumerable<AccountTransaction> rd)
                {
                    var result = rd.ToList();
                    if (rt == ResultType.Web)
                    {
                        return (
                        from trans in result
                        select new
                        {
                            TransactionID = trans.TransactionID,
                            cell = new string[] { trans.TransactionID.ToString(), trans.TransactionDate.ToDateString(),
                                trans.Name, trans.Description,  trans.Withdrawl.ToString(), trans.Deposit.ToString(), trans.Balance.ToString()}
                        }).ToArray();
                    }
                    else
                    {
                        return (
                        from trans in result
                        select new
                        {
                            TransactionID = trans.TransactionID,
                            TransactionDate = trans.TransactionDate.ToDateDisplayString(),
                            Description =    trans.Description,
                            UserName = trans.Name,
                            TransactionType = trans.Type.String(),
                            Amount = trans.Amount, 
                            Balance = trans.Balance,
                            ObjectType = trans.ObjectType.String()
                        }).ToArray();
                    }
                }
                   );

        }
	}
}