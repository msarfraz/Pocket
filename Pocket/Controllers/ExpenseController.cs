using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Pocket.Common;
using Pocket.Models;
using Pocket.Extensions;

namespace Pocket.Controllers
{
    [Authorize]
    public class ExpenseController : Controller
    {
        private QDbContext db = new QDbContext();

        // GET: /Expense/
        public ActionResult Index()
        {
            //var expenses = db.Users.Find(State.UserID).Expenses;// db.Expenses.Include(e => e.Payee);
            //(expenses as ObjectQuery).ToTraceString();
            return View();
        }

       
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
        public JsonResult MExpenseByID(int ExpenseID)
        {
            var ex = Repository.GetExpenseByID(db, ExpenseID);
            if (ex == null)
            {
                return Global.BadRequest<JsonResult>();
            }
            return Util.Package<JsonResult>(new []{
                new
            {
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
                Editable = ex.UserID == State.UserID,
                Comments = ex.Comments.Count
            }
            });

            
        }
        public JsonResult MList(int? page, int? rows, DateTime? ExpenseFrom, DateTime? ExpenseTo, bool? AllUsers)
        {
            IQueryable<Expense> expenses;
            if (AllUsers.HasValue && AllUsers.Value)
                expenses = Repository.GetAllAccessibleExpenses(db);
            else
                expenses = db.Expenses.Where(exp => exp.UserID == State.UserID);

            if (ExpenseFrom.HasValue)
                expenses = expenses.Where(ex => ex.ExpenseDate >= ExpenseFrom);
            if (ExpenseTo.HasValue)
                expenses = expenses.Where(ex => ex.ExpenseDate <= ExpenseTo);

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
                              SubcategoryText = ex.Subcategory.Name,
                              CategoryText = ex.Subcategory.Category.Name,
                              ExpenseID = ex.ExpenseID,
                              Editable = ex.UserID == State.UserID,
                              UserName = ex.UserID == State.UserID ? "" : ex.User.UserName
                          })
                      };
            int totalRecords = gexp.Count();
            int pageIndex = (page.HasValue ? page.Value : 1) - 1;
            int pageSize = rows.HasValue ? rows.Value : totalRecords;
            int totalPages = (int)Math.Ceiling((float)totalRecords / (float)pageSize);

            var result = gexp.Skip(pageIndex * pageSize).Take(pageSize).ToList();
            return Util.Package<JsonResult>(result, totalRecords, totalPages, pageIndex);
        }
        public JsonResult JList(string sidx, string sord, int page, int rows)
        {
            if (Request.IsAjaxRequest())
            {
                DateTime expenseFrom = Util.ToDateTime(Request.Params["ExpenseFrom"], new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1));
                DateTime expenseTo = Util.ToDateTime(Request.Params["ExpenseTo"], new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1));
                
                var expenses = db.Users.Find(State.UserID).Expenses.Where(ex=> ex.ExpenseDate >= expenseFrom && ex.ExpenseDate <= expenseTo).ToList();

                return Util.CreateJsonResponse<Expense>(sidx, sord, page, rows, expenses, (Func<IEnumerable<Expense>, Array>)delegate(IEnumerable<Expense> rd)
                {
                    return (
                        from expense in rd
                        select new
                        {
                            ExpenseID = expense.ExpenseID,
                            cell = new string[] { expense.ExpenseID.ToString(), expense.ExpenseDate.ToDateString(), expense.Subcategory.SubcategoryID.ToString(), 
                                expense.Account.Name, expense.Amount.ToString(), expense.Description, expense.PayeeID.HasValue ? expense.Payee.Name: "",
                            expense.VendorID.HasValue ?expense.Vendor.Name: "", expense.EventID.HasValue ? expense.Event.Name : "",
                            expense.Comments.Count + " comments", Util.EnumToString<RepeatPattern>(expense.Repeat)}
                        }).ToArray();
                }
                    );
            }
            else
                return Json(new HttpStatusCodeResult(HttpStatusCode.BadRequest));

        }
        [HttpPost]
        public JsonResult MEdit([Bind(Include = "ExpenseID,ExpenseDate,SubcategoryID,AccountID,Amount,Description,PayeeID,VendorID,EventID,Repeat")] 
            int ExpenseID, DateTime ExpenseDate, int SubcategoryID, int AccountID, double Amount, string Description, int? PayeeID, int? VendorID,
            int? EventID, int Repeat)
        {
            return Edit<JsonResult>(ExpenseID, ExpenseDate, SubcategoryID, AccountID, Amount, Description, PayeeID, VendorID, EventID, Repeat);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public JsonResult JEdit([Bind(Include = "ExpenseID,ExpenseDate,SubcategoryID,AccountID,Amount,Description,PayeeID,VendorID,EventID,Repeat")] 
            int ExpenseID, DateTime ExpenseDate, int SubcategoryID, int AccountID, double Amount, string Description, int? PayeeID, int? VendorID,
            int? EventID, int Repeat)
        {
            return Edit<JsonResult>(ExpenseID, ExpenseDate, SubcategoryID, AccountID, Amount, Description, PayeeID, VendorID, EventID, Repeat);
        }

        private T Edit<T>(int ExpenseID, DateTime ExpenseDate, int SubcategoryID, int AccountID, double Amount, string Description, int? PayeeID, int? VendorID,
            int? EventID, int Repeat) where T:JsonResult
        {
            if ( ModelState.IsValid)
            {
                bool add = false;
                Expense exp = null;
                if(ExpenseID > 0)
                {
                    exp = db.Expenses.Where(ex => ex.ExpenseID == ExpenseID && ex.UserID == State.UserID).FirstOrDefault();
                    if (exp == null)
                        return Global.BadRequest<T>();
                    if (exp.EventID.HasValue && exp.EventID != EventID)
                    {
                        NotificationManager.AddExpenseDelNotification(db, exp);
                    }
                }
                else
                {
                    add = true;
                    exp = new Expense();
                    exp.UserID = State.UserID;
                    exp.CreatedDate = DateTime.Now;
                }
                
                exp.ExpenseDate = ExpenseDate;
                exp.SubcategoryID = SubcategoryID;
                exp.AccountID = AccountID;
                exp.Amount = Amount;
                exp.Description = Description;
                exp.PayeeID = PayeeID;
                exp.VendorID = VendorID;
                exp.EventID = EventID;
                exp.Repeat = (RepeatPattern) Repeat;
                exp.ModifiedDate = DateTime.Now;

                if(ValidateExpense(exp))
                {
                    if (add) //add
                    {
                        db.Expenses.Add(exp);
                    }
                    else // edit
                    {
                        db.Entry(exp).State = EntityState.Modified;
                    }

                    
                    
                    if(exp.Repeat == RepeatPattern.None) // remove any existing scheduler
                    {
                        var schedule = exp.Schedule;
                        if (schedule != null)
                        {
                            db.Schedules.Remove(schedule);
                        }
                    }
                    else
                    {
                        var schedule = exp.Schedule;
                        if (schedule == null)
                        {
                            schedule = new Schedule
                            {
                                UserID = exp.UserID,
                                Name = exp.Description,
                                Type = ScheduleType.Expense,
                                Status = ScheduleStatus.Pending,
                                CreateDate = DateTime.Now,
                                LastRunDate = DateTime.Today,
                                NextRunDate = Util.GetNextRunDate(exp.Repeat, DateTime.Today)
                            };
                            exp.Schedule = schedule;
                        }
                        else
                        {
                            schedule.LastRunDate = DateTime.Today;
                            schedule.NextRunDate = Util.GetNextRunDate(exp.Repeat, schedule.LastRunDate);
                            db.Entry(schedule).State = EntityState.Modified;
                        }
                    }
                        db.SaveChanges();
                        NotificationManager.AddExpenseNotification(db, exp, add, State.UserID);
                        db.SaveChanges();
                        return Repository.Success<T>(exp.ExpenseID);
                   
                }
                
            }
            return Repository.Failure<T>("Model state is invalid.");
        }
        [HttpPost]
        public JsonResult MDelete(int ExpenseID)
        {
            return Delete<JsonResult>(ExpenseID);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public JsonResult JDelete(int ExpenseID)
        {
            return Delete<JsonResult>(ExpenseID);
        }

        private T Delete<T>(int ExpenseID) where T : JsonResult
        {
            
                try
                {
                    Expense expense = db.Expenses.Where(ex => ex.UserID == State.UserID && ex.ExpenseID == ExpenseID).FirstOrDefault();
                    if (expense != null)
                    {
                        db.Expenses.Remove(expense);
                        db.SaveChanges();
                        return Repository.Success<T>(expense.ExpenseID);
                    }
                }
                catch (Exception ex)
                {
                    
                }
return Repository.DelFailure<T>();
            
        }
        public ActionResult Event(int EventID)
        {

            return View();
        }
        public JsonResult JEventExpenses(string sidx, string sord, int page, int rows)
        {
            if (Request.IsAjaxRequest())
            {
                int eventID = Util.ParseInt(Request.Params["EventID"]);

                var expenses = db.Expenses.Where(ex => ex.UserID == State.UserID && ex.EventID == eventID).ToList();

                return Util.CreateJsonResponse<Expense>(sidx, sord, page, rows, expenses, (Func<IEnumerable<Expense>, Array>)delegate(IEnumerable<Expense> rd)
                {
                    return (
                        from expense in rd
                        select new
                        {
                            ExpenseID = expense.ExpenseID,
                            cell = new string[] { expense.ExpenseID.ToString(), expense.ExpenseDate.ToDateString(), expense.Subcategory.Category.Name + " -- " + expense.Subcategory.Name, 
                                expense.Account.Name, expense.Amount.ToString(), expense.PayeeID.HasValue ? expense.Payee.Name: "",
                            expense.VendorID.HasValue ?expense.Vendor.Name: "", expense.Comments.Count + " Comments"}
                        }).ToArray();
                }
                    );
            }
            else
                return Json(new HttpStatusCodeResult(HttpStatusCode.BadRequest));

        }
        public JsonResult MExpenseComments(int expenseID)
        {
            
                string userid = State.UserID;

                var ei = db.Expenses.Where(e => e.ExpenseID == expenseID).SingleOrDefault();
                if (ei != null && Util.HasExpenseAccess(ei, State.UserID))
                {
                    var expenseComments = db.ExpenseComments.Where(ec => ec.ExpenseID == expenseID).ToList();
                    var comments = from ecomment in expenseComments
                                   group ecomment by ecomment.CommentDate.Date into g
                                   orderby g.Key
                                   select new { CDate = g.Key.ToDateDisplayString(), Comments = g.Select(ec=>new{
                                                   CommentID = ec.CommentID,
                                                ExpenseID = ec.ExpenseID.ToString(),
                                                Comment = ec.Comment,
                                                UserName = ec.User.UserName,
                                                CommentDate = ec.CommentDate.ToDateDisplayString(),
                                                IsSent = ec.UserID == userid
                                   } )};
                    return Util.Package<JsonResult>(comments);
                }
                return Global.BadRequest<JsonResult>();
        }
        public JsonResult JExpenseComments(string sidx, string sord, int page, int rows)
        {
            int expenseID = Util.ParseInt(Request.Params["ExpenseID"]);
            return ExpenseComments(sidx, sord, page, rows, expenseID);
        }
        private JsonResult ExpenseComments(string sidx, string sord, int page, int rows, int expenseID )
        {
            if (Request.IsAjaxRequest())
            {
                string userid = State.UserID;
                
                var ei = db.Expenses.Where(e => e.ExpenseID == expenseID).SingleOrDefault();
                if (ei != null && Util.HasExpenseAccess(ei, State.UserID))
                {

                    var expenseComments = db.ExpenseComments.Where(ec => ec.ExpenseID == expenseID).ToList();

                    return Util.CreateJsonResponse<ExpenseComment>(sidx, sord, page, rows, expenseComments,  (Func<IEnumerable<ExpenseComment>, Array>)delegate(IEnumerable<ExpenseComment> rd)
                    {
                        return (
                            from expensecomment in rd
                            select new
                            {
                                CommentID = expensecomment.CommentID,
                                cell = new string[] { expensecomment.CommentID.ToString(), expensecomment.ExpenseID.ToString(), expensecomment.Comment, expensecomment.User.UserName, expensecomment.CommentDate.ToDateString() }
                            }).ToArray();
                    }
                        );
                }
            }
            return Repository.Failure<JsonResult>("Bad Request");

        }
        public JsonResult Mcommentedit(string Comment, int ExpenseID)
        {
            return EditComment<JsonResult>(Comment, ExpenseID);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public JsonResult jcommentedit([Bind(Include = "Comment,ExpenseID")] string Comment, int ExpenseID)
        {
            return EditComment<JsonResult>(Comment, ExpenseID);
        }

        private T EditComment<T>(string Comment, int ExpenseID) where T:JsonResult
        {
            if (ModelState.IsValid)
            {
                ApplicationUser u = db.Users.Find(State.UserID);

                var ei = db.Expenses.Where(e => e.ExpenseID == ExpenseID).SingleOrDefault();
                if (ei != null && Util.HasExpenseAccess(ei, State.UserID))
                {
                    ExpenseComment ec = new ExpenseComment();
                    ec.UserID = State.UserID;
                    ec.ExpenseID = ExpenseID;
                    ec.Comment = Comment;
                    ec.CommentDate = DateTime.Now;
                    db.ExpenseComments.Add(ec);
                    db.SaveChanges();
                    NotificationManager.AddExpenseCommentNotification(Comment, ec.ExpenseID, db);
                    return Repository.Success<T>(ec.CommentID);
                }
                else
                    return Repository.Failure<T>();
            }
            return Repository.Failure<T>();
        }
        private bool ValidateExpense(Expense expense)
        {
            ApplicationUser u = db.Users.Find(State.UserID);
            if (u == null)
                return false;
            if (expense.ExpenseID != 0 && u.Expenses.Where(e => e.ExpenseID == expense.ExpenseID).SingleOrDefault() == null)
                return false;
            if (Common.Global.geAllUserAccounts(db).SingleOrDefault(a => a.AccountID == expense.AccountID) == null)

                return false;
            if (u.Categories.Find(c => c.Subcategories.Find(sc => sc.SubcategoryID == expense.SubcategoryID) != null) == null)
            {
                var fcats = db.CategoryUsers.Where(cu => cu.UserID == State.UserID).Select(cu => cu.Category);
                if (fcats.Where(c => c.Subcategories.Where(sc => sc.SubcategoryID == expense.SubcategoryID).FirstOrDefault() != null).FirstOrDefault() == null)
                return false;
            }
            if (expense.EventID.HasValue && u.Events.Find(ev => ev.EventID == expense.EventID.Value) == null)
            {
                var fevents = db.EventUsers.Where(eu => eu.UserID == State.UserID).Select(eu => eu.Event).ToList();
                if(fevents.Where(fe=>fe.EventID == expense.EventID).FirstOrDefault() == null)
                return false;
            }
            if (expense.PayeeID.HasValue && u.Payees.Find(ev => ev.PayeeID == expense.PayeeID.Value) == null)
                return false;
            if (expense.VendorID.HasValue && u.Vendors.Find(ev => ev.VendorID == expense.VendorID.Value) == null)
                return false;
            return true;
        }
    }
}
