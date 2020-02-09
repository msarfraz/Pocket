using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Pocket.Common;
using Pocket.Models;

namespace Pocket.Controllers
{
    public class IncomeController : Controller
    {
        private QDbContext db = new QDbContext();

        // GET: /Income/
        public ActionResult Index()
        {
            return View(db.Income.ToList());
        }

        // GET: /Income/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Income income = db.Income.Find(id);
            if (income == null)
            {
                return HttpNotFound();
            }
            return View(income);
        }

        // GET: /Income/Create
        public ActionResult Create()
        {
            ViewBag.AccountID = new SelectList(db.Users.Find(State.UserID).Accounts, "AccountID", "Name");
            ViewBag.SourceID = new SelectList(db.Users.Find(State.UserID).IncomeSources, "SourceID", "Name");
            return View();
        }

        // POST: /Income/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="IncomeID,AccountID,SourceID,Amount,IncomeDate")] Income income)
        {
            if (ModelState.IsValid)
            {
                income.UserID = State.UserID;
                db.Income.Add(income);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(income);
        }

        // GET: /Income/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Income income = db.Income.Find(id);
            if (income == null)
            {
                return HttpNotFound();
            }
            return View(income);
        }

        // POST: /Income/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="IncomeID,UserID,AccountID,SourceID,Amount,IncomeDate")] Income income)
        {
            if (ModelState.IsValid)
            {
                db.Entry(income).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(income);
        }

        // GET: /Income/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Income income = db.Income.Find(id);
            if (income == null)
            {
                return HttpNotFound();
            }
            return View(income);
        }

        // POST: /Income/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Income income = db.Income.Find(id);
            db.Income.Remove(income);
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
            return View(db.Users.Find(State.UserID).Income);
        }
        // GET: /Payee/
        public JsonResult JList(string sidx, string sord, int page, int rows)
        {
            if (Request.IsAjaxRequest())
            {
                DateTime IncomeFrom = Util.ToDateTime(Request.Params["IncomeFrom"], new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1));
                DateTime IncomeTo = Util.ToDateTime(Request.Params["IncomeTo"], new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1));

                var incomes = db.Users.Find(State.UserID).Income.Where(inc => inc.IncomeDate >= IncomeFrom && inc.IncomeDate <= IncomeTo).ToList();

                return Util.CreateJsonResponse<Income>(sidx, sord, page, rows, incomes, (Func<IEnumerable<Income>, Array>)delegate(IEnumerable<Income> rd)
                {
                    return (
                        from income in rd
                        select new
                        {
                            IncomeID = income.IncomeID,
                            cell = new string[] { income.IncomeID.ToString(), income.IncomeDate.ToShortDateString(), income.IncomeSource.Name.ToString(), income.Account.Name, income.Amount.ToString(), income.Description, Util.EnumToString<RepeatPattern>(income.Repeat) }
                        }).ToArray();
                }
                    );
            }
            else
                return Json(new HttpStatusCodeResult(HttpStatusCode.BadRequest));

        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public JsonResult JEdit([Bind(Include = "IncomeID,AccountID,SourceID,Amount,IncomeDate,Description,Repeat")] Income income)
        {
            if (Request.IsAjaxRequest() && ModelState.IsValid)
            {
                if(!ValidateIncome(income))
                    return Json(HttpStatusCode.BadRequest);
                income.UserID = State.UserID;
                if (income.IncomeID == 0) //add
                {
                    db.Income.Add(income);
                }
                else
                {
                    Income inc = db.Income.Find(income.IncomeID);
                    inc.IncomeID = income.IncomeID;
                    inc.AccountID = income.AccountID;
                    inc.SourceID = income.SourceID;
                    inc.Amount = income.Amount;
                    inc.IncomeDate = income.IncomeDate;
                    inc.Description = income.Description;
                    inc.Repeat = income.Repeat;
                    db.Entry(inc).State = EntityState.Modified;
                }
                db.SaveChanges();
                
                income = db.Income.Find(income.IncomeID);

                if (income.Repeat == RepeatPattern.None) // remove any existing scheduler, if any
                {
                    var schedule = income.Schedule;
                    if (schedule != null)
                    {
                        db.Schedules.Remove(schedule);
                        income.ScheduleID = null;
                        db.SaveChanges();
                    }

                }
                else
                {
                    var schedule = income.Schedule;
                    if (schedule == null)
                    {
                        schedule = new Schedule
                        {
                            UserID = income.UserID,
                            Type = ScheduleType.Income,
                            Status = ScheduleStatus.Pending,
                            CreateDate = DateTime.Now,
                            LastRunDate = DateTime.Now,
                            NextRunDate = Util.GetNextRunDate(income.Repeat, DateTime.Now)
                        };
                        db.Schedules.Add(schedule);
                        db.SaveChanges();
                        income.ScheduleID = schedule.ScheduleID;
                        db.Entry(income).State = EntityState.Modified;
                    }
                    else
                    {
                        schedule.LastRunDate = DateTime.Now;
                        schedule.NextRunDate = Util.GetNextRunDate(income.Repeat, DateTime.Now);
                        db.Entry(schedule).State = EntityState.Modified;
                    }
                    db.SaveChanges();
                }
                return Json(new
                {
                    success = true,
                    message = "success",
                    new_id = income.IncomeID
                });
            }
            return Json(new
            {
                success = false,
                message = "Model state is invalid.",
                new_id = 0
            });
        }

        private bool ValidateIncome(Income income)
        {
            bool validated = true;
            User u = db.Users.Find(State.UserID);
            if(income.IncomeID != 0)
                validated = validated && u.Income.Where(inc => inc.IncomeID == income.IncomeID).FirstOrDefault() != null;
            validated = validated && u.Accounts.Where(acc => acc.AccountID == income.AccountID).FirstOrDefault() != null;
            validated = validated && u.IncomeSources.Where(acc => acc.SourceID == income.SourceID).FirstOrDefault() != null;
            return validated;
        }
    }
}
