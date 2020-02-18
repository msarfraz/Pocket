using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Pocket.Common;
using Pocket.Models;
using Pocket.ViewModels;

namespace Pocket.Controllers
{
    public class HomeController : ApplicationController
    {
        public HomeController(ApplicationDbContext
            context) : base(context)
        {
        }
        //
        // GET: /Home/
        public ActionResult Index()
        {
             return View();
        }
        [Authorize]
        // GET: /Home/Default
        public ActionResult Default()
        {
            var accounts = Global.getAllUserAccountsWithCurrentAmount(db, UserID);

            return View(accounts.OrderByDescending(a=>a.CurrentAmount));
        }
        // GET: /Home/
        public ActionResult CustRequest()
        {
            return View();
        }
        // GET: /Home/
        public ActionResult Support()
        {
            return View();
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        // GET: /Home/CustomerRequest
        public IActionResult CustomerRequest(string Name, string Email, string Title, string Type, string Body)
        {
            
                CustomerRequest cr = new CustomerRequest();
                if (User.Identity.IsAuthenticated)
                {
                    cr.UserID = User.FindFirstValue(ClaimTypes.NameIdentifier); 
                    Name = User.Identity.Name;
                    //Email = Email;
                }
                cr.Name = Name;
                cr.Email = Email;
                cr.Title = Title;
                cr.RequestType = (Common.CustomerRequestType) Util.ParseInt( Type, 1);
                cr.Body = Body;
                cr.CreatedDate = DateTime.Now;
                db.CustomerRequests.Add(cr);
                db.SaveChanges();
                return Ok(); 
            
        }

        public virtual ActionResult Feed(string id)
        {
            /*
            ScheduleController sc = new ScheduleController();
            sc.ProcessSchedules(db);
            double pjobs = db.ScheduleJobs.Where(sj=>sj.CreatedDate == DateTime.Today).Select(p=>p.JobsProcessed).DefaultIfEmpty(0).Sum();
            double users = db.Users.Count();

            var items = new List<SyndicationItem>();


            var helper = new UrlHelper(this.Request.RequestContext);
            var url = helper.Action("Index", "Home", new { }, Request.IsSecureConnection ? "https" : "http");

            string feedDescription = string.Format( "Number of jobs processed Today '{0}': {1}." , DateTime.Today, pjobs) ;
            var feedPackageItem = new SyndicationItem("Schedule Status", feedDescription, new Uri(url));
            feedPackageItem.PublishDate = DateTime.Now;
            items.Add(feedPackageItem);

            feedDescription = string.Format("Number of expenses: {0}", users);
            feedPackageItem = new SyndicationItem("Expense Status", feedDescription, new Uri(url));
            feedPackageItem.PublishDate = DateTime.Now;
            items.Add(feedPackageItem);

            return new RssResult("Xpert Budget Feed", items);
            */
            return View();
        }
        // GET: /Home/Budget
        [Authorize]
        public ActionResult Budget(bool? ConstantBudget)
        {
            int Month = DateTime.Today.Month;
            int Year = DateTime.Today.Year;

            List<CategoryBudgetAmount> catsba = Repository.GetCategoryBudgetAmount(db, UserID, Month, Year, ConstantBudget.HasValue?ConstantBudget.Value:false);
            List<EventBudgetAmount> evsba = Global.getEventMonthlyBudgetAmount(db,UserID, Month, Year, ConstantBudget.HasValue ? ConstantBudget.Value : false);

            Tuple<IEnumerable<CategoryBudgetAmount>, IEnumerable<EventBudgetAmount>,bool> model = new Tuple<IEnumerable<CategoryBudgetAmount>, IEnumerable<EventBudgetAmount>, bool>(catsba, evsba, ConstantBudget.HasValue ? ConstantBudget.Value : false);

            // Get Amount Data

            return View(model);
        }


        public JsonResult MBudgetDetails(bool? ConstantBudget)
        {
            int Month = DateTime.Today.Month;
            int Year = DateTime.Today.Year;

            List<CategoryBudgetAmount> catsba = Repository.GetCategoryBudgetAmount(db,UserID, Month, Year, ConstantBudget.HasValue ? ConstantBudget.Value : false);
            List<EventBudgetAmount> evsba = Global.getEventMonthlyBudgetAmount(db,UserID, Month, Year, ConstantBudget.HasValue ? ConstantBudget.Value : false);

            // Get Amount Data

           return Util.Package<JsonResult>(new
            {
                Categories = catsba.Select(c => new { 
                    Name = c.Name,
                    Budget = c.Budget,
                    Amount = c.Amount,
                    Percentage = c.BudgetPercentage,
                    Subcategories = c.Subcategories.Select(s=> new
                    {
                        Name = s.Name,
                        Budget = s.Budget,
                        Amount = s.Amount,
                        Percentage = s.BudgetPercentage
                    })
                }),
                AllEvents = new {
                    Name = "Events",
                    Budget = Math.Round( evsba.Select(e=>e.Budget).DefaultIfEmpty(0).Sum(), 2),
                    Amount = Math.Round( evsba.Select(e => e.Amount).DefaultIfEmpty(0).Sum(), 2),
                    Percentage = evsba.Select(e => e.BudgetPercentage).DefaultIfEmpty(0).Sum() / Math.Max(1, evsba.Count) , // avoid divide by zero
                    Events = evsba.Select(e => new
                    {
                        Name = e.Name,
                        Budget = Math.Round( e.Budget, 2),
                        Amount = Math.Round(e.Amount, 2),
                        Percentage = e.BudgetPercentage
                    })
                },
                
            });
        }
        
	}
}