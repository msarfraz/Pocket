using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Pocket.Models;
using Pocket.Common;
using Pocket.ViewModels;
using Pocket.Extensions;

namespace Pocket.Controllers
{
    [Authorize]
    public class TargetController : Controller
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

        // GET: /target/Index
        public ActionResult Index()
        {
            return View();
        }
        public JsonResult MRecord(int targetid)
        {
            var targets = ToTargetResult( db.Targets.Where(t => t.UserID == State.UserID && t.TargetID == targetid).ToList());
            return Util.Package<JsonResult>(targets.Select(
                target=>new{
                            TargetID = target.TargetID,
                            Name = target.Name,
                            TargetAmount = target.TargetAmount,
                            InitialAmount = target.InitialAmount,
                            Status = target.Status.GetHashCode(),
                            StatusText = target.Status.String(),
                            TargetDate = target.TargetDate.ToUTCDateString(),
                            TargetDateText = target.TargetDate.ToDateDisplayString(),

                            Budgeted = target.Budgeted,
                            BudgetedText = target.Budgeted.String(),
                            BudgetDuration = target.BudgetDuration,
                            BudgetDurationText = target.BudgetDuration.String(),
                            BudgetAmount = target.BudgetAmount,

                            SavingAmount = target.TotalSaving,

                            CurrentAmount = target.CurrentSaving,
                            RequiredAmount = target.RequiredSaving,
                            ExpectedDateText = target.ExpectedDate.ToDateDisplayString(),
                            CreatedDateText = target.CreatedDate.ToDateDisplayString(),
                            Outlook = target.CurrentSaving >= target.RequiredSaving
            }));
        }
        public JsonResult MList(int page, int rows)
        {
            var targets = db.Targets.Where(t => t.UserID == State.UserID);
            return (JsonResult) Util.CreateJsonResponse<Target>("TargetDate", "desc", page, rows, targets, ResultType.Mobile, (Func<IEnumerable<Target>, Array>)delegate(IEnumerable<Target> rd)
                {
                    var tars = ToTargetResult(rd.ToList());
                    return (from target in tars
                            select new
                            {
                                TargetID = target.TargetID,
                                Name = target.Name,
                                TargetAmount = target.TargetAmount,
                                StatusText = target.Status.String(),
                                TargetDateText = target.TargetDate.ToDateDisplayString(),
                                ExpectedDateText = target.ExpectedDate.ToDateDisplayString(),
                                Outlook = target.CurrentSaving >= target.RequiredSaving
                            }).ToArray();
                });
        }
        public JsonResult MUpcomingList()
        {
            var targets = db.Targets.Where(t => t.UserID == State.UserID && t.Status == Common.TargetStatus.Active && t.TargetDate >= DateTime.Today);
            return (JsonResult)Util.CreateJsonResponse<Target>("TargetDate", "desc", 1, 5, targets, ResultType.Mobile, (Func<IEnumerable<Target>, Array>)delegate(IEnumerable<Target> rd)
            {
                var tars = ToTargetResult(rd.ToList());
                return (from target in tars
                        select new
                        {
                            TargetID = target.TargetID,
                            Name = target.Name,
                            TargetAmount = target.TargetAmount,
                            StatusText = target.Status.String(),
                            TargetDateText = target.TargetDate.ToDateDisplayString(),
                            ExpectedDateText = target.ExpectedDate.ToDateDisplayString(),
                            Savings = target.TotalSaving + target.InitialAmount,
                            Outlook = target.CurrentSaving >= target.RequiredSaving

                        }).ToArray();
            });
        }
        public JsonResult JList(string sidx, string sord, int page, int rows)
        {
            return GetList(sidx, sord, page, rows, ResultType.Web);
        }
        private JsonResult GetList(string sidx, string sord, int page, int rows, ResultType rt)
        {
                string userid = State.UserID;
                var targets = ToTargetResult(db.Targets.Where(t => t.UserID == State.UserID).ToList());
               
                return Util.CreateJsonResponse<TargetResult>(sidx, sord, page, rows, targets,rt, (Func<IEnumerable<TargetResult>, Array>)delegate(IEnumerable<TargetResult> rd)
                {
                   
                        if (rt == ResultType.Web)
	{
                        return (     from target in rd
                        select new
                        {
                            TargetID = target.TargetID,
                            cell = new string[] { target.TargetID.ToString(), target.Name, target.TargetAmount.ToString(), target.Status.String(), 
                                target.TargetDate.ToDateString(), target.InitialAmount.ToString(), target.BudgetDuration.String(), target.BudgetAmount.ToString(),target.Budgeted.String(),
                           Math.Round( target.CurrentSaving,2).ToString(), Math.Round( target.RequiredSaving,2).ToString(),
                               target.ExpectedDate == DateTime.MinValue ? "" : target.ExpectedDate.ToDateString(), target.TargetPercentage.ToString(),target.TotalSaving.ToString()
                            }
                        }).ToArray();
	}
                    else
                        {
                            return (from target in rd
                        select new
                        {
                            TargetID = target.TargetID,
                            Name = target.Name,
                            TargetAmount=target.TargetAmount,
                            Status=target.Status.GetHashCode(),
                            StatusText=target.Status.String(),
                            TargetDate=target.TargetDate.ToUTCDateString(),
                            CurrentSaving=Math.Round( target.CurrentSaving,2).ToString(),
                            RequiredSaving=Math.Round( target.RequiredSaving,2).ToString(),
                            ExpectedDate= target.ExpectedDate == DateTime.MinValue ? "" : target.ExpectedDate.ToUTCDateString(),
                            TargetPercentage= target.TargetPercentage.ToString()
                            
                        }).ToArray();
                        }
                        
                }
                    );

        }

