using Pocket.Common;
using Pocket.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Pocket.ViewModels;

namespace Pocket.Controllers
{
    [Authorize]
    public class CategoryController : ApplicationController
    {
        public CategoryController(ApplicationDbContext
            context) : base(context)
        {
        }

        // GET: /Payee/Index
        public ActionResult Index()
        {
            return View(db.Users.Find(UserID).Categories);
        }
        // GET: /Payee/
        public JsonResult MCategories()
        {
            
                GroupList<MutableTuple<int, string, List<MutableTuple<int, string>>>> groups = new GroupList<MutableTuple<int, string, List<MutableTuple<int, string>>>>();

                var mcategories = db.Categories.Where(cat => cat.UserID == UserID).ToList();
                var fcategories = db.CategoryUsers.Where(ucat => ucat.UserID == UserID).Select(uc => uc.Category).ToList();
                var categories = mcategories.Union(fcategories);

                foreach (var cat in categories)
                {
                    List<MutableTuple<int, string>> subcats = new List<MutableTuple<int, string>>();
                    foreach (var scat in cat.Subcategories)
                    {
                        subcats.Add(new MutableTuple<int, string>(scat.SubcategoryID, scat.Name));
                    }
                    groups.AddGroupDetails(cat.UserID == UserID ? "" : cat.User.UserName, new MutableTuple<int, string, List<MutableTuple<int, string>>>(cat.CategoryID, cat.Name, subcats));
                }
                return Util.Package<JsonResult>(groups.Groups.Select(g =>
                       new
                       {
                           Name = g.Name,
                           Categories = g.GroupDetails.Select(mt => new { CategoryID = mt.Item1, Name = mt.Item2, Subcategories = mt.Item3.Select(scat=>new {
                           SubcategoryID = scat.Item1,
                           Name = scat.Item2
                           }) })
                       }), categories.Count());

                
           
        }
        public JsonResult MCategoryByID(int CategoryID)
        {
            
                var categories = Repository.GetAllCategoryBudget(db, UserID).Where(cba=>cba.CategoryID == CategoryID).ToList();

                var scats = db.Subcategories.Where(scat => scat.CategoryID == CategoryID).ToList().Select(scat => new SubcategoryBudgetAmount { Name = scat.Name, Budget = scat.Budget.BudgetAmount, Amount = 0, BudgetDuration = scat.Budget.BudgetDuration, SubcategoryID = scat.SubcategoryID }).ToList();
                categories.ForEach(cat => cat.Subcategories = scats);

                return (JsonResult) Util.CreateJsonResponse<CategoryBudgetAmount>("Name", "asc", null, null, categories, ResultType.Mobile, (Func<IEnumerable<CategoryBudgetAmount>, Array>)delegate(IEnumerable<CategoryBudgetAmount> rd)
                {
                    //var abc = rd.Select(c => new { });
                    return (
                        from cat in rd
                        select new
                        {
                            Name = cat.Name,
                            CategoryID = cat.CategoryID,
                            Budget = cat.Budget,
                            Display = cat.Display.GetHashCode(),
                            DisplayText = cat.Display.String(),
                            IsShared = cat.Shared,
                            Editable = cat.Editable,
                            Subcategories = cat.Subcategories.Select(scat => new
                            {
                                SubcategoryID = scat.SubcategoryID,
                                Name = scat.Name,
                                Budget = scat.Budget,
                                BudgetDuration = scat.BudgetDuration.GetHashCode(),
                                BudgetDurationText = scat.BudgetDuration.String()
                            })
                        }).ToArray();
                }
                    );
           
        }
        public JsonResult MList()
        {
            
                var categories = Repository.GetAllCategoryBudget(db, UserID);
                return Util.CreateJsonResponse<CategoryBudgetAmount>("Name", "asc", null, null, categories, ResultType.Mobile, (Func<IEnumerable<CategoryBudgetAmount>, Array>)delegate(IEnumerable<CategoryBudgetAmount> rd)
                {
                    //var abc = rd.Select(c => new { });
                    return (
                        from categ in rd
                        group categ by categ.UserName into g
                        orderby g.Key
                        select new
                        {
                            UserName = g.Key,
                            Categories = g.Select
                            (cat=>new {
                            Name = cat.Name,
                            CategoryID = cat.CategoryID,
                            Budget = cat.Budget,
                            Display = cat.Display.GetHashCode(),
                            DisplayText = cat.Display.String(),
                            IsShared = cat.Shared,
                            Editable = cat.Editable,
                            UserName = cat.UserName
                        })
                        }).ToArray();
                }
                    );
            
        }
        // GET: /Payee/
        public JsonResult JList(string sidx, string sord, int page, int rows)
        {
            
                var categories = Repository.GetCategoryBudget(db, UserID);
                //var categories = db.Categories.Where(cat => cat.UserID == UserID).AsNoTracking();

                return Util.CreateJsonResponse<CategoryBudgetAmount>(sidx, sord, page, rows, categories, (Func<IEnumerable<CategoryBudgetAmount>, Array>)delegate(IEnumerable<CategoryBudgetAmount> rd)
                {
                    //var abc = rd.Select(c => new { });
                    return (
                        from category in rd
                        select new
                        {
                            CategoryID = category.CategoryID,
                            cell = new string[] { category.CategoryID.ToString(), category.Name.ToString(),  category.Budget.ToString(), category.Shared.ToString() }
                        }).ToArray();
                }
                    );
            
        }
        // GET: /Category/JSharedList
        public JsonResult JSharedList(string sidx, string sord, int page, int rows)
        {
          
                var categories = Repository.GetSharedCategoryBudget(db, UserID);
                //var categories = db.Categories.Where(cat => cat.UserID == UserID).AsNoTracking();

                return Util.CreateJsonResponse<CategoryBudgetAmount>(sidx, sord, page, rows, categories, (Func<IEnumerable<CategoryBudgetAmount>, Array>)delegate(IEnumerable<CategoryBudgetAmount> rd)
                {
                    //var abc = rd.Select(c => new { });
                    return (
                        from category in rd
                        select new
                        {
                            CategoryID = category.CategoryID,
                            cell = new string[] { category.CategoryID.ToString(), category.Name.ToString(), category.Display.String(), category.Budget.ToString(), category.Shared.ToString() }
                        }).ToArray();
                }
                    );
            
        }
        [ValidateAntiForgeryToken]
        [HttpPost]
        public JsonResult JSharedEdit([Bind( "CategoryID,Display")] int CategoryID,  DisplaySetting Display)
        {
            if (ModelState.IsValid)
            {
                var catu = db.CategoryUsers.Where(cu => cu.UserID == UserID && cu.CategoryID == CategoryID).SingleOrDefault();
                if (catu != null)
                {
                    catu.Display = Display;
                    db.Entry(catu).State = EntityState.Modified;
                    db.SaveChanges();

                    return Json(new
                    {
                        success = true,
                        message = "success",
                        new_id = 0
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
        [HttpPost]
        public JsonResult MEdit(int CategoryID, string Name)
        {
            return Edit<JsonResult>(CategoryID, Name);
        }
        [ValidateAntiForgeryToken]
        [HttpPost]
        public JsonResult JEdit([Bind( "CategoryID,Name")] int CategoryID, string Name)
        {
            return Edit<JsonResult>(CategoryID, Name);
        }

        private T Edit<T>(int CategoryID, string Name) where T:JsonResult
        {
            if (ModelState.IsValid)
            {
                Category cat = new Category { UserID = UserID };

                if (CategoryID == 0) //add
                {
                    cat.Name = Name;
                    cat.Display = DisplaySetting.Yes;
                    db.Categories.Add(cat);
                }
                else
                {
                    cat = db.Categories.Where(c => c.UserID == UserID && c.CategoryID == CategoryID).SingleOrDefault();
                    if (cat == null)
                    {
                        return Repository.Failure<T>("Model state is invalid.");
                    }
                    else
                    {
                        cat.Name = Name;
                        db.Entry(cat).State = EntityState.Modified;
                    }
                }
                db.SaveChanges();
                return Repository.Success<T>(cat.CategoryID);
            }
            return Repository.Failure<T>("Model state is invalid.");
        }
        [HttpPost]
        public JsonResult MDelete(int CategoryID)
        {
            return Delete<JsonResult>(CategoryID);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public JsonResult JDelete(int CategoryID)
        {
            return Delete<JsonResult>(CategoryID);
        }

        private T Delete<T>(int CategoryID) where T : JsonResult
        {
           
                try
                {
                    Category cat = db.Categories.Where(ex => ex.UserID == UserID && ex.CategoryID == CategoryID).FirstOrDefault();
                    if (cat != null)
                    {
                        db.Categories.Remove(cat);
                        db.SaveChanges();
                        return Repository.Success<T>(cat.CategoryID);
                    }
                }
                catch (Exception ex)
                {
                    
                }
return Repository.DelFailure<T>();
           
        }
        public string JCategories()
        {
            string selectStr = string.Empty;
            DateTime currMonth = DateTime.Today;
            
                var categories = db.Users.Find(UserID).Categories.Where(cat=>cat.Display == DisplaySetting.Yes).ToList();
                foreach (var category in categories)
                {
                    var sc = string.Empty;
                    foreach (var subcat in category.Subcategories)
                    {
                        double amount = Repository.GetSubcatExpense(db,UserID, subcat, currMonth, true);
                        string color = "black";
                        if (subcat.Budget.BudgetDuration != RepeatPattern.None && subcat.Budget.BudgetAmount < amount)
                        {
                            color = "red";
                        }
                        else if (subcat.Budget.BudgetDuration != RepeatPattern.None && subcat.Budget.BudgetAmount > amount)
                        {
                            color = "blue";
                        }
                        if (subcat.Budget.BudgetAmount == 0)
                            color = "black";

                        sc += string.Format("<option value='{0}' title='Budget={1};Expense={2}' style='color:{3}'>{4}</option>", 
                            subcat.SubcategoryID, subcat.Budget.BudgetAmount, amount, color, subcat.Name);
                    }

                    selectStr += string.Format("<optgroup label='{0}' >{1}</optgroup>", category.Name, sc);
                }

                var sharedcats = db.CategoryUsers.Where(cu => cu.UserID == UserID && cu.Display == DisplaySetting.Yes).Select(cu => cu.Category).ToList();
                foreach (var category in sharedcats)
                {
                    var sc = string.Empty;
                    foreach (var subcat in category.Subcategories)
                    {
                        double amount = Repository.GetSubcatExpense(db,UserID, subcat, currMonth, true);
                        string color = "black";
                        if (subcat.Budget.BudgetDuration != RepeatPattern.None && subcat.Budget.BudgetAmount < amount)
                        {
                            color = "red";
                        }
                        else if (subcat.Budget.BudgetDuration != RepeatPattern.None && subcat.Budget.BudgetAmount > amount)
                        {
                            color = "blue";
                        }
                        if (subcat.Budget.BudgetAmount == 0)
                            color = "black";

                        sc += string.Format("<option value='{0}' title='Budget={1};Expense={2}' style='color:{3}'>{4}</option>",
                            subcat.SubcategoryID, subcat.Budget.BudgetAmount, amount, color, subcat.Name);
                    }

                    selectStr += string.Format("<optgroup label='{1}   [{0}]' class='special'>{2}</optgroup>", category.User.UserName , category.Name, sc);
                }

            
            

            selectStr = "<select>" + selectStr + "</select>";
            return selectStr;
            //return Json(selectStr, JsonRequestBehavior.AllowGet);
        }
        public JsonResult JCategoryValues()
        {
            Dictionary<string, string> lst = new Dictionary<string, string>();
            var cats = db.Users.Find(UserID).Categories.ToList();
            foreach (var cat in cats)
            {
                foreach (var scat in cat.Subcategories)
                {
                    lst.Add(scat.SubcategoryID.ToString(), cat.Name + " - " + scat.Name);
                }
            }

            var sharedcats = db.CategoryUsers.Where(cu => cu.UserID == UserID).Select(cu => cu.Category).ToList();
            foreach (var cat in sharedcats)
            {
                foreach (var scat in cat.Subcategories)
                {
                    lst.Add(scat.SubcategoryID.ToString(), string.Format( "{0} - {1}  [{2}]", cat.Name, scat.Name, cat.User.UserName));
                }
            }
            return Json(lst);
        }
        //public JsonResult MList()
        //{
        //    List<object> lst = new List<object>();

        //    var cats = db.Users.Find(UserID).Categories.ToList();
        //    foreach (var cat in cats)
        //    {
        //        foreach (var scat in cat.Subcategories)
        //        {
        //            lst.Add(new { SubcategoryID = scat.SubcategoryID.ToString(), Name = cat.Name + " - " + scat.Name, Shared = false } );
        //        }
        //    }

        //    var sharedcats = db.CategoryUsers.Where(cu => cu.UserID == UserID).Select(cu => cu.Category).ToList();
        //    foreach (var cat in sharedcats)
        //    {
        //        foreach (var scat in cat.Subcategories)
        //        {
        //            lst.Add(new { SubcategoryID = scat.SubcategoryID.ToString(), Name = string.Format("{0} - {1}  [{2}]", cat.Name, scat.Name, cat.User.UserName), Shared = true });
        //        }
        //    }
        //    return new JsonResult(new { categories = lst });
        //}
        public string DisplayOptions()
        {
            string selectStr = "<select>";
            selectStr += string.Format("<option value='{0}' selected='selected' title='Category will be visible in other screens'>{1}</option>", DisplaySetting.Yes.GetHashCode(), DisplaySetting.Yes.String());
            selectStr += string.Format("<option value='{0}' title='Category will be invisible in other screens'>{1}</option>", DisplaySetting.No.GetHashCode(), DisplaySetting.No.String());

            selectStr += "</select>";
            return selectStr;
        }
        
        // get: /Category/SubcatList/5
        public JsonResult SubcatList(int? id)
        {
            List<SelectListItem> sl = new List<SelectListItem>();

            if (id != null)
            {
                Category category = db.Users.Find(UserID).Categories.Find(cat => cat.CategoryID == id);
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

            return Json(sl);
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
