using Pocket.Common;
using Pocket.Models;
using Pocket.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;


namespace Pocket.Controllers
{
    public class ReportController : Controller
    {
        private QDbContext db = new QDbContext();

        // GET: /Report/
        public ActionResult Index()
        {
            return View();
        }

        
        // GET: /Report/AdvanceFilter
        public ActionResult AdvanceFilter()
        {
            User u = db.Users.Find(State.UserID);
            var fevents = db.EventUsers.Where(eu => eu.UserID == State.UserID).Select(eu => eu.Event).ToList();
            ReportData model = new ReportData
            {
                Accounts = u.Accounts,
                Categories = u.Categories,
                Payees = u.Payees,
                Vendors = u.Vendors,
                MyEvents = u.Events,
                SharedEvents = fevents
            };
            return View(model);
        }

        // POST: /Report/AdvanceFilter/
        public JsonResult JAdvanceFilter(string sidx, string sord, int page, int rows)
        {
             if (Request.IsAjaxRequest())
            {
                 int? AccountID = Util.ToInt(Request.Params["AccountID"]);
                 int? SubcatID = Util.ToInt(Request.Params["SubcatID"]); 
                 int? PayeeID = Util.ToInt(Request.Params["PayeeID"]); 
                 int? VendorID = Util.ToInt(Request.Params["VendorID"]);
                 int? EventID = Util.ToInt(Request.Params["EventID"]);
                 DateTime? FromDate = Util.ToDateTime(Request.Params["ExpenseFrom"]);
                 DateTime? ToDate = Util.ToDateTime(Request.Params["ExpenseTo"]);

            var expenses = db.Expenses.Where(exp => exp.UserID == State.UserID);
            if (AccountID.HasValue)
                expenses = expenses.Where(exp => exp.AccountID == AccountID);
            if (SubcatID.HasValue)
                expenses = expenses.Where(exp => exp.SubcategoryID == SubcatID);
            if (PayeeID.HasValue)
                expenses = expenses.Where(exp => exp.PayeeID == PayeeID);
            if (VendorID.HasValue)
                expenses = expenses.Where(exp => exp.VendorID == VendorID);
            if (EventID.HasValue)
                expenses = expenses.Where(exp => exp.EventID == EventID);
            if (FromDate.HasValue)
                expenses = expenses.Where(exp => exp.ExpenseDate >= FromDate);
            if (ToDate.HasValue)
                expenses = expenses.Where(exp => exp.ExpenseDate <= ToDate);

            return Util.CreateJsonResponse<Expense>(sidx, sord, page, rows, expenses, (Func<IEnumerable<Expense>, Array>)delegate(IEnumerable<Expense> rd)
            {
                return (
                    from expense in rd
                    select new
                    {
                        ExpenseID = expense.ExpenseID,
                        cell = new string[] { expense.ExpenseID.ToString(), expense.ExpenseDate.ToShortDateString(), expense.Subcategory.Category.Name + " -- " + expense.Subcategory.Name, expense.Account.Name, expense.Amount.ToString(), 
                                expense.PayeeID.HasValue ? expense.Payee.Name : "",
                            expense.VendorID.HasValue ? expense.Vendor.Name : "",
                            expense.EventID.HasValue ? expense.Event.Name : "",Util.EnumToString<RepeatPattern>(expense.Repeat), expense.Comments.Count + " comments" }
                    }).ToArray();
            }
               );
            }
             else
                 return Json(new HttpStatusCodeResult(HttpStatusCode.BadRequest));
        }

