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
using System.Collections;
using Pocket.ViewModels;
using Pocket.Extensions;

namespace Pocket.Controllers
{
    [Authorize]
    public class FriendController : Controller
    {
        private QDbContext db = new QDbContext();

        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
        // GET: /Friend/List
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index([Bind(Include = "Email")] string Email)
        {
            if (ModelState.IsValid)
            {
                string error = SearchUser(Email);
                if (!string.IsNullOrEmpty(error))
                {
                    ModelState.AddModelError("Email", error);
                }
            }

            return View();

        }
        // GET: /Friends/
        public JsonResult SearchFriend(string Email)
        {
            string error = SearchUser(Email);
            return Json(new
            {
                success = string.IsNullOrEmpty(error),
                message = error,
                new_id = 0
            });
        }
        private string SearchUser(string Email)
        {
            ApplicationUser u = db.Users.Where(user => user.Email == Email).SingleOrDefault();
            if (u != null)
            {
                Friend f = db.Users.Find(State.UserID).Friends.SingleOrDefault(fr => fr.FriendID == u.Id);
                if (f == null)
                {
                    f = new Friend();
                    f.UserID = State.UserID;
                    f.FriendID = u.Id;
                    f.Status = FriendStatus.Pending;
                    db.Friends.Add(f);
                    NotificationManager.AddFriendNotification(u.Id, db);
                    db.SaveChanges();
                    return string.Empty;
                }
                else
                {
                   return "Friend already added.";

                }

            }
            else
            {

                return "User not found";

            }
        }
        // GET: /Friends/
        public JsonResult MList(int? page, int? rows)
        {
            return (JsonResult)GetList("FriendID", "asc", page, rows, ResultType.Mobile);
        }

        public JsonResult JList(string sidx, string sord, int page, int rows)
        {
            return GetList(sidx, sord, page, rows, ResultType.Web);

        }
        private JsonResult GetList(string sidx, string sord, int? page, int? rows, ResultType rt)
        {
           
                string userid = State.UserID;
                var friends = db.Users.Find(userid).Friends;

                return Util.CreateJsonResponse<Friend>(sidx, sord, page, rows, friends, rt, (Func<IEnumerable<Friend>, Array>)delegate(IEnumerable<Friend> rd)
                {
                    if (rt == ResultType.Web)
                    {
                        return (
                        from friend in rd
                        select new
                        {
                            UserID = friend.FriendID,
                            cell = new string[] { friend.FriendID.ToString(), friend.UserFriend.UserName, friend.UserFriend.Email, friend.Status == FriendStatus.Pending ? "Pending Approval" : "" }
                        }).ToArray();
                    }
                    else
                    {
                        return (
                        from friend in rd
                        select new
                        {
                            UserID = friend.FriendID,
                            UserName = friend.UserFriend.UserName,
                            Email = friend.UserFriend.Email,
                            Status = friend.Status == FriendStatus.Pending ? "Pending Approval" : ""
                        }).ToArray();
                    }


                }
                    );
            
        }

        public JsonResult MPendingList(int? page, int? rows)
        {
            return (JsonResult)PendingList("UserName", "asc", page, rows, ResultType.Mobile);
        }

