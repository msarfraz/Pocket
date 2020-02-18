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

namespace Pocket.Controllers
{
    [Authorize]
    public class IncomeSourceController : ApplicationController
    {
        public IncomeSourceController(ApplicationDbContext
            context) : base(context)
        {
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

            
                var sources = db.Users.Find(UserID).IncomeSources;
                foreach (var source in sources)
                {
                    selectStr += string.Format("<option value='{0}'>{1}</option>", source.SourceID, source.Name);
                }
            

            selectStr = "<select>" + selectStr + "</select>";
            return selectStr;
            //return Json(selectStr, JsonRequestBehavior.AllowGet);
        }

        // GET: /Payee/Index
        public ActionResult Index()
        {
            return View(db.Users.Find(UserID).IncomeSources);
        }
        
        public JsonResult MList()
        {
            return (JsonResult) GetList("Name", "asc", 1, 100, ResultType.Mobile);
        }
        public JsonResult JList(string sidx, string sord, int page, int rows)
        {
            return GetList(sidx, sord, page, rows, ResultType.Web);
        }
        private JsonResult GetList(string sidx, string sord, int page, int rows, ResultType rt)
        {
                var sources = db.Users.Find(UserID).IncomeSources;

                return Util.CreateJsonResponse<IncomeSource>(sidx, sord, page, rows, sources,rt, (Func<IEnumerable<IncomeSource>, Array>)delegate(IEnumerable<IncomeSource> rd)
                {
                    if (rt == ResultType.Web)
                    {
                        return (
                        from source in rd
                        select new
                        {
                            SourceID = source.SourceID,
                            cell = new string[] { source.SourceID.ToString(), source.Name }
                        }).ToArray();
                    }
                    else
                    {
                        return (
                        from source in rd
                        select new { SourceID = source.SourceID, Name = source.Name }
                        ).ToArray();
                    }
                }
                    );

        }
        [HttpPost]
        public JsonResult MEdit([Bind( "SourceID,Name")] int SourceID, string Name)
        {
            return Edit<JsonResult>(SourceID, Name);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public JsonResult JEdit([Bind( "SourceID,Name")] int SourceID, string Name)
        {
            return Edit<JsonResult>(SourceID, Name);
        }

        private T Edit<T>([Bind( "SourceID,Name")] int SourceID, string Name)where T:JsonResult
        {
            if (ModelState.IsValid)
            {
                IncomeSource source = db.IncomeSources.Where(ics => ics.SourceID == SourceID && ics.UserID == UserID).FirstOrDefault();
                if (source == null) //add
                {
                    source = new IncomeSource();
                    source.UserID = UserID;
                    source.Name = Name;
                    db.IncomeSources.Add(source);
                }
                else
                {
                    source.Name = Name;
                    db.Entry(source).State = EntityState.Modified;
                }
                db.SaveChanges();
                return Repository.Success<T>(source.SourceID);
               
            }
            return Repository.Failure<T>();
        }
        [HttpPost]
        public JsonResult MDelete(int SourceID)
        {
            return Delete<JsonResult>(SourceID);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public JsonResult JDelete(int SourceID)
        {
            return Delete<JsonResult>(SourceID);
        }

        private T Delete<T>(int SourceID) where T : JsonResult
        {
                try
                {
                    IncomeSource source = db.IncomeSources.Where(s => s.UserID == UserID && s.SourceID == SourceID).FirstOrDefault();
                    if (source != null)
                    {
                        db.IncomeSources.Remove(source);
                        db.SaveChanges();
                        return Repository.Success<T>(source.SourceID);
                    }
                }
                catch (Exception ex)
                {
                    
                }
                return Repository.DelFailure<T>();
        }
    }
}
