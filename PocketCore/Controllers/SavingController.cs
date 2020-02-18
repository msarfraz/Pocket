using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pocket.Common;
using Pocket.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pocket.Controllers
{
    [Authorize]
    public class SavingController : ApplicationController
    {
        public SavingController(ApplicationDbContext
            context) : base(context)
        {
            
        }
        // GET: /Payee/Index
        public ActionResult Index(int TargetID)
        {
            return View(TargetID);
        }
        public JsonResult MRecord(int SavingID)
        {
            var savings = db.Savings.Where(s => s.UserID == UserID && s.SavingID == SavingID).ToList();
            return Util.Package<JsonResult>(savings.Select(s => new
            {
                SavingID = s.SavingID,
                SavingDate = s.SavingDate.ToUTCDateString(),
                TargetID = s.TargetID,
                TargetText = s.Target.Name,
                AccountID = s.AccountID,
                AccountText = s.Account.Name,
                Amount = s.Amount
            }));
        }
        public JsonResult MList(int page, int rows, int? TargetID)
        {
            return (JsonResult) GetList("SavingDate", "desc", page, rows, ResultType.Mobile, TargetID);
        }
        // GET: /Payee/
        public JsonResult JList(string sidx, string sord, int page, int rows, int? TargetID)
        {
            
            //int TargetID = Util.ParseInt(Request.Params["TargetID"]);
            return GetList(sidx, sord, page, rows, ResultType.Web, TargetID);
        }
        // GET: /Payee/
        private JsonResult GetList(string sidx, string sord, int page, int rows, ResultType rt, int? TargetID)
        {
                var savings = db.Savings.Where(s => s.UserID == UserID);
                if (TargetID.HasValue)
                {
                    savings = savings.Where(s => s.TargetID == TargetID);
                }

                return Util.CreateJsonResponse<Saving>(sidx, sord, page, rows, savings, rt, (Func<IEnumerable<Saving>, Array>)delegate(IEnumerable<Saving> rd)
                {
                    if (rt == ResultType.Web)
                    {
                        return (
                        from s in rd
                        select new
                        {
                            SavingID = s.SavingID,
                            cell = new string[] { s.SavingID.ToString(), s.SavingDate.ToDateString(), s.Account.Name, s.Amount.ToString() }
                        }).ToArray();
                    }
                    else
                    {
                        return (
                        from s in rd
                        select new
                        {
                            SavingID = s.SavingID,
                            SavingDate = s.SavingDate.ToDateDisplayString(),
                            TargetID = s.TargetID,
                            TargetText = s.Target.Name,
                            AccountID = s.AccountID,
                            AccountText = s.Account.Name,
                            Amount = s.Amount
                        }).ToArray();
                    }
                }
                    );

        }
        [HttpPost]
        public JsonResult MEdit( int SavingID, int TargetID, int AccountID, double Amount, DateTime SavingDate)
        {
            return Edit<JsonResult>(SavingID, TargetID, AccountID, Amount, SavingDate);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public JsonResult JEdit(int SavingID, int TargetID, int AccountID, double Amount, DateTime SavingDate)
        {
            return Edit<JsonResult>(SavingID, TargetID, AccountID, Amount, SavingDate);
        }

        private T Edit<T>(int SavingID, int TargetID, int AccountID, double Amount, DateTime SavingDate) where T : JsonResult
        {
                Saving saving = null;
                if (SavingID == 0)
                {
                    saving = new Saving();
                    saving.UserID = UserID;
                    saving.CreatedDate = DateTime.Now;
                }
                else
                {
                    saving = db.Savings.Where(s => s.UserID == UserID && s.SavingID == SavingID).FirstOrDefault();
                    if (saving == null)
                    {
                        return Repository.Failure<T>();
                    }
                }
                saving.SavingDate = SavingDate;
                saving.TargetID = TargetID;
                saving.AccountID = AccountID;
                saving.Amount = Amount;

                if (SavingID == 0)
                    db.Savings.Add(saving);
                else
                    db.Entry(saving).State = EntityState.Modified;

                
                db.SaveChanges();
                return Repository.Success<T>(saving.SavingID);
        }
        [HttpPost]
        public JsonResult MDelete(int SavingID)
        {
            return Delete<JsonResult>(SavingID);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public JsonResult JDelete(int SavingID)
        {
            return Delete<JsonResult>(SavingID);
        }

        private T Delete<T>(int SavingID) where T : JsonResult
        {
                try
                {
Saving saving = db.Savings.Where(s => s.UserID == UserID && s.SavingID == SavingID).FirstOrDefault();
                if (saving != null)
                {
                    db.Savings.Remove(saving);
                    db.SaveChanges();
                    return Repository.Success<T>(saving.SavingID);
                }
                }
                catch (Exception)
                {
                    
                }
                return Repository.DelFailure<T>();
        }
       
	}
}