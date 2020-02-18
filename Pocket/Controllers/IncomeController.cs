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
using System.Data.Entity.Validation;
using Pocket.Extensions;

namespace Pocket.Controllers
{
    [Authorize]
    public class IncomeController : Controller
    {
        private QDbContext db = new QDbContext();

        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        // GET: /Payee/Index
        public ActionResult Index()
        {
            return View(db.Users.Find(State.UserID).Income);
        }
        public JsonResult MRecord(int IncomeID)
        {
            

                var incomes = db.Income.Where(inc => inc.UserID == State.UserID && inc.IncomeID == IncomeID).ToList();
                return Util.Package<JsonResult>(incomes.Select(inc => new
                {
                    IncomeID = inc.IncomeID,
                    IncomeDate = inc.IncomeDate.ToUTCDateString(),
                    AccountID = inc.AccountID,
                    AccountText = inc.Account.Name,
                    Amount = inc.Amount,
                    Description = inc.Description,
                    SourceID = inc.SourceID,
                    SourceText = inc.IncomeSource.Name,
                    Repeat = inc.Repeat.GetHashCode(),
                    RepeatText = inc.Repeat.String()
                }));

        }
        public JsonResult MList(int page, int rows, DateTime? IncomeFrom, DateTime? IncomeTo)
        {
            return (JsonResult)GetList("IncomeDate", "desc", page, rows, IncomeFrom.HasValue?IncomeFrom.Value:DateTime.Today.MonthFirstDate(), IncomeTo.HasValue?IncomeTo.Value:DateTime.Now.MonthLastDate(), ResultType.Mobile);
        }
       public JsonResult JList(string sidx, string sord, int page, int rows)
        {
            DateTime IncomeFrom = Util.ToDateTime(Request.Params["IncomeFrom"], new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1));
            DateTime IncomeTo = Util.ToDateTime(Request.Params["IncomeTo"], new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1));

