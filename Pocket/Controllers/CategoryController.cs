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
using System.Data.Common;

namespace Pocket.Controllers
{
    public class CategoryController : Controller
    {
        private QDbContext db = new QDbContext();

        // GET: /Category/
        public ActionResult Index()
        {
            return View(db.Categories.ToList());
        }

        // GET: /Category/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }

        // GET: /Category/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: /Category/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="CategoryID,Name,IconID")] Category category)
        {
            if (ModelState.IsValid)
            {

                category.UserID = State.UserID;
                db.Categories.Add(category);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(category);
        }

        // GET: /Category/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }

        // POST: /Category/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "CategoryID,Name,IconID")] Category category)
        {
            if (ModelState.IsValid)
            {
                db.Entry(category).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(category);
        }

        // GET: /Category/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }

        // POST: /Category/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Category category = db.Categories.Find(id);
            db.Categories.Remove(category);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // get: /Category/SubcatList/5
        public JsonResult SubcatList(int? id)
        {
            List<SelectListItem> sl = new List<SelectListItem>(); 
            
            if (Request.IsAjaxRequest() && id != null)
            {
                Category category = db.Users.Find(State.UserID).Categories.Find(cat => cat.CategoryID == id);
                if (category != null)
                {
                    foreach (Subcategory sc in category.Subcategories)
                    {
                        SelectListItem sli = new SelectListItem();
                        sli.Text = sc.Name;
                        sli.Value = sc.SubcategoryID.ToString();
                        sl.Add(sli);
                    }
                }
            }
            
            return Json(sl, JsonRequestBehavior.AllowGet);
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
            return View(db.Users.Find(State.UserID).Categories);
        }
        // GET: /Payee/
        public JsonResult JList(string sidx, string sord, int page, int rows)
        {
            if (Request.IsAjaxRequest())
            {
                db.Database.Log = Console.Write;
                var categories = db.Categories.Where(cat => cat.UserID == State.UserID).AsNoTracking();

                return Util.CreateJsonResponse<Category>(sidx, sord, page, rows, categories, (Func<IEnumerable<Category>, Array>)delegate(IEnumerable<Category> rd)
                {
                    //var abc = rd.Select(c => new { });
                    return (
                        from category in rd
                        select new
                        {
                            CategoryID = category.CategoryID,
                            cell = new string[] { category.CategoryID.ToString(), category.Name.ToString() }
                        }).ToArray();
                }
                    );
            }
            else
                return Json(new HttpStatusCodeResult(HttpStatusCode.BadRequest));

        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public JsonResult JEdit([Bind(Include = "CategoryID,Name")] Category category)
        {
            if (Request.IsAjaxRequest() && ModelState.IsValid)
            {
                category.UserID = State.UserID;
                if (category.CategoryID == 0) //add
                {
                    db.Categories.Add(category);
                }
                else
                {
                    db.Entry(category).State = EntityState.Modified;
                }
                db.SaveChanges();
                return Json(new
                {
                    success = true,
                    message = "success",
                    new_id = category.CategoryID
                });
            }
            return Json(new
            {
                success = false,
                message = "Model state is invalid.",
                new_id = 0
            });
        }

        public string JCategories()
        {
            string selectStr = string.Empty;

            if (Request.IsAjaxRequest())
            {
                var categories = db.Users.Find(State.UserID).Categories;
                foreach (var category in categories)
                {
                    var sc = string.Empty;
                    foreach (var subcat in category.Subcategories)
                    {
                        sc += string.Format("<option value='{0}'>{1}</option>", subcat.SubcategoryID, subcat.Name);
                    }

                    selectStr += string.Format("<optgroup label='{0}'>{1}</optgroup>", category.Name, sc);
                }
            }

            selectStr = "<select>" + selectStr + "</select>";
            return selectStr;
            //return Json(selectStr, JsonRequestBehavior.AllowGet);
        }
    }
}
