using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Pocket.Common;
using Pocket.Models;

namespace Pocket.Controllers
{
    public class ExpenseController : Controller
    {
        private QDbContext db = new QDbContext();

        // GET: /Expense/
        public ActionResult Index()
        {
            var expenses = db.Users.Find(State.UserID).Expenses;// db.Expenses.Include(e => e.Payee);
            //(expenses as ObjectQuery).ToTraceString();
            return View(expenses.ToList());
        }

        // GET: /Expense/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Expense expense = db.Expenses.Find(id);
            if (expense == null)
            {
                return HttpNotFound();
            }
            return View(expense);
        }

        // GET: /Expense/Create
        public ActionResult Create()
        {
            ViewBag.PayeeID = new SelectList(db.Users.Find(State.UserID).Payees, "PayeeID", "Name");
            ViewBag.CategoryID = new SelectList(db.Users.Find(State.UserID).Categories, "CategoryID", "Name");
            ViewBag.SubcategoryID = new SelectList(new List<SelectListItem>(), "SubcategoryID", "Name");
            ViewBag.AccountID = new SelectList(db.Users.Find(State.UserID).Accounts, "AccountID", "Name");

            return View();
        }

        // POST: /Expense/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="ExpenseID,AccountID,SubcategoryID,PayeeID,Amount,ExpenseDate")] Expense expense)
        {
            if (ModelState.IsValid)
            {
                expense.UserID = State.UserID;
                db.Expenses.Add(expense);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.PayeeID = new SelectList(db.Payees, "PayeeID", "Name", expense.PayeeID);
            return View(expense);
        }

        // GET: /Expense/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Expense expense = db.Expenses.Find(id);
            if (expense == null)
            {
                return HttpNotFound();
            }
            ViewBag.PayeeID = new SelectList(db.Payees, "PayeeID", "Name", expense.PayeeID);
            return View(expense);
        }

        // POST: /Expense/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="ExpenseID,AccountID,SubcategoryID,PayeeID,Amount,ExpenseDate")] Expense expense)
        {
            if (ModelState.IsValid)
            {
                expense.UserID = State.UserID;
                db.Entry(expense).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.PayeeID = new SelectList(db.Payees, "PayeeID", "Name", expense.PayeeID);
            return View(expense);
        }

        // GET: /Expense/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Expense expense = db.Expenses.Find(id);
            if (expense == null)
            {
                return HttpNotFound();
            }
            return View(expense);
        }

        // POST: /Expense/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Expense expense = db.Expenses.Find(id);
            db.Expenses.Remove(expense);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        
        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        // GET: /Payee/List
        public ActionResult List()
        {
            return View(db.Users.Find(State.UserID).Expenses);
        }