        // GET: /Friends/
        public JsonResult JPendingList(string sidx, string sord, int page, int rows)
        {
            return PendingList(sidx, sord, page, rows, ResultType.Web);
        }
        // GET: /Friends/
        private JsonResult PendingList(string sidx, string sord, int? page, int? rows, ResultType rt)
        {
            
                string userid = State.UserID;
                var pendingMyApproval = db.Friends.Where(f => f.FriendID == userid && f.Status == FriendStatus.Pending);
                var friends = from pa in pendingMyApproval
                              join u in db.Users on pa.UserID equals u.Id
                              select u;

                return Util.CreateJsonResponse<ApplicationUser>(sidx, sord, page, rows, friends, rt, (Func<IEnumerable<ApplicationUser>, Array>)delegate(IEnumerable<ApplicationUser> rd)
                {
                    if (rt == ResultType.Web)
                    {
                        return (
                        from friend in rd
                        select new
                        {
                            UserID = friend.Id,
                            cell = new string[] { friend.Id.ToString(), friend.UserName, friend.Email, friend.Id }
                        }).ToArray();
                    }
                    else
                    {
                        return (
                       from friend in rd
                       select new
                       {
                           UserID = friend.Id,
                           UserName = friend.UserName,
                           Email = friend.Email
                       }).ToArray();
                    }
                }
                    );
            
        }
        // GET: /Friends/JFriendResources
        public JsonResult MResourceFriends(int page, int rows, int ResourceID, SharedResourceType ResourceType)
        {
            return (JsonResult) GetResourceFriends("FriendName", "asc", page, rows,ResourceID, ResourceType, ResultType.Mobile);
        }
        // GET: /Friends/JFriendResources
        public JsonResult JResourceFriends(string sidx, string sord, int page, int rows)
        {
            int ResourceID = Util.ParseInt(Request.Params["ResourceID"]);
            SharedResourceType ResourceType = (SharedResourceType)Util.ParseInt(Request.Params["ResourceType"]);

            return GetResourceFriends(sidx, sord, page, rows,ResourceID, ResourceType, ResultType.Web);
        }
        // GET: /Friends/JFriendResources
        public JsonResult GetResourceFriends(string sidx, string sord, int page, int rows, int ResourceID, SharedResourceType ResourceType, ResultType rt)
        {
            
                string userid = State.UserID;
                
                if (ResourceID == 0 || ResourceType.GetHashCode() == 0)
                {
                    return Json(new HttpStatusCodeResult(HttpStatusCode.BadRequest));
                }
                ApplicationUser usr = db.Users.Find(userid);
                List<FriendResource> friends = usr.Friends.Where(f => f.Status == Common.FriendStatus.Approved).Select(f=>new FriendResource{ FriendID = f.FriendID, FriendName = f.UserFriend.UserName, Shared=false}).ToList();
                List<FriendResource> sfriends = null;
                
                switch (ResourceType)
	            {
		            case SharedResourceType.Event:
                        Event ev = db.Users.Find(userid).Events.Find(e=>e.EventID == ResourceID);
                         if (ev == null)
                                return Json(new HttpStatusCodeResult(HttpStatusCode.BadRequest));
                          sfriends = ev.SharedFriends.Select(sf=>new FriendResource{ FriendID = sf.User.Id, FriendName = sf.User.UserName, Shared=true}).ToList();
                     break;
                    case SharedResourceType.Account:
                        Account acc = db.Users.Find(userid).Accounts.Find(e=>e.AccountID == ResourceID);
                         if (acc == null)
                                return Json(new HttpStatusCodeResult(HttpStatusCode.BadRequest));
                          sfriends = acc.SharedFriends.Select(sf=>new FriendResource{ FriendID = sf.User.Id, FriendName = sf.User.UserName, Shared=true}).ToList();
                     break;
                    case SharedResourceType.Category:
                        Category cat = db.Users.Find(userid).Categories.Find(c=>c.CategoryID == ResourceID);
                         if (cat == null)
                                return Json(new HttpStatusCodeResult(HttpStatusCode.BadRequest));
                          sfriends = cat.SharedContacts.Select(sf=>new FriendResource{ FriendID = sf.User.Id, FriendName = sf.User.UserName, Shared=true}).ToList();
                     break;
	            }
                var selfriends = friends.Except(sfriends, new FriendResource());

                return Util.CreateJsonResponse<FriendResource>(sidx, sord, page, rows, selfriends.Union(sfriends, new FriendResource()),rt, (Func<IEnumerable<FriendResource>, Array>)delegate(IEnumerable<FriendResource> rd)
                    {
                        if (rt == ResultType.Web)
                        {
                            return (
                            from u in rd
                            select new
                            {
                                FriendID = u.FriendID,
                                cell = new string[] { u.FriendID.ToString(), u.FriendName, u.Shared.ToString() }
                            }).ToArray();
                        }
                        else
                        {
                            return (
                            from u in rd
                            select new
                            {
                                FriendID = u.FriendID,
                                FriendName = u.FriendName,
                                Shared = u.Shared
                            }).ToArray();
                        }
                    }
                        );
            
        }
        public JsonResult MShareResource(int ResourceID, SharedResourceType ResourceType, string FriendID, bool Shared)
        {
            return _ShareResource(ResourceID, ResourceType, FriendID, Shared);
        }

        // POST: /Event/ShareResource/5/6
        [HttpPost, ActionName("ShareResource")]
        [ValidateAntiForgeryToken]
        public JsonResult ShareResource(int ResourceID, SharedResourceType ResourceType, string FriendID, bool Shared)
        {
            return _ShareResource(ResourceID, ResourceType, FriendID, Shared);
        }

