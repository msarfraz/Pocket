using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using Pocket.Common;
using Pocket.Models;
using Pocket.ViewModels;

namespace Pocket.Controllers
{
    public class EventController : Controller
    {
        private QDbContext db = new QDbContext();

        // GET: /Event/
        public ActionResult Index()
        {
            var events = db.Events.Include(p => p.User);
            ViewBag.Friends = new SelectList(db.Friends, "FriendID", "Name");
            return View(events.ToList());
        }

        // GET: /Event/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Event @event = db.Events.Find(id);
            if (@event == null)
            {
                return HttpNotFound();
            }
            return View(@event);
        }

        // GET: /Event/Create
        public ActionResult Create()
        {
           // ViewBag.UserID = new SelectList(db.Users, "UserID", "LoginID");
            //ViewBag.Friends = new SelectList(db.Friends, "FriendID", "Name");
            //EventFriends ef = new EventFriends();
            //ef.Friends = db.Users.Find(State.UserID).Friends;
            return View();
        }

        // POST: /Event/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Name")] Event ev)
        {
            if (ModelState.IsValid)
            {
                //@eventfr.UserID = State.UserID;
                //@eventfr.LoadFriends(db);
                //db.Events.Add(@eventfr);
                ev.UserID = State.UserID;
                db.Events.Add(ev);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

           // ViewBag.UserID = new SelectList(db.Users, "UserID", "LoginID", @eventfr.UserID);
            //ViewBag.Friends = new SelectList(db.Friends, "FriendID", "Name");
            return View(ev);
        }

        // GET: /Event/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Event @event = db.Users.Find(State.UserID).Events.Find(evt => evt.EventID == id);
            if (@event == null)
            {
                return HttpNotFound();
            }
            Mapper.CreateMap<Event, EventFriends>();
            EventFriends vmEvent = Mapper.Map<EventFriends>(@event);
            //vmEvent.AllFriends = db.Users.Find(State.UserID).Friends;
            vmEvent.SetSelectedFriends(db);

            //ViewBag.UserID = new SelectList(db.Users, "UserID", "LoginID", @event.UserID);
            //ViewBag.Friends = new SelectList(db.Users.Find(State.UserID).Friends, "FriendID", "Name", @event.Friends);


            return View(vmEvent);
        }

        // POST: /Event/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "EventID,SelectedFriendIDs,Name")] EventFriends @event)
        {
            if (ModelState.IsValid)
            {
                db.Entry(@event).State = EntityState.Modified;
                //db.Entry((Event)@event).Collection(a => a.Friends).Load();
                //@event.UserID = State.UserID;
               // @event.LoadFriends(db);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            //ViewBag.UserID = new SelectList(db.Users, "UserID", "LoginID", @event.UserID);
            //ViewBag.Friends = new SelectList(db.Users.Find(State.UserID).Friends, "FriendID", "Name", @event.Friends);
            return View(@event);
        }

        // GET: /Event/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Event @event = db.Events.Find(id);
            if (@event == null)
            {
                return HttpNotFound();
            }
            return View(@event);
        }

        // POST: /Event/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Event @event = db.Events.Find(id);
            db.Events.Remove(@event);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: /Event/Share/5
        public ActionResult Share(int id)
        {
            Event ev = db.Users.Find(State.UserID).Events.Find(e => e.EventID == id);
            if (ev == null)
            {
                return HttpNotFound();
            }
            else
            return View(ev);
        }
        public ActionResult JShare(string sidx, string sord, int page, int rows)
        {
            int id = Util.ParseInt( Request.Params["EventID"]);
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Event ev = db.Users.Find(State.UserID).Events.Find(e=>e.EventID == id);
            if (ev == null)
            {
                return HttpNotFound();
            }
            
            if (Request.IsAjaxRequest())
            {
                EventFriends ef = new EventFriends(db, State.UserID, id);

                // we have to identify later that which users are already shared and which ones are yet to be shared. we will use
                // usertype to flag the users. Not a good approach and may be changed later

                ef.EFriends.Each(u=>u.Type = UserType.Application); // used the usertype to flag the user as event user already shared

                ef.Friends.Each(u => u.Type = UserType.FB);// used the usertype to flag the user as event user already shared
                
                return Util.CreateJsonResponse<User>(sidx, sord, page, rows, ef.Friends.Union(ef.EFriends), (Func<IEnumerable<User>, Array>)delegate(IEnumerable<User> rd)
                {
                    return (
                        from u in rd
                        select new
                        {
                            FriendID = u.UserID,
                            cell = new string[] { u.UserID.ToString(), u.FirstName + " " + u.LastName, u.Type == UserType.Application ? "" : u.UserID.ToString() }
                        }).ToArray();
                }
                    );
            }
            else
                return Json(new HttpStatusCodeResult(HttpStatusCode.BadRequest));
        }