        // GET: /Report/Category
        public ActionResult GroupReport()
        {
            return View(db.Users.Find(State.UserID).Expenses);
        }
        // GET: /JCategory/
        public JsonResult JGroupReport(string sidx, string sord, int page, int rows)
        {
            if (Request.IsAjaxRequest())
            {
                int month = Util.ParseInt(Request.Params["Month"]);
                int year = Util.ParseInt(Request.Params["Year"]);

                var expenses = db.Users.Find(State.UserID).Expenses.Where(ex=> ex.ExpenseDate.Month == month && ex.ExpenseDate.Year == year);

                return Util.CreateJsonResponse<Expense>(sidx, sord, page, rows, expenses, (Func<IEnumerable<Expense>, Array>)delegate(IEnumerable<Expense> rd)
                {
                    return (
                        from expense in rd
                        select new
                        {
                            ExpenseID = expense.ExpenseID,
                            cell = new string[] { expense.ExpenseID.ToString(),string.Empty, expense.ExpenseDate.ToShortDateString(), expense.Subcategory.Category.Name + " -- " + expense.Subcategory.Name, expense.Account.Name, expense.Amount.ToString(), 
                                expense.PayeeID.HasValue ? expense.Payee.Name : "",
                            expense.VendorID.HasValue ? expense.Vendor.Name : "",
                            expense.EventID.HasValue ? expense.Event.Name : ""}
                        }).ToArray();
                }
                    );
            }
            else
                return Json(new HttpStatusCodeResult(HttpStatusCode.BadRequest));

        }

        // GET: /Report/CategoryReport
        public ActionResult CategoryReport()
        {
            return View();
        }
        // GET: /JCategory/
        
        public JsonResult JCategoryReport(string sidx, string sord, int page, int rows)
        {
            if (Request.IsAjaxRequest())
            {
                int month = Util.ParseInt(Request.Params["Month"]);
                int year = Util.ParseInt(Request.Params["Year"]);
                
                var expenses = db.Users.Find(State.UserID).Expenses.Where(ex => ex.ExpenseDate.Month == month && ex.ExpenseDate.Year == year);
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
                                                    expense.Subcategory.Category.Name, 
                                                    string.Empty, 
                                                    string.Empty, 
                                                    string.Empty,
                                                    string.Empty,
                                                    string.Empty,
                                                    string.Empty,
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
                                                    expense.ExpenseDate.ToShortDateString(), 
                                                    expense.Account.Name, 
                                                    expense.Amount.ToString(), 
                                                    string.Empty,
                                                    expense.PayeeID.HasValue ? expense.Payee.Name : "",
                                                    expense.VendorID.HasValue ? expense.Vendor.Name : "",
                                                    expense.EventID.HasValue ? expense.Event.Name : "",
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
            else
                return Json(new HttpStatusCodeResult(HttpStatusCode.BadRequest));

        }
        // Get: Report/Event/id
        public ActionResult EventReport(int? id, int? ExpenseID)
        {
            var myevents = db.Users.Find(State.UserID).Events;
            var fevents = db.EventUsers.Where(eu => eu.UserID == State.UserID).Select(eu => eu.Event).ToList();

            Tuple<List<Event>, List<Event>,int?, int?> model = new Tuple<List<Event>, List<Event>,int?,int?>(myevents, fevents, id, ExpenseID);
            
            return View(model);
        }
        private List<Event> GetAllEvents()
        {
            var myevents = db.Users.Find(State.UserID).Events;
            var fevents = db.EventUsers.Where(eu => eu.UserID == State.UserID).Select(eu => eu.Event);
            myevents.AddRange(fevents);
            return myevents;
        }
        // GET: /JCategory/
        public JsonResult JEventReport(string sidx, string sord, int page, int rows)
        {
            if (Request.IsAjaxRequest())
            {
                int eventid = Util.ParseInt(Request.Params["EventID"]);
                if (GetAllEvents().Find(ev=> ev.EventID == eventid) == null)
                {
                    eventid = 0;
                }
                var expenses = db.Users.Find(State.UserID).Expenses.Where(ex => ex.EventID == eventid);

                return Util.CreateJsonResponse<Expense>(sidx, sord, page, rows, expenses, (Func<IEnumerable<Expense>, Array>)delegate(IEnumerable<Expense> rd)
                {
                    return (
                        from expense in rd
                        select new
                        {
                            ExpenseID = expense.ExpenseID,
                            cell = new string[] { expense.ExpenseID.ToString(), expense.ExpenseDate.ToShortDateString(), expense.Subcategory.Category.Name + " -- " + expense.Subcategory.Name, expense.Account.Name, expense.Amount.ToString(), 
                                expense.PayeeID.HasValue ? expense.Payee.Name : "",
                            expense.VendorID.HasValue ? expense.Vendor.Name : "", expense.Comments.Count + " comments"}
                        }).ToArray();
                }
                    );
            }
            else
                return Json(new HttpStatusCodeResult(HttpStatusCode.BadRequest));

        }

	}
}