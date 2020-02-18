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
using Pocket.Extensions;

namespace Pocket.Controllers
{
    [Authorize]
    public class PayeeController : Controller
    {
        private QDbContext db = new QDbContext();

        // GET: /Payee/Index
        public ActionResult Index()
        {
            var payees = db.Users.Find(State.UserID).Payees;
            return View(payees.ToList());
        }
        [HttpPost]
        public JsonResult MEdit([Bind(Include = "PayeeID,Name")] int PayeeID, string Name)
        {
            return Edit<JsonResult>(PayeeID, Name);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public JsonResult JEdit([Bind(Include = "PayeeID,Name")] int PayeeID, string Name)
        {
            return Edit<JsonResult>(PayeeID, Name);
        }
       
        private T Edit<T>([Bind(Include = "PayeeID,Name")] int PayeeID, string Name) where T:JsonResult
        {
            if (ModelState.IsValid)
            {
                Payee payee = db.Payees.Where(p => p.PayeeID == PayeeID && p.UserID == State.UserID).FirstOrDefault();
                if (payee == null) //add
                {
                    payee = new Payee();
                payee.UserID = State.UserID;
                payee.Name = Name;
                    db.Payees.Add(payee);
                }
                else
                {
                    payee.Name = Name;
                    db.Entry(payee).State = EntityState.Modified;
                }
                db.SaveChanges();
                return Repository.Success<T>(payee.PayeeID);
                
            }
            return Repository.Failure<T>();
        }
        
        public JsonResult MList()
        {
            return GetList("Name", "asc", 1, 100, ResultType.Mobile);
        }
        public JsonResult JIndex(string sidx, string sord, int page, int rows)
        {
            return GetList(sidx, sord, page, rows, ResultType.Web);
        }
        private JsonResult GetList(string sidx, string sord, int page, int rows, ResultType rt)
        {
                var payees = db.Users.Find(State.UserID).Payees;

                return Util.CreateJsonResponse<Payee>(sidx, sord, page, rows, payees, rt, (Func<IEnumerable<Payee>, Array>)delegate(IEnumerable<Payee> rd)
                {
                    if (rt == ResultType.Web)
                    {
                        return (
                        from p in rd
                        select new
                        {
                            PayeeID = p.PayeeID,
                            cell = new string[] { p.PayeeID.ToString(), p.Name.ToString() }
                        }).ToArray();
                    }
                    else
                    {
                        return (
                        from p in rd
                        select new
                        {
                            PayeeID = p.PayeeID,
                            Name = p.Name
                        }).ToArray();
                    }
                }
                    );
           
        }

        public string JPayees()
        {
            string selectStr = "<option value=''></option>";

            if (Request.IsAjaxRequest())
            {
                var payees = db.Users.Find(State.UserID).Payees;
                foreach (var payee in payees)
                {
                    selectStr += string.Format("<option value='{0}'>{1}</option>", payee.PayeeID, payee.Name);
                }
            }

            selectStr = "<select>" + selectStr + "</select>";
            return selectStr;
        }
        [HttpPost]
        public JsonResult MDelete(int PayeeID)
        {
            return Delete<JsonResult>(PayeeID);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public JsonResult JDelete(int PayeeID)
        {
            return Delete<JsonResult>(PayeeID);
        }

        private T Delete<T>(int PayeeID) where T : JsonResult
        {
                try
                {
Payee payee = db.Payees.Where(p => p.UserID == State.UserID && p.PayeeID == PayeeID).FirstOrDefault();
                if (payee != null)
                {
                    db.Payees.Remove(payee);
                    db.SaveChanges();
                    return Repository.Success<T>(payee.PayeeID);
                }
                }
                catch (Exception)
                {
                    
                }
                return Repository.DelFailure<T>();
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