        public JsonResult JList(string sidx, string sord, int page, int rows, bool _search, string searchField, string searchOper, string searchString)
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
                            cell = new string[] { expense.ExpenseID.ToString(), expense.ExpenseDate.ToShortDateString(), expense.Subcategory.Category.Name + " -- " + expense.Subcategory.Name, 
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

        [ValidateAntiForgeryToken]
        [HttpPost]
        public JsonResult JEdit([Bind(Include = "ExpenseID,ExpenseDate,SubcategoryID,AccountID,Amount,Description,PayeeID,VendorID,EventID,Repeat")] Expense expense)
        {
            if (Request.IsAjaxRequest() && ModelState.IsValid)
            {
                expense.UserID = State.UserID;
                if(ValidateExpense(expense))
                {
                    bool add = (expense.ExpenseID == 0);
                    if (add) //add
                    {
                        db.Expenses.Add(expense);
                    }
                    else // edit
                    {
                        Expense exp = db.Expenses.Find(expense.ExpenseID);
                        exp.ExpenseDate = expense.ExpenseDate;
                        exp.SubcategoryID = expense.SubcategoryID;
                        exp.AccountID = expense.AccountID;
                        exp.Amount = expense.Amount;
                        exp.Description = expense.Description;
                        exp.PayeeID = expense.PayeeID;
                        exp.VendorID = expense.VendorID;
                        exp.EventID = expense.EventID;
                        exp.Repeat = expense.Repeat;
                        db.Entry(exp).State = EntityState.Modified;
                    }
                    db.SaveChanges();
                    NotificationController.AddExpenseNotification(db, expense.ExpenseID, add);
                    
                    expense = db.Expenses.Find(expense.ExpenseID);

                    if(expense.Repeat == RepeatPattern.None) // remove any existing scheduler
                    {
                        var schedule = expense.Schedule;
                        if (schedule != null)
                        {
                            db.Schedules.Remove(schedule);
                            expense.ScheduleID = null;
                            db.SaveChanges();
                        }
                            
                    }
                    else
                    {
                        var schedule = expense.Schedule;
                        if (schedule == null)
                        {
                            schedule = new Schedule
                            {
                                UserID = expense.UserID,
                                Type = ScheduleType.Expense,
                                Status = ScheduleStatus.Pending,
                                CreateDate = DateTime.Now,
                                LastRunDate = DateTime.Now,
                                NextRunDate = Util.GetNextRunDate(expense.Repeat, DateTime.Now)
                            };
                            db.Schedules.Add(schedule);
                            db.SaveChanges();
                            expense.ScheduleID = schedule.ScheduleID;
                            db.Entry(expense).State = EntityState.Modified;
                        }
                        else
                        {
                            schedule.LastRunDate = DateTime.Now;
                            schedule.NextRunDate = Util.GetNextRunDate(expense.Repeat, DateTime.Now);
                            db.Entry(schedule).State = EntityState.Modified;
                        }
                        db.SaveChanges();
                    }
                    return Json(new
                    {
                        success = true,
                        message = "success",
                        new_id = expense.ExpenseID
                    });
                }
                
            }
            return Json(new
            {
                success = false,
                message = "Model state is invalid.",
                new_id = 0
            });
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
                            cell = new string[] { expense.ExpenseID.ToString(), expense.ExpenseDate.ToShortDateString(), expense.Subcategory.Category.Name + " -- " + expense.Subcategory.Name, 
                                expense.Account.Name, expense.Amount.ToString(), expense.PayeeID.HasValue ? expense.Payee.Name: "",
                            expense.VendorID.HasValue ?expense.Vendor.Name: "", expense.Comments.Count + " Comments"}
                        }).ToArray();
                }
                    );
            }
            else
                return Json(new HttpStatusCodeResult(HttpStatusCode.BadRequest));

        }
        public JsonResult JExpenseComments(string sidx, string sord, int page, int rows)
        {
            if (Request.IsAjaxRequest())
            {
                int expenseID = Util.ParseInt(Request.Params["ExpenseID"]);

                var expenseComments = db.ExpenseComments.Where(ec => ec.UserID == State.UserID && ec.ExpenseID == expenseID).ToList();

                return Util.CreateJsonResponse<ExpenseComment>(sidx, sord, page, rows, expenseComments, (Func<IEnumerable<ExpenseComment>, Array>)delegate(IEnumerable<ExpenseComment> rd)
                {
                    return (
                        from expensecomment in rd
                        select new
                        {
                            CommentID = expensecomment.CommentID,
                            cell = new string[] { expensecomment.CommentID.ToString(), expensecomment.ExpenseID.ToString(), expensecomment.Comment, expensecomment.User.FirstName + " " + expensecomment.User.LastName, expensecomment.CommentDate.ToShortDateString()}
                        }).ToArray();
                }
                    );
            }
            else
                return Json(new HttpStatusCodeResult(HttpStatusCode.BadRequest));

        }
        [ValidateAntiForgeryToken]
        [HttpPost]
        public JsonResult jcommentedit([Bind(Include = "Comment,ExpenseID")] ExpenseComment exComment)
        {
            if (Request.IsAjaxRequest() && ModelState.IsValid)
            {
                var ei = db.Expenses.Where(e => e.UserID == State.UserID && e.ExpenseID == exComment.ExpenseID).Count() > 0;
                if (ei)
                {
                    ExpenseComment ec = new ExpenseComment();
                    ec.UserID = State.UserID;
                    ec.ExpenseID = exComment.ExpenseID;
                    ec.Comment = exComment.Comment;
                    ec.CommentDate = DateTime.Now;
                    db.ExpenseComments.Add(ec);
                    db.SaveChanges();
                    NotificationController.AddExpenseCommentNotification(exComment.Comment, ec.ExpenseID, db);
                    return Json(new
                    {
                        success = true,
                        message = "success",
                        new_id = ec.CommentID
                    });
                }
                else
                    return Json(new HttpStatusCodeResult(HttpStatusCode.BadRequest));
            }
            return Json(new
            {
                success = false,
                message = "Model state is invalid.",
                new_id = 0
            });
        }
        private bool ValidateExpense(Expense expense)
        {
            User u = db.Users.Find(State.UserID);
            if (u == null)
                return false;
            if (expense.ExpenseID != 0 && u.Expenses.Where(e => e.ExpenseID == expense.ExpenseID).FirstOrDefault() == null)
                return false;
            if (u.Accounts.Find(a => a.AccountID == expense.AccountID) == null)
                return false;
            if (u.Categories.Find(c => c.Subcategories.Find(sc => sc.SubcategoryID == expense.SubcategoryID) != null) == null)
                return false;
            if (expense.EventID.HasValue && u.Events.Find(ev => ev.EventID == expense.EventID.Value) == null)
                return false;
            if (expense.PayeeID.HasValue && u.Payees.Find(ev => ev.PayeeID == expense.PayeeID.Value) == null)
                return false;
            if (expense.VendorID.HasValue && u.Vendors.Find(ev => ev.VendorID == expense.VendorID.Value) == null)
                return false;
            return true;
        }
    }
}