        public JsonResult _ShareResource(int ResourceID, SharedResourceType ResourceType, string FriendID, bool Shared)
        {
            
                string userid = State.UserID;
                Friend fr = db.Users.Find(userid).Friends.Find(f => f.FriendID == FriendID && f.Status == FriendStatus.Approved);
                if (fr == null)
                    return Json(new HttpStatusCodeResult(HttpStatusCode.BadRequest));

                switch (ResourceType)
                {
                    case SharedResourceType.Event:
                        Event ev = db.Users.Find(userid).Events.Find(e => e.EventID == ResourceID);
                        if (ev != null)
                        {
                            if (Shared)
                            {
                                EventUser evu = new EventUser();
                                evu.EventID = ResourceID;
                                evu.UserID = FriendID;
                                db.EventUsers.Add(evu);
                                NotificationManager.AddEventShareNotification(ev.EventID, ev.Name, FriendID, db, State.UserID, State.CurrentUserName);
                                db.SaveChanges();
                                return Json(new HttpStatusCodeResult(HttpStatusCode.OK));

                            }
                            else
                            {
                                EventUser eu = db.EventUsers.Where(evu => evu.EventID == ResourceID && evu.UserID == FriendID).SingleOrDefault();
                                if (eu != null)
                                {
                                    db.EventUsers.Remove(eu);
                                    NotificationManager.RemoveEventShareNotification(ev.EventID, ev.Name, FriendID, db, State.UserID, State.CurrentUserName);
                                    db.SaveChanges();
                                }
                    
                            }
                        }
                        break;
                    case SharedResourceType.Account:
                        Account acc = db.Users.Find(userid).Accounts.Find(a => a.AccountID == ResourceID);
                        if (acc != null)
                        {
                            if (Shared)
                            {
                                AccountUser au = new AccountUser();
                                au.AccountID = ResourceID;
                                au.UserID = FriendID;
                                db.AccountUsers.Add(au);
                                NotificationManager.AddAccountShareNotification(acc.AccountID, acc.Name, FriendID, db, State.UserID, State.CurrentUserName);
                                db.SaveChanges();
                                return Json(new HttpStatusCodeResult(HttpStatusCode.OK));

                            }
                            else
                            {
                                AccountUser au = db.AccountUsers.Where(accu => accu.AccountID == ResourceID && accu.UserID == FriendID).SingleOrDefault();
                                if (au != null)
                                {
                                    db.AccountUsers.Remove(au);
                                    NotificationManager.RemoveAccountShareNotification(acc.AccountID, acc.Name, FriendID, db, State.UserID, State.CurrentUserName);
                                    db.SaveChanges();
                                }
                    
                            }
                        }
                        break;
                    case SharedResourceType.Category:
                         Category cat = db.Users.Find(userid).Categories.Find(c => c.CategoryID == ResourceID);
                        if (cat != null)
                        {
                            CategoryUser cu = db.CategoryUsers.Where(catu => catu.CategoryID == ResourceID && catu.UserID == FriendID).SingleOrDefault();
                            if (Shared)
                            {
                                if (cu == null)
                                {
                                    cu = new CategoryUser();
                                    cu.CategoryID = ResourceID;
                                    cu.UserID = FriendID;
                                    cu.Display = DisplaySetting.Yes;
                                    db.CategoryUsers.Add(cu);
                                    NotificationManager.AddCategoryShareNotification(cat.CategoryID, cat.Name, FriendID, db, State.UserID, State.CurrentUserName);
                                    db.SaveChanges();
                                    return Json(new HttpStatusCodeResult(HttpStatusCode.OK));


                                }
                            }
                            else
                            {
                                if (cu != null)
                                {
                                    db.CategoryUsers.Remove(cu);
                                    NotificationManager.RemoveCategoryShareNotification(cat.CategoryID, cat.Name, FriendID, db, State.UserID, State.CurrentUserName);
                                    db.SaveChanges();
                                }
                    
                            }
                        }
                        break;
                    default:
                        break;
                }
                return Json(new HttpStatusCodeResult(HttpStatusCode.OK));
           
        }
        public JsonResult MApproveFriend(string FriendID)
        {
            return ApproveFriend<JsonResult>(FriendID);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult JApproveFriend(string FriendID)
        {
            return ApproveFriend<JsonResult>(FriendID);
        }
        private T ApproveFriend<T>(string FriendID) where T:JsonResult
        {
            string userid = State.UserID;
            if (userid == FriendID)
                return Repository.Failure<T>("A user cannot approve himself.");
            var fr = db.Friends.Where(f => f.FriendID == userid && f.UserID == FriendID && f.Status == FriendStatus.Pending).FirstOrDefault();
            if (fr != null)
            {
                fr.Status = FriendStatus.Approved;
                Friend nf = new Friend();
                nf.UserID = userid;
                nf.FriendID = FriendID;
                nf.Status = FriendStatus.Approved;
                db.Friends.Add(nf);
                db.Entry(fr).State = EntityState.Modified;
                db.SaveChanges();
                return Repository.Success<T>(0);
            }
            else
                return Global.BadRequest<T>();
        }
        public JsonResult MRejectFriend(string FriendID)
        {
            return RejectFriend<JsonResult>(FriendID);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult JRejectFriend(string FriendID)
        {
            return RejectFriend<JsonResult>(FriendID);
        }
        private T RejectFriend<T>(string FriendID) where T : JsonResult
        {
            string userid = State.UserID;
            var fr = db.Friends.Where(f => f.FriendID == userid && f.UserID == FriendID && f.Status == FriendStatus.Pending).FirstOrDefault();
            if (fr != null)
            {
                db.Friends.Remove(fr);
                db.SaveChanges();
                return Repository.Success<T>(0);
            }
            else
                return Global.BadRequest<T>();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult JResourceList(string FriendID)
        {
            string userid = State.UserID;
            var fr = db.Friends.Where(f => f.FriendID == userid && f.UserID == FriendID && f.Status == FriendStatus.Pending).FirstOrDefault();
            if (fr != null)
            {
                fr.Status = FriendStatus.Approved;
                Friend nf = new Friend();
                nf.UserID = userid;
                nf.FriendID = FriendID;
                nf.Status = FriendStatus.Approved;
                db.Friends.Add(nf);
                db.Entry(fr).State = EntityState.Modified;
                db.SaveChanges();
                return Json(new HttpStatusCodeResult(HttpStatusCode.OK));
            }
            else
                return Json(new HttpStatusCodeResult(HttpStatusCode.BadRequest));
        }
    }
}
