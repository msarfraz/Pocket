using Pocket.Common;
using Pocket.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace Pocket.Controllers
{
    [Authorize]
    public class SubcatController : ApplicationController
    {
        public SubcatController(ApplicationDbContext
            context) : base(context)
        {
            
        }

        // GET: /Subcat/
        public ActionResult Index()
        {
            var subcategories = db.Users.Find(UserID).Categories;
            return View(subcategories);
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
        public IActionResult JList(string sidx, string sord, int? page, int? rows, int? CategoryID)
        {
            if (!page.HasValue)
                page = 1;
            if (!rows.HasValue)
                rows = 10;
            if (string.IsNullOrEmpty(sidx))
                sidx = "SubcategoryID";
            if (string.IsNullOrEmpty(sord))
                sord = "asc";
            if (CategoryID != null)
            {
                Category category = db.Users.Find(UserID).Categories.Find(cat => cat.CategoryID == CategoryID);
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
                                cell = new string[] { subcategory.SubcategoryID.ToString(), subcategory.CategoryID.ToString(), subcategory.Name.ToString(), subcategory.Budget.BudgetAmount.ToString(), subcategory.Budget.BudgetDuration.String() }
                            }).ToArray();
                    }
                    );
                }

                
            }
                
            return BadRequest();

        }
        [HttpPost]
        public JsonResult MEdit(int SubcategoryID, int CategoryID, string Name, int BudgetAmount, int BudgetDuration)
        {
            return Edit<JsonResult>(SubcategoryID, CategoryID, Name, BudgetAmount, BudgetDuration);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public JsonResult JEdit([Bind( "SubcategoryID, CategoryID,Name, BudgetAmount,BudgetDuration")] int SubcategoryID, int CategoryID, string Name, int BudgetAmount, int BudgetDuration)
        {
            return Edit<JsonResult>(SubcategoryID, CategoryID, Name, BudgetAmount, BudgetDuration);
        }

        private T Edit<T>(int SubcategoryID, int CategoryID, string Name, int BudgetAmount, int BudgetDuration)where T:JsonResult
        {
            if (ModelState.IsValid)
            {
                Category category = db.Users.Find(UserID).Categories.Find(cat => cat.CategoryID == CategoryID);
                Subcategory subcategory = new Subcategory();
                if (category != null) // security check for user
                {
                    if (SubcategoryID == 0) //add
                    {
                        
                        subcategory.CategoryID = CategoryID;
                        subcategory.Name = Name;
                        
                        Budget budge = new Budget();
                        budge.BudgetAmount = BudgetAmount;//.HasValue ? BudgetAmount.Value : 0;
                        budge.BudgetDuration = (RepeatPattern)BudgetDuration;
                        budge.UserID = UserID;
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
                        subcategory.Budget.BudgetDuration = (RepeatPattern)BudgetDuration;
                        db.Entry(subcategory).State = EntityState.Modified;
                    }
                    db.SaveChanges();
                    return Repository.Success<T>(subcategory.SubcategoryID);
                }
            }
            return Repository.Failure<T>();
        }
        [HttpPost]
        public JsonResult MDelete(int SubcategoryID)
        {
            return Delete<JsonResult>(SubcategoryID);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public JsonResult JDelete(int SubcategoryID)
        {
            return Delete<JsonResult>(SubcategoryID);
        }

        private T Delete<T>(int SubcategoryID) where T : JsonResult
        {
                try
                {
                    Subcategory subcat = db.Subcategories.Where(s => s.Category.UserID == UserID && s.SubcategoryID == SubcategoryID).FirstOrDefault();
                    if (subcat != null)
                    {
                        db.Subcategories.Remove(subcat);
                        db.SaveChanges();
                        return Repository.Success<T>(subcat.SubcategoryID);
                    }
                }
                catch (Exception ex)
                {
                    
                }
return Repository.DelFailure<T>();
        }
    }
}
