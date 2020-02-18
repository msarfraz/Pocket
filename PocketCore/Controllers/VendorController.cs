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
    public class VendorController : ApplicationController
    {
        public VendorController(ApplicationDbContext
            context) : base(context)
        {
            
        }
        // GET: /Payee/Index
        public ActionResult Index()
        {
            return View();
        }
        public JsonResult MList()
        {
            return (JsonResult) GetList("Name", "asc", 1, 100, ResultType.Mobile);
        }
        // GET: /Payee/
        public JsonResult JList(string sidx, string sord, int page, int rows)
        {
            return GetList(sidx, sord, page, rows, ResultType.Web);
        }
        // GET: /Payee/
        private JsonResult GetList(string sidx, string sord, int page, int rows, ResultType rt)
        {
                var vendors = db.Users.Find(UserID).Vendors;

                return Util.CreateJsonResponse<Vendor>(sidx, sord, page, rows, vendors,rt, (Func<IEnumerable<Vendor>, Array>)delegate(IEnumerable<Vendor> rd)
                {
                    if (rt == ResultType.Web)
                    {
                        return (
                        from vendor in rd
                        select new
                        {
                            VendorID = vendor.VendorID,
                            cell = new string[] { vendor.VendorID.ToString(), vendor.Name }
                        }).ToArray();
                    }
                    else
                    {
                        return (
                        from vendor in rd
                        select new
                        {
                            VendorID = vendor.VendorID,
                            Name =  vendor.Name 
                        }).ToArray();
                    }
                }
                    );

        }
        [HttpPost]
        public JsonResult MEdit([Bind( "VendorID,Name")] int VendorID, string Name)
        {
            return Edit<JsonResult>(VendorID, Name);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public JsonResult JEdit( int VendorID, string Name)
        {
                return Edit<JsonResult>(VendorID, Name);
        }

        private T Edit<T>([Bind( "VendorID,Name")] int VendorID, string Name)where T:JsonResult
        {
                Vendor vendor = db.Vendors.Where(v => v.UserID == UserID && v.VendorID == VendorID).FirstOrDefault();
                if(vendor == null)
                {
                    vendor = new Vendor();
                    vendor.UserID = UserID;
                    vendor.Name = Name;
                    db.Vendors.Add(vendor);
                }
                else
                {
                    vendor.Name = Name;
                    db.Entry(vendor).State = EntityState.Modified;
                }
                db.SaveChanges();
                return Repository.Success<T>(vendor.VendorID);
        }

        public string JVendors()
        {
            string selectStr = "<option value=''></option>";

           
                var vendors = db.Users.Find(UserID).Vendors;
                foreach (var vendor in vendors)
                {
                    selectStr += string.Format("<option value='{0}'>{1}</option>", vendor.VendorID, vendor.Name);
                }
           
            selectStr = "<select>" + selectStr + "</select>";
            return selectStr;
        }
        [HttpPost]
        public JsonResult MDelete(int VendorID)
        {
            return Delete<JsonResult>(VendorID);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public JsonResult JDelete(int VendorID)
        {
            return Delete<JsonResult>(VendorID);
        }

        private T Delete<T>(int VendorID) where T : JsonResult
        {
                try
                {
Vendor vendor = db.Vendors.Where(v => v.UserID == UserID && v.VendorID == VendorID).FirstOrDefault();
                if (vendor != null)
                {
                    db.Vendors.Remove(vendor);
                    db.SaveChanges();
                    return Repository.Success<T>(vendor.VendorID);
                }
                }
                catch (Exception)
                {
                    
                }
                return Repository.DelFailure<T>();
        }
	}
}