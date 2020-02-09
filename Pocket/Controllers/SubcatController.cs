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
    public class SubcatController : Controller
    {
        private QDbContext db = new QDbContext();

        // GET: /Subcat/
        public ActionResult Index()
        {
            var subcategories = db.Users.Find(State.UserID).Categories;
            return View(subcategories);
        }

        // GET: /Subcat/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Subcategory subcategory = db.Subcategories.Find(id);
            if (subcategory == null)
            {
                return HttpNotFound();
            }
            return View(subcategory);
        }

        // GET: /Subcat/Create
        public ActionResult Create()
        {
            //ViewBag.BudgetID = new SelectList(db.Budgets, "BudgetID", "BudgetID");
            //ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "Name");
            //ViewBag.IconID = new SelectList(db.Icons, "IconID", "Name");
            return View();
        }

        // POST: /Subcat/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "CategoryID,Name,Budget_BudgetAmount")] int CategoryID, string Name, int Budget_BudgetAmount)
        {
            if (ModelState.IsValid)
            {
                Category cat = db.Users.Find(State.UserID).Categories.Find(c => c.CategoryID == CategoryID);
                if (cat != null)
                {
                    Subcategory subcat = new Subcategory();
                    subcat.CategoryID = CategoryID;
                    subcat.Name = Name;
                    Budget budge = new Budget();
                    budge.BudgetAmount = Budget_BudgetAmount;//.HasValue ? BudgetAmount.Value : 0;
                    budge.UserID = State.UserID;
                    db.Budgets.Add(budge);
                    db.SaveChanges();
                    subcat.BudgetID = budge.BudgetID;

                    db.Subcategories.Add(subcat);
                    db.SaveChanges();

                    return RedirectToAction("Index");
                }
                
            }
            
            return View();
        }

        // GET: /Subcat/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Subcategory subcategory = db.Subcategories.Find(id);
            if (subcategory == null)
            {
                return HttpNotFound();
            }
            if (subcategory.Category.UserID != State.UserID)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            return View(subcategory);
        }

        // POST: /Subcat/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="SubcategoryID,Name,Budget.BudgetAmount")] Subcategory scat)
        {
            if (ModelState.IsValid)
            {
                Category cat = db.Users.Find(State.UserID).Categories.Find(c => c.Subcategories.Find(sc => sc.SubcategoryID == scat.SubcategoryID) != null);
                if(cat != null)
                {
                    Subcategory subcat = cat.Subcategories.Find(sc => sc.SubcategoryID == scat.SubcategoryID);

                    subcat.Name = scat.Name;
                    db.Entry(subcat).State = EntityState.Modified; 
                    
                    Budget budge = db.Users.Find(State.UserID).Budgets.Find(b => b.BudgetID == subcat.BudgetID);
                    budge.BudgetAmount = scat.Budget.BudgetAmount;
                    db.Entry(budge).State = EntityState.Modified;
                    db.SaveChanges();

                    return RedirectToAction("Index");
                }
                
            }
            return View();
        }

        // GET: /Subcat/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Subcategory subcategory = db.Subcategories.Find(id);
            if (subcategory == null)
            {
                return HttpNotFound();
            }
            return View(subcategory);
        }

        // POST: /Subcat/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Subcategory subcategory = db.Subcategories.Find(id);
            db.Subcategories.Remove(subcategory);
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

        // GET: /Subcategory/
        public JsonResult JList(string sidx, string sord, int? page, int? rows, int? CategoryID)
        {
            if (!page.HasValue)
                page = 1;
            if (!rows.HasValue)
                rows = 10;
            if (string.IsNullOrEmpty(sidx))
                sidx = "SubcategoryID";
            if (string.IsNullOrEmpty(sord))
                sord = "asc";
            if (Request.IsAjaxRequest() && CategoryID != null)
            {
                Category category = db.Users.Find(State.UserID).Categories.Find(cat => cat.CategoryID == CategoryID);
                if (category != null)
                {
                    var subcats = category.Subcategories;
                    return Util.CreateJsonResponse<Subcategory>(sidx, sord, page.Value, rows.Value, subcats, (Func<IEnumerable<Subcategory>, Array>)delegate(IEnumerable<Subcategory> rd)
                    {
                        return (
                            from subcategory in rd
                            select new
                            {
                                SubcategoryID = subcategory.SubcategoryID,
                                cell = new string[] { subcategory.SubcategoryID.ToString(), subcategory.CategoryID.ToString(), subcategory.Name.ToString(), subcategory.Budget.BudgetAmount.ToString(), Global.RepeatToString(RepeatPattern.Monthly) }
                            }).ToArray();
                    }
                    );
                }

                
            }
                
            return Json(new HttpStatusCodeResult(HttpStatusCode.BadRequest));

        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public JsonResult JEdit([Bind(Include = "SubcategoryID, CategoryID,Name, BudgetAmount,Recursive")] int SubcategoryID, int CategoryID, string Name, int BudgetAmount, int? Recursive)
        {
            if (Request.IsAjaxRequest() && ModelState.IsValid)
            {
                Category category = db.Users.Find(State.UserID).Categories.Find(cat => cat.CategoryID == CategoryID);
                Subcategory subcategory = new Subcategory();
                if (category != null) // security check for user
                {
                    if (SubcategoryID == 0) //add
                    {
                        
                        subcategory.CategoryID = CategoryID;
                        subcategory.Name = Name;
                        
                        Budget budge = new Budget();
                        budge.BudgetAmount = BudgetAmount;//.HasValue ? BudgetAmount.Value : 0;
                        budge.BudgetDuration = RepeatPattern.Monthly;// Recursive;
                        budge.UserID = State.UserID;
                        db.Budgets.Add(budge);
                        db.SaveChanges();
                        subcategory.BudgetID = budge.BudgetID;

                        db.Subcategories.Add(subcategory);

                    }
                    else
                    {
                        subcategory = category.Subcategories.Find(sc => sc.SubcategoryID == SubcategoryID); 
                        subcategory.Name = Name;
                        subcategory.Budget.BudgetAmount = BudgetAmount;
                        subcategory.Budget.BudgetDuration = RepeatPattern.Monthly;
                        db.Entry(subcategory).State = EntityState.Modified;
                    }
                    db.SaveChanges();
                    return Json(new
                    {
                        success = true,
                        message = "success",
                        new_id = subcategory.CategoryID
                    });
                }
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
