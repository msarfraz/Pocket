using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pocket.Common;
using Pocket.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Pocket.Controllers
{
    [Authorize(Roles="Admin")]
    public class ScheduleController : ApplicationController
    {
        public ScheduleController(ApplicationDbContext
            context) : base(context)
        {
            
        }
        // GET: /Schedule/
        public ActionResult Index()
        {
            return View();
        }
        public JsonResult JIndex(string sidx, string sord, int page, int rows)
        {
                var schedules = db.Schedules.Where(sch=>sch.Status != ScheduleStatus.Complete).ToList();
                var evSchedules = db.Events.Where(ev => ev.EventStatus == EventStatus.Active && ev.EventDate <= DateTime.Today).ToList().Select(
                                    ev => new Schedule { ScheduleID = ev.EventID,
                                    Name = ev.Name, LastRunDate = ev.CreatedDate, NextRunDate = ev.EventDate, Status = ScheduleStatus.Pending, CreateDate = ev.CreatedDate,
                                     Type = ScheduleType.Event}).ToList();
                schedules.AddRange(evSchedules);                    
                return Util.CreateJsonResponse<Schedule>(sidx, sord, page, rows, schedules, (Func<IEnumerable<Schedule>, Array>)delegate(IEnumerable<Schedule> rd)
                {
                    return (
                        from schedule in rd
                        select new
                        {
                            ScheduleID = schedule.ScheduleID,
                            cell = new string[] { schedule.ScheduleID.ToString(), schedule.Name.ToString(), schedule.LastRunDate.ToDateString(), schedule.NextRunDate.ToDateString(),
                                                  schedule.Status.String(), schedule.CreateDate.ToDateString(), schedule.Type.String()}
                        }).ToArray();
                }
                    );

        }
        public JsonResult sjobs(string sidx, string sord, int page, int rows)
        {
                var schedules = db.ScheduleJobs.AsEnumerable();

                return Util.CreateJsonResponse<ScheduleJob>(sidx, sord, page, rows, schedules, (Func<IEnumerable<ScheduleJob>, Array>)delegate(IEnumerable<ScheduleJob> rd)
                {
                    return (
                        from schedule in rd
                        select new
                        {
                            ScheduleJobID = schedule.ScheduleJobID,
                            cell = new string[] { schedule.ScheduleJobID.ToString(), schedule.Status.String(), schedule.CreatedDate.ToDateString(), schedule.JobsProcessed.ToString()}
                        }).ToArray();
                }
                    );

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ProcessSchedules()
        {

            return ProcessSchedules(db);

        }

        internal IActionResult ProcessSchedules(ApplicationDbContext db)
        {
            ScheduleJob sjob = db.ScheduleJobs.Where(sj => sj.CreatedDate == DateTime.Today).FirstOrDefault();
            //if (sjob == null)
            //{
                sjob = new ScheduleJob
                {
                    UserID = db.Users.Where(u=>u.UserName=="TestUser2").FirstOrDefault().Id,
                    CreatedDate = DateTime.Today,
                    Status = ScheduleStatus.In_Process
                };
                db.ScheduleJobs.Add(sjob);
                db.SaveChanges();
           // }
            //else
            //return Json(new HttpStatusCodeResult(HttpStatusCode.ExpectationFailed));

            var expenseSchedules = db.Expenses.Where(exp => exp.ScheduleID.HasValue && exp.Schedule.NextRunDate <= DateTime.Today).ToList();
            List<int> processedSchedules = new List<int>();

            foreach (var item in expenseSchedules)
            {
                var expense = new Expense();

                expense.CreatedDate = DateTime.Today;
                expense.ModifiedDate = DateTime.Now;
                    expense.AccountID = item.AccountID;
                    expense.Amount = item.Amount;
                    expense.Description = "#Auto Repeat#" + item.Description;
                    expense.EventID = item.EventID;
                    expense.ExpenseDate = DateTime.Today;
                    expense.PayeeID = item.PayeeID;
                    expense.Repeat = RepeatPattern.None;
                    expense.SubcategoryID = item.SubcategoryID;
                    expense.UserID = item.UserID;
                    expense.VendorID = item.VendorID;
                
                db.Expenses.Add(expense);
                NotificationManager.AddExpenseNotification(db, expense, true, item.UserID);

                processedSchedules.Add(item.ScheduleID.Value);
                //update the scheduler
                item.Schedule.LastRunDate = DateTime.Today;
                item.Schedule.NextRunDate = Util.GetNextRunDate(item.Repeat, item.Schedule.LastRunDate);
                db.Entry(item).State = EntityState.Modified;
            }
        
            var incomeSchedules = db.Income.Where(inc => inc.ScheduleID.HasValue && inc.Schedule.NextRunDate <= DateTime.Today).ToList();
            foreach (var item in incomeSchedules)
            {
                var income = new Income
                {
                    AccountID = item.AccountID,
                    Amount = item.Amount,
                    Description = "#Auto Repeat#" + item.Description,
                    IncomeDate = DateTime.Today,
                    SourceID = item.SourceID,
                    Repeat = RepeatPattern.None,
                    UserID = item.UserID

                };
                db.Income.Add(income);

                //update the scheduler
                item.Schedule.LastRunDate = DateTime.Today;
                item.Schedule.NextRunDate = Util.GetNextRunDate(item.Repeat, item.Schedule.LastRunDate);
                db.Entry(item).State = EntityState.Modified;
            }

            var eventSchedule = db.Events.Where(ev => ev.EventStatus == EventStatus.Active && ev.EventDate <= DateTime.Today).ToList();
            foreach (var item in eventSchedule)
            {
                if (item.Budget.BudgetDuration == RepeatPattern.None)
                {
                    item.EventStatus = EventStatus.Expired;
                    db.Entry(item).State = EntityState.Modified;
                }
                else
                {
                    DateTime nextRun = Util.GetNextRunDate(item.Budget.BudgetDuration, item.EventDate);
                    var nextEvent = new Event
                    {
                        CreatedDate = DateTime.Today,
                        EventDate = nextRun,
                        EventStatus = EventStatus.Active,
                        Name = item.Name,
                        ReminderDate = nextRun,
                        UserID = item.UserID,

                        Budget = new Budget
                        {
                            BudgetAmount = item.Budget.BudgetAmount,
                            BudgetDuration = item.Budget.BudgetDuration,
                            BudgetType = item.Budget.BudgetType, 
                            UserID = item.UserID
                        }
                        
                    };
                    db.Events.Add(nextEvent);
                    
                    //db.Events.Create();

                    foreach (var eu in item.SharedFriends)
                    {
                        //nextEvent.SharedFriends.Add(new EventUser { UserID = eu.UserID, Event = nextEvent });
                        EventUser evu = new EventUser();
                        evu.Event = nextEvent;
                        evu.UserID = eu.UserID;
                        db.EventUsers.Add(evu);
                        NotificationManager.AddEventShareNotification(nextEvent.EventID, nextEvent.Name, eu.UserID, db, item.UserID, item.User.UserName);
                    }
                    /*
                    
                    */
                    // update the expired event
                    item.EventStatus = EventStatus.Expired;
                    db.Entry(item).State = EntityState.Modified;
                }
            }


            sjob.Status = ScheduleStatus.Complete;
            sjob.JobsProcessed = incomeSchedules.Count + expenseSchedules.Count + eventSchedule.Count;
            db.Entry(sjob).State = EntityState.Modified;
            try
            {

            db.SaveChanges();
            }
            catch(Exception ex)
            {
                string str = ex.ToString();
            }
            return Ok();
        }
	}
}