        // POST: /Event/ShareConfirmed/5/6
        [HttpPost, ActionName("Share")]
        [ValidateAntiForgeryToken]
        public ActionResult ShareConfirmed(int EventID, int UserID)
        {
            if (Request.IsAjaxRequest())
            {
                Event ev = db.Users.Find(State.UserID).Events.Find(e => e.EventID == EventID);
                Friend fr = db.Users.Find(State.UserID).Friends.Find(f => f.FriendID == UserID);
                if (ev != null && fr != null)
                {
                    EventUser evu = new EventUser();
                    evu.EventID = EventID;
                    evu.UserID = UserID;
                    db.EventUsers.Add(evu);
                    db.SaveChanges();
                    NotificationController.AddEventShareNotification(ev.EventID, ev.Name, fr.UserID, db);
                    return Json(new HttpStatusCodeResult(HttpStatusCode.OK));
                }
            }
            
            return Json(new HttpStatusCodeResult(HttpStatusCode.BadRequest));
            
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        // GET: /Event/List
        public ActionResult List()
        {
            return View();
        }
        // GET: /Event/
        public JsonResult JList(string sidx, string sord, int page, int rows)
        {
            if (Request.IsAjaxRequest())
            {
                var events = db.Users.Find(State.UserID).Events;

                return Util.CreateJsonResponse<Event>(sidx, sord, page, rows, events, (Func<IEnumerable<Event>, Array>)delegate(IEnumerable<Event> rd)
                {
                    return (
                        from ev in rd
                        select new
                        {
                            EventID = ev.EventID,
                            cell = new string[] { ev.EventID.ToString(), ev.Name, ev.EventDate.ToShortDateString(),ev.Budget.BudgetAmount.ToString(), Global.RepeatToString((RepeatPattern) ev.Budget.BudgetDuration) ,"Share With Friends", new EventFriends(db, State.UserID, ev.EventID).EFriendsString()}
                        }).ToArray();
                }
                    );
            }
            else
                return Json(new HttpStatusCodeResult(HttpStatusCode.BadRequest));

        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public JsonResult JEdit([Bind(Include = "EventID, Name, EventDate, BudgetAmount, Recursive")] int EventID, string Name, DateTime EventDate, int Recursive, int BudgetAmount)
        {
            if (Request.IsAjaxRequest() && ModelState.IsValid)
            {
                Event ev = new Event();
                if (EventID == 0) //add
                {
                    ev.UserID = State.UserID;
                    ev.Name = Name;
                    ev.ReminderDate = EventDate;
                    ev.EventDate = EventDate;
                    Budget budget = new Budget();
                    budget.BudgetAmount = BudgetAmount;
                    budget.BudgetDuration = (RepeatPattern) Recursive;
                    budget.BudgetType = BudgetType.Event;
                    budget.UserID = State.UserID;
                    db.Budgets.Add(budget);
                    ev.BudgetID = budget.BudgetID;
                    db.Events.Add(ev);
                    
                }
                else
                {
                    ev = db.Users.Find(State.UserID).Events.Find(t => t.EventID == EventID);
                    if (ev == null)
                        return Json(HttpNotFound());
                    ev.Name = Name;
                    ev.ReminderDate = EventDate;
                    ev.EventDate = EventDate;
                    ev.Budget.BudgetAmount = BudgetAmount;
                    ev.Budget.BudgetDuration = (RepeatPattern)Recursive;

                    db.Entry(ev).State = EntityState.Modified;
                }
                db.SaveChanges();
                return Json(new
                {
                    success = true,
                    message = "success",
                    new_id = ev.EventID
                });
            }
            return Json(new
            {
                success = false,
                message = "Model state is invalid.",
                new_id = 0
            });
        }
        public string JEvents()
        {
            string selectStr = "<option value=''></option>";

            if (Request.IsAjaxRequest())
            {
                var events = db.Users.Find(State.UserID).Events;
                foreach (var ev in events)
                {
                    selectStr += string.Format("<option value='{0}'>{1}</option>", ev.EventID, ev.Name);
                }
            }

            selectStr = "<select>" + selectStr + "</select>";
            return selectStr;
        }
    }
}
