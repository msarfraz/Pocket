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

namespace Pocket.Controllers
{
    public class TargetController : Controller
    {
        private QDbContext db = new QDbContext();

        // GET: /Target/
        public ActionResult Index()
        {
            var targets = db.Targets.Include(t => t.User);
            return View(targets.ToList());
        }

        // GET: /Target/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Target target = db.Targets.Find(id);
            if (target == null)
            {
                return HttpNotFound();
            }
            return View(target);
        }

        // GET: /Target/Create
        public ActionResult Create()
        {
            ViewBag.UserID = new SelectList(db.Users, "UserID", "LoginID");
            return View();
        }

        // POST: /Target/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="TargetID,TargetAmount,ExpectedDate,Name,UserID")] Target target)
        {
            if (ModelState.IsValid)
            {
                db.Targets.Add(target);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.UserID = new SelectList(db.Users, "UserID", "LoginID", target.UserID);
            return View(target);
        }

        // GET: /Target/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Target target = db.Targets.Find(id);
            if (target == null)
            {
                return HttpNotFound();
            }
            ViewBag.UserID = new SelectList(db.Users, "UserID", "LoginID", target.UserID);
            return View(target);
        }

        // POST: /Target/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="TargetID,TargetAmount,ExpectedDate,Name,UserID")] Target target)
        {
            if (ModelState.IsValid)
            {
                db.Entry(target).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.UserID = new SelectList(db.Users, "UserID", "LoginID", target.UserID);
            return View(target);
        }

        // GET: /Target/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Target target = db.Targets.Find(id);
            if (target == null)
            {
                return HttpNotFound();
            }
            return View(target);
        }

        // POST: /Target/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Target target = db.Targets.Find(id);
            db.Targets.Remove(target);
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
            return View();
        }
        // GET: /Payee/
        public JsonResult JList(string sidx, string sord, int page, int rows)
        {
            if (Request.IsAjaxRequest())
            {
                var targets = db.Users.Find(State.UserID).Targets;

                return Util.CreateJsonResponse<Target>(sidx, sord, page, rows, targets, (Func<IEnumerable<Target>, Array>)delegate(IEnumerable<Target> rd)
                {
                    return (
                        from target in rd
                        select new
                        {
                            TargetID = target.TargetID,
                            cell = new string[] {target.TargetID.ToString(), target.Name, target.TargetAmount.ToString(), target.TargetDate.ToShortDateString() , target.ExpectedDate.ToShortDateString(), target.Status == Common.TargetStatus.InActive ? "InActive" : "Active" }
                        }).ToArray();
                }
                    );
            }
            else
                return Json(new HttpStatusCodeResult(HttpStatusCode.BadRequest));

        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public JsonResult JEdit([Bind(Include = "TargetID,Name,TargetAmount,TargetDate,Status")] Target target)
        {
            if (Request.IsAjaxRequest() && ModelState.IsValid)
            {
                target.UserID = State.UserID;
                if (target.TargetID == 0) //add
                {
                    target.ExpectedDate = target.TargetDate;
                    db.Targets.Add(target);
                }
                else
                {
                    Target tar = db.Users.Find(State.UserID).Targets.Find(t => t.TargetID == target.TargetID);
                    if (tar == null)
                        return Json( HttpNotFound());
                    tar.Name = target.Name;
                    tar.TargetAmount = target.TargetAmount;
                    tar.TargetDate = target.TargetDate;
                    tar.Status = target.Status;

                    db.Entry(tar).State = EntityState.Modified;
                }
                db.SaveChanges();
                return Json(new
                {
                    success = true,
                    message = "success",
                    new_id = target.TargetID
                });
            }
            return Json(new
            {
                success = false,
                message = "Model state is invalid.",
                new_id = 0
            });
        }

        public string TargetStatus()
        {

            string selectStr = "<select>" +
                                    "<option value='" + Pocket.Common.TargetStatus.InActive.GetHashCode() + "'>InActive</option>" +
                                    "<option value='" + Pocket.Common.TargetStatus.Active.GetHashCode() + "'>Active</option>" +
                                "</select>";

            return selectStr;
        }
    }
}
