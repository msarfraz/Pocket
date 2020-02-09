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
    public class IncomeSourceController : Controller
    {
        private QDbContext db = new QDbContext();

        // GET: /IncomeSource/
        public ActionResult Index()
        {
            var incomesources = db.IncomeSources.Include(i => i.IconFile).Include(i => i.User);
            return View(incomesources.ToList());
        }

        // GET: /IncomeSource/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            IncomeSource incomesource = db.IncomeSources.Find(id);
            if (incomesource == null)
            {
                return HttpNotFound();
            }
            return View(incomesource);
        }

        // GET: /IncomeSource/Create
        public ActionResult Create()
        {
            ViewBag.IconID = new SelectList(db.Icons, "IconID", "Name");
            ViewBag.UserID = new SelectList(db.Users, "UserID", "LoginID");
            return View();
        }

        // POST: /IncomeSource/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="SourceID,UserID,Name,IconID")] IncomeSource incomesource)
        {
            if (ModelState.IsValid)
            {
                db.IncomeSources.Add(incomesource);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.IconID = new SelectList(db.Icons, "IconID", "Name", incomesource.IconID);
            ViewBag.UserID = new SelectList(db.Users, "UserID", "LoginID", incomesource.UserID);
            return View(incomesource);
        }

        // GET: /IncomeSource/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            IncomeSource incomesource = db.IncomeSources.Find(id);
            if (incomesource == null)
            {
                return HttpNotFound();
            }
            ViewBag.IconID = new SelectList(db.Icons, "IconID", "Name", incomesource.IconID);
            ViewBag.UserID = new SelectList(db.Users, "UserID", "LoginID", incomesource.UserID);
            return View(incomesource);
        }

        // POST: /IncomeSource/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="SourceID,UserID,Name,IconID")] IncomeSource incomesource)
        {
            if (ModelState.IsValid)
            {
                db.Entry(incomesource).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.IconID = new SelectList(db.Icons, "IconID", "Name", incomesource.IconID);
            ViewBag.UserID = new SelectList(db.Users, "UserID", "LoginID", incomesource.UserID);
            return View(incomesource);
        }

        // GET: /IncomeSource/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            IncomeSource incomesource = db.IncomeSources.Find(id);
            if (incomesource == null)
            {
                return HttpNotFound();
            }
            return View(incomesource);
        }

        // POST: /IncomeSource/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            IncomeSource incomesource = db.IncomeSources.Find(id);
            db.IncomeSources.Remove(incomesource);
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

        public string JSources()
        {
            string selectStr = string.Empty;

            if (Request.IsAjaxRequest())
            {
                var sources = db.Users.Find(State.UserID).IncomeSources;
                foreach (var source in sources)
                {
                    selectStr += string.Format("<option value='{0}'>{1}</option>", source.SourceID, source.Name);
                }
            }

            selectStr = "<select>" + selectStr + "</select>";
            return selectStr;
            //return Json(selectStr, JsonRequestBehavior.AllowGet);
        }

        // GET: /Payee/List
        public ActionResult List()
        {
            return View(db.Users.Find(State.UserID).IncomeSources);
        }
        // GET: /Payee/
        public JsonResult JList(string sidx, string sord, int page, int rows)
        {
            if (Request.IsAjaxRequest())
            {
                var sources = db.Users.Find(State.UserID).IncomeSources;

                return Util.CreateJsonResponse<IncomeSource>(sidx, sord, page, rows, sources, (Func<IEnumerable<IncomeSource>, Array>)delegate(IEnumerable<IncomeSource> rd)
                {
                    return (
                        from source in rd
                        select new
                        {
                            SourceID = source.SourceID,
                            cell = new string[] { source.SourceID.ToString(), source.Name}
                        }).ToArray();
                }
                    );
            }
            else
                return Json(new HttpStatusCodeResult(HttpStatusCode.BadRequest));

        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public JsonResult JEdit([Bind(Include = "SourceID,Name")] IncomeSource source)
        {
            if (Request.IsAjaxRequest() && ModelState.IsValid)
            {
                source.UserID = State.UserID;
                if (source.SourceID == 0) //add
                {
                    db.IncomeSources.Add(source);
                }
                else
                {
                    db.Entry(source).State = EntityState.Modified;
                }
                db.SaveChanges();
                return Json(new
                {
                    success = true,
                    message = "success",
                    new_id = source.SourceID
                });
            }
            return Json(new
            {
                success = false,
                message = "Model state is invalid.",
                new_id = 0
            });
        }
    }
}
