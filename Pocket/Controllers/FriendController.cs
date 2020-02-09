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

namespace Pocket.Controllers
{
    public class FriendController : Controller
    {
        private QDbContext db = new QDbContext();

        // GET: /Friend/
        public ActionResult Index()
        {
            var friends = db.Users.Find(State.UserID).Friends;
            return View(friends.ToList());
        }

        // GET: /Friend/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Friend friend = db.Friends.Find(id);
            if (friend == null)
            {
                return HttpNotFound();
            }
            return View(friend);
        }

        // GET: /Friend/Create
        public ActionResult Create()
        {
            ViewBag.FriendID = new SelectList(db.Users, "UserID", "LoginID");
            return View();
        }

        // POST: /Friend/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="Email")] string Email)
        {
            if (ModelState.IsValid)
            {
                User u = db.Users.SingleOrDefault(user => user.Email == Email);
                if (u != null)
                {
                    Friend f = new Friend();
                    f.UserID = State.UserID;
                    f.FriendID = u.UserID;
                    f.Status = FriendStatus.Pending;
                    db.Friends.Add(f);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("Email", "not found");
                    return View();
                }
                
            }

            return View();
           
        }

        // GET: /Friend/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Friend friend = db.Friends.Find(id);
            if (friend == null)
            {
                return HttpNotFound();
            }
            ViewBag.UserID = new SelectList(db.Users, "UserID", "LoginID", friend.UserID);
            return View(friend);
        }

        // POST: /Friend/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="FriendID,UserID,Name")] Friend friend)
        {
            if (ModelState.IsValid)
            {
                db.Entry(friend).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.UserID = new SelectList(db.Users, "UserID", "LoginID", friend.UserID);
            return View(friend);
        }

        // GET: /Friend/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Friend friend = db.Friends.Find(id);
            if (friend == null)
            {
                return HttpNotFound();
            }
            return View(friend);
        }

        // POST: /Friend/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Friend friend = db.Friends.Find(id);
            db.Friends.Remove(friend);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // POST: /Friend/Search/abc@yahoo.com
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Search([Bind(Include = "Email")] string email)
        {
            if (ModelState.IsValid)
            {
                User u = db.Users.SingleOrDefault(user => user.Email == email);
                if (u != null)
                {
                    Friend f = new Friend();
                    f.UserID = State.UserID;
                    f.FriendID = u.UserID;
                    f.Status = FriendStatus.Pending;
                    db.Friends.Add(new Friend());
                    db.SaveChanges();
                }
               

            }
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
        // GET: /Friend/List
        public ActionResult List()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult List([Bind(Include = "Email")] string Email)
        {
            if (ModelState.IsValid)
            {
                User u = db.Users.SingleOrDefault(user => user.Email == Email);
                if (u != null)
                {
                    Friend f = db.Users.Find(State.UserID).Friends.SingleOrDefault(fr => fr.FriendID == u.UserID);
                    if (f == null)
                    {
                        f = new Friend();
                        f.UserID = State.UserID;
                        f.FriendID = u.UserID;
                        f.Status = FriendStatus.Pending;
                        db.Friends.Add(f);
                        db.SaveChanges();
                    }
                    else
                    {
                        ModelState.AddModelError("Email", "Friend already added.");
                        
                    }
                        
                }
                else
                {
                    ModelState.AddModelError("Email", "User not found");
                    
                }

            }

            return View();

        }

        // GET: /Friends/
        public JsonResult JList(string sidx, string sord, int page, int rows)
        {
            if (Request.IsAjaxRequest())
            {
                int userid = State.UserID;
                var friends = db.Users.Find(userid).Friends;

                //var friends = db.Users.Find(State.UserID).Friends.Select(f=>f.UserFriend);

                return Util.CreateJsonResponse<Friend>(sidx, sord, page, rows, friends, (Func<IEnumerable<Friend>, Array>)delegate(IEnumerable<Friend> rd)
                {
                    
                        return (
                            from friend in rd
                            select new
                            {
                                UserID = friend.FriendID,
                                cell = new string[] { friend.FriendID.ToString(), friend.UserFriend.FirstName + " " + friend.UserFriend.LastName, friend.UserFriend.Email, friend.Status == FriendStatus.Pending ? "Pending Approval": "" }
                            }).ToArray();
                    
                }
                    );
            }
            else
                return Json(new HttpStatusCodeResult(HttpStatusCode.BadRequest));

        }
        // GET: /Friends/
        public JsonResult JPendingList(string sidx, string sord, int page, int rows)
        {
            if (Request.IsAjaxRequest())
            {
                int userid = State.UserID;
                var pendingMyApproval = db.Friends.Where(f => f.FriendID == userid && f.Status == FriendStatus.Pending);
                var friends = from pa in pendingMyApproval
                              join u in db.Users on pa.UserID equals u.UserID
                              select u;

                return Util.CreateJsonResponse<User>(sidx, sord, page, rows, friends, (Func<IEnumerable<User>, Array>)delegate(IEnumerable<User> rd)
                {
                    return (
                        from friend in rd
                        select new
                        {
                            UserID = friend.UserID,
                            cell = new string[] { friend.UserID.ToString(), friend.FirstName + " " + friend.LastName, friend.Email, friend.UserID.ToString() }
                        }).ToArray();
                }
                    );
            }
            else
                return Json(new HttpStatusCodeResult(HttpStatusCode.BadRequest));

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult JApproveFriend(int FriendID)
        {
            int userid = State.UserID;
            var fr = db.Friends.Where(f => f.FriendID == userid && f.UserID == FriendID && f.Status == FriendStatus.Pending).FirstOrDefault();
            if(fr != null)
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