        private List<TargetResult> ToTargetResult(List<Target> Targets)
        {
            var targets = Targets.OrderBy(t => t.TargetDate).Select(t => new TargetResult
            {
                Name = t.Name,
                ExpectedDate = t.ExpectedDate,
                CreatedDate = t.CreatedDate,
                Status = t.Status,
                TargetAmount = t.TargetAmount,
                InitialAmount = t.InitialAmount,
                TargetDate = t.TargetDate,
                BudgetDuration = t.Budget.BudgetDuration,
                BudgetAmount = t.Budget.BudgetAmount,
                Budgeted = t.Budgeted,
                TargetID = t.TargetID,
                TotalSaving = t.Savings.Select(s => s.Amount).DefaultIfEmpty(0).Sum()
            }).ToList();

            foreach (var t in targets)
            {
                if (t.Status == Common.TargetStatus.Active)
                {
                    if (t.TargetAmount == 0)
                        t.TargetPercentage = 100;
                    else
                        t.TargetPercentage = Math.Round((((t.TotalSaving + t.InitialAmount) / t.TargetAmount) * 100), 2);

                    var rs = Math.Max(0, t.TargetDate.Subtract(t.CreatedDate).TotalDays);// avoid minus value
                    var rSavingPattern = t.BudgetDuration == RepeatPattern.None ? 0 : Math.Ceiling(rs / t.BudgetDuration.GetHashCode());
                    if (rSavingPattern == 0)
                        t.RequiredSaving = t.TargetAmount;
                    else
                        t.RequiredSaving = Math.Round( t.TargetAmount / rSavingPattern, 2);

                    double expectedDays = 0;
                    if (t.TotalSaving == 0)
                    {
                        t.CurrentSaving = t.InitialAmount;
                        expectedDays = t.BudgetAmount < 1 ? rs : ((t.TargetAmount - t.InitialAmount) / t.BudgetAmount) * t.BudgetDuration.GetHashCode();
                    }
                    else
                    {
                        var timePassed = Math.Max(1, Math.Ceiling(DateTime.Now.Subtract(t.CreatedDate).TotalDays));
                        var budgetTime = Math.Max(1, t.BudgetDuration.GetHashCode());
                        var cSavingPattern = Math.Ceiling(timePassed / budgetTime);
                        t.CurrentSaving = Math.Round( (t.TotalSaving + t.InitialAmount) / cSavingPattern, 2);

                        var expectedBudgetTime = t.CurrentSaving == 0 ? rs : ((1 / t.CurrentSaving) * t.TargetAmount);
                        expectedDays = expectedBudgetTime * t.BudgetDuration.GetHashCode();
                    }

                    t.ExpectedDate = t.CreatedDate.AddDays(expectedDays);

                }
                else if (t.Status == Common.TargetStatus.Achieved)
                {
                    t.TargetPercentage = 100;
                }
                else
                    t.TargetPercentage = 0;
            }
            return targets;
        }
        [HttpPost]
        public JsonResult MEdit(int TargetID, string Name, double TargetAmount,double InitialAmount, DateTime TargetDate, TargetStatus Status, RepeatPattern BudgetDuration, double BudgetAmount)
        {
            return Edit<JsonResult>(TargetID, Name, TargetAmount,InitialAmount, TargetDate, Status, YesNoOptions.No,  BudgetDuration,  BudgetAmount);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public JsonResult JEdit(int TargetID, string Name, double TargetAmount, double InitialAmount, DateTime TargetDate, TargetStatus Status, RepeatPattern BudgetDuration, double BudgetAmount)
        {
            return Edit<JsonResult>(TargetID, Name, TargetAmount, InitialAmount, TargetDate, Status, YesNoOptions.No, BudgetDuration, BudgetAmount);
        }

        private T Edit<T>(int TargetID, string Name, double TargetAmount,double InitialAmount, DateTime TargetDate, TargetStatus Status, YesNoOptions Budgeted, RepeatPattern BudgetDuration, double BudgetAmount)where T:JsonResult
        {
                Target target = null; 
                Budget budget = null;

                if (TargetID == 0) //add
                {
                    target = new Target();
                    target.UserID = State.UserID;
                    budget = new Budget();
                    budget.UserID = State.UserID;
                    Status = Common.TargetStatus.Active;
                    target.CreatedDate = DateTime.Now;
                }
                else
                {
                    target = db.Targets.Where(t=>t.UserID == State.UserID && t.TargetID == TargetID).FirstOrDefault();
                    if(target == null)
                        return Repository.Failure<T>("Target not found.");
                    budget = target.Budget;
                }

                target.Name = Name;
                target.TargetAmount = TargetAmount;
                target.InitialAmount = InitialAmount;
                target.TargetDate = TargetDate;
                target.Status = Status;
                target.ExpectedDate = target.TargetDate;
                
                target.Budgeted = Budgeted;

                budget.BudgetAmount = BudgetAmount;
                budget.BudgetDuration = (RepeatPattern)BudgetDuration;
                budget.BudgetType = BudgetType.Target;
                    
                if (TargetID == 0)
	            {
                    target.Budget = budget;
                    db.Targets.Add(target);
	            }
                else
                {
                    db.Entry(target).State = EntityState.Modified;
                }
                db.SaveChanges();
                return Repository.Success<T>(target.TargetID);
        }

        public string TargetStatus()
        {

            string selectStr = "<select>" +
                                    string.Format("<option value='{0}' selected='selected'>{1}</option>", Common.TargetStatus.Active.GetHashCode(), Common.TargetStatus.Active.String()) +
                                    string.Format("<option value='{0}'>{1}</option>", Common.TargetStatus.Achieved.GetHashCode(), Common.TargetStatus.Achieved.String()) +
                                    string.Format("<option value='{0}' >{1}</option>", Common.TargetStatus.Cancelled.GetHashCode(), Common.TargetStatus.Cancelled.String()) +
                                "</select>";

            return selectStr;
        }

        public string JTargets()
        {
            string selectStr = "<option value=''></option>";

            if (Request.IsAjaxRequest())
            {
                var targets = db.Targets.Where(t=>t.UserID == State.UserID && t.Status == Common.TargetStatus.Active);
                foreach (var target in targets)
                {
                    selectStr += string.Format("<option value='{0}'>{1}</option>", target.TargetID, target.Name);
                }
            }

            selectStr = "<select>" + selectStr + "</select>";
            return selectStr;
        }
        public JsonResult MTargets()
        {
            var targets = db.Targets.Where(t => t.UserID == State.UserID && t.Status == Common.TargetStatus.Active);
                
            return Util.Package<JsonResult>(targets.Select(t =>
                   new
                   {
                       TargetID = t.TargetID,
                       Name = t.Name 
                   }), targets.Count());
        }
        [HttpPost]
        public JsonResult MDelete(int TargetID)
        {
            return Delete<JsonResult>(TargetID);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public JsonResult JDelete(int TargetID)
        {
            return Delete<JsonResult>(TargetID);
        }

        private T Delete<T>(int TargetID) where T : JsonResult
        {
                try
                {
Target target = db.Targets.Where(t => t.UserID == State.UserID && t.TargetID == TargetID).FirstOrDefault();
                if (target != null)
                {
                    db.Targets.Remove(target);
                    db.SaveChanges();
                    return Repository.Success<T>(target.TargetID);
                }
                }
                catch (Exception)
                {
                    
                }
                return Repository.DelFailure<T>();
        }
    }
}
