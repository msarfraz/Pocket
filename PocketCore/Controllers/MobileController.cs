using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pocket.Common;
using Pocket.Models;

namespace Pocket.Controllers
{

    public class Mobile2Controller : ApplicationController
    {
        public Mobile2Controller(ApplicationDbContext db) : base(db) { }
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Contact()
        {
            return View();
        }

        public ActionResult Payee()
        {
            return View();
        }
        public ActionResult PayeeList()
        {
            return View();
        }
        public ActionResult Vendor()
        {
            return View();
        }
        public ActionResult VendorList()
        {
            return View();
        }
        public ActionResult IncomeSource()
        {
            return View();
        }
        public ActionResult IncomeSourceList()
        {
            return View();
        }
        public ActionResult Target()
        {
            return View();
        }
        public ActionResult TargetList()
        {
            return View();
        }
        public ActionResult Account()
        {
            return View();
        }
        public ActionResult AccountList()
        {
            return View();
        }
        public ActionResult Income()
        {
            return View();
        }
        public ActionResult IncomeList()
        {
            return View();
        }
        public ActionResult Expense()
        {
            return View();
        }
        public ActionResult ExpenseList()
        {
            return View();
        }
        public ActionResult Category()
        {
            return View();
        }
        public ActionResult CategoryList()
        {
            return View();
        }
        public ActionResult Subcategory()
        {
            return View();
        }
        public ActionResult EventList()
        {
            return View();
        }
        public ActionResult Event()
        {
            return View();
        }
        public ActionResult FriendList()
        {
            return View();
        }
        public ActionResult ResourceFriendList()
        {
            return View();
        }
        public ActionResult Friend()
        {
            return View();
        }
        public ActionResult TransferList()
        {
            return View();
        }
        public ActionResult Transfer()
        {
            return View();
        }
        public ActionResult SavingList()
        {
            return View();
        }
        public ActionResult Saving()
        {
            return View();
        }
        public ActionResult ReportList()
        {
            return View();
        }
        public ActionResult ReportCategory()
        {
            return View();
        }
        public ActionResult ChartCat()
        {
            return View();
        }
        public ActionResult Home()
        {
            return View();
        }
        public ActionResult Budget()
        {
            return View();
        }
        public ActionResult ExCommentList()
        {
            return View();
        }
        public ActionResult NotificationList()
        {
            return View();
        }
        [HttpGet]
        [Authorize]
        public JsonResult GetSecureMethod()
        {
            return Json(new { UserID = this.User.Identity.Name });
        }
        [HttpPost]
        [Authorize]
        public JsonResult PostSecureMethod()
        {
            return Json(new { UserID = UserID });
        }
    }
}