using Pocket.Common;
using Pocket.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Pocket.Controllers
{
    public class VendorController : Controller
    {
        private QDbContext db = new QDbContext();

        // GET: /Vendor/
        public ActionResult Index()
        {
            return View();
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
                var vendors = db.Users.Find(State.UserID).Vendors;

                return Util.CreateJsonResponse<Vendor>(sidx, sord, page, rows, vendors, (Func<IEnumerable<Vendor>, Array>)delegate(IEnumerable<Vendor> rd)
                {
                    return (
                        from vendor in rd
                        select new
                        {
                            VendorID = vendor.VendorID,
                            cell = new string[] { vendor.VendorID.ToString(), vendor.Name }
                        }).ToArray();
                }
                    );
            }
            else
                return Json(new HttpStatusCodeResult(HttpStatusCode.BadRequest));

        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public JsonResult JEdit([Bind(Include = "VendorID,Name")] Vendor vendor)
        {
            if (Request.IsAjaxRequest() && ModelState.IsValid)
            {
                vendor.UserID = State.UserID;
                if (vendor.VendorID== 0) //add
                {
                    db.Vendors.Add(vendor);
                }
                else
                {
                    Vendor ven = db.Users.Find(State.UserID).Vendors.Find(v => v.VendorID == vendor.VendorID);
                    if (ven == null)
                        return Json(HttpNotFound());
                    ven.Name = vendor.Name;
                    
                    db.Entry(ven).State = EntityState.Modified;
                }
                db.SaveChanges();
                return Json(new
                {
                    success = true,
                    message = "success",
                    new_id = vendor.VendorID
                });
            }
            return Json(new
            {
                success = false,
                message = "Model state is invalid.",
                new_id = 0
            });
        }

        public string JVendors()
        {
            string selectStr = "<option value=''></option>";

            if (Request.IsAjaxRequest())
            {
                var vendors = db.Users.Find(State.UserID).Vendors;
                foreach (var vendor in vendors)
                {
                    selectStr += string.Format("<option value='{0}'>{1}</option>", vendor.VendorID, vendor.Name);
                }
            }

            selectStr = "<select>" + selectStr + "</select>";
            return selectStr;
        }
	}
}