            return GetList(sidx, sord, page, rows, IncomeFrom, IncomeTo, ResultType.Web);
        }
        private JsonResult GetList(string sidx, string sord, int page, int rows, DateTime IncomeFrom, DateTime IncomeTo, ResultType rt)
        {
                
                var incomes = db.Users.Find(State.UserID).Income.Where(inc => inc.IncomeDate >= IncomeFrom && inc.IncomeDate <= IncomeTo).ToList();

                return Util.CreateJsonResponse<Income>(sidx, sord, page, rows, incomes,rt, (Func<IEnumerable<Income>, Array>)delegate(IEnumerable<Income> rd)
                {
                    if (rt == ResultType.Web)
	{
                         return (
                        from income in rd
                        select new
                        {
                            IncomeID = income.IncomeID,
                            cell = new string[] { income.IncomeID.ToString(), income.IncomeDate.ToDateString(), income.IncomeSource.Name.ToString(), income.Account.Name, income.Amount.ToString(), income.Description, Util.EnumToString<RepeatPattern>(income.Repeat) }
                        }).ToArray();
	}
                   else
	{
                         return (
                        from inc in rd
                        select new { 
                IncomeID = inc.IncomeID,
                IncomeDate = inc.IncomeDate.ToDateDisplayString(),
                AccountID = inc.AccountID,
                AccountText = inc.Account.Name,
                Amount = inc.Amount,
                Description = inc.Description,
                SourceID = inc.SourceID,
                SourceText = inc.IncomeSource.Name,
                Repeat = inc.Repeat.GetHashCode(),
                RepeatText = inc.Repeat.String()
                }).ToArray();
	}
                }
                    );

        }
        [HttpPost]
        public JsonResult MEdit([Bind(Include = "IncomeID,AccountID,SourceID,Amount,IncomeDate,Description,Repeat")] int IncomeID, int AccountID, int SourceID,
                                                                            double Amount, DateTime IncomeDate, string Description, int Repeat)
        {
            return Edit<JsonResult>(IncomeID, AccountID, SourceID, Amount, IncomeDate, Description, Repeat);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public JsonResult JEdit([Bind(Include = "IncomeID,AccountID,SourceID,Amount,IncomeDate,Description,Repeat")] int IncomeID, int AccountID, int SourceID,
                                                                            double Amount, DateTime IncomeDate, string Description, int Repeat)
        {
            return Edit<JsonResult>(IncomeID, AccountID, SourceID, Amount, IncomeDate, Description, Repeat);
        }

        private T Edit<T>([Bind(Include = "IncomeID,AccountID,SourceID,Amount,IncomeDate,Description,Repeat")] int IncomeID, int AccountID, int SourceID,
                                                                            double Amount, DateTime IncomeDate, string Description, int Repeat)where T:JsonResult
        {
            if (ModelState.IsValid)
            {
                bool add = false;
                Income income = null;
                if(IncomeID > 0)
                 income = db.Income.Where(inc => inc.UserID == State.UserID && inc.IncomeID == IncomeID).FirstOrDefault();
                else
                {
                    income = new Income();
                    income.UserID = State.UserID;
                    add = true;
                }
                if (income == null)
                    return Global.BadRequest<T>();

                    income.AccountID = AccountID;
                    income.SourceID = SourceID;
                    income.Amount = Amount;
                    income.IncomeDate = IncomeDate;
                    income.Description = Description;
                    income.Repeat = (RepeatPattern)Repeat;
                    
               
                    
                if (ValidateIncome(income))
                {
                    if (add)
                        db.Income.Add(income);
                    else
                        db.Entry(income).State = EntityState.Modified;
                }
                else
                    return Global.BadRequest<T>();

                if (income.Repeat == RepeatPattern.None) // remove any existing scheduler, if any
                {
                    var schedule = income.Schedule;
                    if (schedule != null)
                    {
                        db.Schedules.Remove(schedule);
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
                            Name = income.Description,
                            Type = ScheduleType.Income,
                            Status = ScheduleStatus.Pending,
                            CreateDate = DateTime.Now,
                            LastRunDate = DateTime.Today,
                            NextRunDate = Util.GetNextRunDate(income.Repeat, DateTime.Today)
                        };
                        income.Schedule = schedule;
                    }
                    else
                    {
                        schedule.LastRunDate = DateTime.Today;
                        schedule.NextRunDate = Util.GetNextRunDate(income.Repeat, DateTime.Today);
                        db.Entry(schedule).State = EntityState.Modified;
                    }
                    
                    
                }
                try
                {
                    db.SaveChanges();
                }
                catch (DbEntityValidationException e)
                {
                    foreach (var eve in e.EntityValidationErrors)
                    {
                        Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                            eve.Entry.Entity.GetType().Name, eve.Entry.State);
                        foreach (var ve in eve.ValidationErrors)
                        {
                            Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                                ve.PropertyName, ve.ErrorMessage);
                        }
                    }
                    throw;
                }
                return Repository.Success<T>(income.IncomeID);
            }
            return Repository.Failure<T>();
        }
        [HttpPost]
        public JsonResult MDelete(int IncomeID)
        {
            return Delete<JsonResult>(IncomeID);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public JsonResult JDelete(int IncomeID)
        {
            return Delete<JsonResult>(IncomeID);
        }

        private T Delete<T>(int IncomeID) where T : JsonResult
        {
                try
                {
                    Income income = db.Income.Where(inc => inc.UserID == State.UserID && inc.IncomeID == IncomeID).FirstOrDefault();
                    if (income != null)
                    {
                        db.Income.Remove(income);
                        db.SaveChanges();
                        return Repository.Success<T>(income.IncomeID);
                    }
                }
                catch (Exception ex)
                {
                    
                }
return Repository.DelFailure<T>();
        }
        private bool ValidateIncome(Income income)
        {
            bool validated = true;
            ApplicationUser u = db.Users.Find(State.UserID);
            if(income.IncomeID != 0)
                validated = validated && u.Income.Where(inc => inc.IncomeID == income.IncomeID).FirstOrDefault() != null;
            validated = validated && Global.geAllUserAccounts(db).Where(acc => acc.AccountID == income.AccountID).FirstOrDefault() != null;
            validated = validated && u.IncomeSources.Where(acc => acc.SourceID == income.SourceID).FirstOrDefault() != null;
            return validated;
        }
    }
}
