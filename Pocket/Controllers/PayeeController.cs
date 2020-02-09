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
    public class PayeeController : Controller
    {
        private QDbContext db = new QDbContext();

        // GET: /Payee/
        public ActionResult Index()
        {
            var payees = db.Users.Find(State.UserID).Payees;
            return View(payees.ToList());
        }
        // GET: /Payee/List
        public ActionResult List()
        {
            var payees = db.Users.Find(State.UserID).Payees;
            return View(payees.ToList());
        }
        
        // GET: /Payee/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Payee payee = db.Payees.Find(id);
            if (payee == null)
            {
                return HttpNotFound();
            }
            return View(payee);
        }

        // GET: /Payee/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: /Payee/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="PayeeID,Name")] Payee payee)
        {
            if (ModelState.IsValid)
            {
                payee.UserID = State.UserID;
                db.Payees.Add(payee);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.UserID = new SelectList(db.Users, "UserID", "LoginID", payee.UserID);
            return View(payee);
        }

        // GET: /Payee/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Payee payee = db.Payees.Find(id);
            if (payee == null)
            {
                return HttpNotFound();
            }
            ViewBag.UserID = new SelectList(db.Users, "UserID", "LoginID", payee.UserID);
            return View(payee);
        }

        // POST: /Payee/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="PayeeID,UserID,Name")] Payee payee)
        {
            if (ModelState.IsValid)
            {
                db.Entry(payee).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.UserID = new SelectList(db.Users, "UserID", "LoginID", payee.UserID);
            return View(payee);
        }

        // GET: /Payee/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Payee payee = db.Payees.Find(id);
            if (payee == null)
            {
                return HttpNotFound();
            }
            return View(payee);
        }

        // POST: /Payee/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Payee payee = db.Payees.Find(id);
            db.Payees.Remove(payee);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        [ValidateAntiForgeryToken]
        [HttpPost]
        public JsonResult JEdit([Bind(Include = "PayeeID,Name")] Payee payee)
        {
            if (Request.IsAjaxRequest() && ModelState.IsValid)
            {
                payee.UserID = State.UserID;
                if (payee.PayeeID == 0) //add
                {
                    db.Payees.Add(payee);
                }
                else
                {
                    db.Entry(payee).State = EntityState.Modified;
                }
                db.SaveChanges();
                return Json(new {
                success = true,
                message = "success",
                new_id = payee.PayeeID
                });
            }
            //ViewBag.UserID = new SelectList(db.Users, "UserID", "LoginID", payee.UserID);
            return Json(payee);
        }
        // GET: /Payee/
        public JsonResult JIndex(string sidx, string sord, int page, int rows)
        {
            if (Request.IsAjaxRequest())
            {
                var payees = db.Users.Find(State.UserID).Payees;

                return Util.CreateJsonResponse<Payee>(sidx, sord, page, rows, payees, (Func<IEnumerable<Payee>, Array>)delegate(IEnumerable<Payee> rd)
                {
                    return (
                        from question in rd
                        select new
                        {
                            PayeeID = question.PayeeID,
                            cell = new string[] { question.PayeeID.ToString(), question.Name.ToString() }
                        }).ToArray();
                }
                    );
            }
            else
                return Json(new HttpStatusCodeResult(HttpStatusCode.BadRequest));
           
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
