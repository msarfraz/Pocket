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
    public class QAccountController : Controller
    {
        private QDbContext db = new QDbContext();

        // GET: /Account/
        public ActionResult Index()
        {
            var accounts = db.Users.Find(State.UserID).Accounts;
            return View(accounts);
        }

        // GET: /Account/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Account account = db.Accounts.Find(id);
            if (account == null)
            {
                return HttpNotFound();
            }
            return View(account);
        }

        // GET: /Account/Create
        public ActionResult Create()
        {
           // ViewBag.UserID = new SelectList(db.Users, "UserID", "LoginID");
            return View();
        }

        // POST: /Account/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="AccountID,Name,InitialAmount,CurrentAmount")] Account account)
        {
            if (ModelState.IsValid)
            {
                account.UserID = State.UserID;
                db.Accounts.Add(account);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            //ViewBag.UserID = new SelectList(db.Users, "UserID", "LoginID", account.UserID);
            return View(account);
        }

        // GET: /Account/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Account account = db.Accounts.Find(id);
            if (account == null)
            {
                return HttpNotFound();
            }
           // ViewBag.UserID = new SelectList(db.Users, "UserID", "LoginID", account.UserID);
            return View(account);
        }

        // POST: /Account/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="AccountID,Name,InitialAmount,CurrentAmount")] Account account)
        {
            if (ModelState.IsValid)
            {
                account.UserID = State.UserID;
                db.Entry(account).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.UserID = new SelectList(db.Users, "UserID", "LoginID", account.UserID);
            return View(account);
        }

        // GET: /Account/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Account account = db.Users.Find(State.UserID).Accounts.Find(acc => acc.AccountID == id);
            if (account == null)
            {
                return HttpNotFound();
            }
            return View(account);
        }

        // POST: /Account/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Account account = db.Users.Find(State.UserID).Accounts.Find(acc => acc.AccountID == id);
            db.Accounts.Remove(account);
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
            return View(db.Users.Find(State.UserID).Accounts);
        }
        // GET: /Payee/
        public JsonResult JList(string sidx, string sord, int page, int rows)
        {
            if (Request.IsAjaxRequest())
            {
                var accounts = db.Users.Find(State.UserID).Accounts;

                return Util.CreateJsonResponse<Account>(sidx, sord, page, rows, accounts, (Func<IEnumerable<Account>, Array>)delegate(IEnumerable<Account> rd)
                {
                    return (
                        from account in rd
                        select new
                        {
                            PayeeID = account.AccountID,
                            cell = new string[] { account.AccountID.ToString(), account.Name.ToString(), account.InitialAmount.ToString() }
                        }).ToArray();
                }
                    );
            }
            else
                return Json(new HttpStatusCodeResult(HttpStatusCode.BadRequest));

        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public JsonResult JEdit([Bind(Include = "AccountID,Name,InitialAmount")] Account account)
        {
            if (Request.IsAjaxRequest() && ModelState.IsValid)
            {
                account.UserID = State.UserID;
                if (account.AccountID == 0) //add
                {
                    db.Accounts.Add(account);
                }
                else
                {
                    db.Entry(account).State = EntityState.Modified;
                }
                db.SaveChanges();
                return Json(new
                {
                    success = true,
                    message = "success",
                    new_id = account.AccountID
                });
            }
            return Json(new
            {
                success = false,
                message = "Model state is invalid.",
                new_id = 0
            });
        }

        public string JAccounts()
        {
            string selectStr = string.Empty;

           // if (Request.IsAjaxRequest())
            //{
                var accounts = db.Users.Find(State.UserID).Accounts;
                foreach (var account in accounts)
                {
                    selectStr += string.Format("<option value='{0}'>{1}</option>", account.AccountID, account.Name);
                }
            //}

            selectStr = "<select>" + selectStr + "</select>";
            return selectStr;
            //return Json(selectStr, JsonRequestBehavior.AllowGet);
        }
    }
}
