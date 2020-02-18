using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using AutoMapper;
using Pocket.Common;
using Pocket.Models;
using Pocket.ViewModels;
using Pocket.Extensions;

namespace Pocket.Controllers
{
    [Authorize]
    public class EventController : Controller
    {
        private QDbContext db = new QDbContext();

       
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
            
            
                EventFriends ef = new EventFriends(db, State.UserID, id);

                // we have to identify later that which users are already shared and which ones are yet to be shared. we will use
                // usertype to flag the users. Not a good approach and may be changed later

                ef.EFriends.Each(u=>u.Type = UserType.Application); // used the usertype to flag the user as event user already shared

                ef.Friends.Each(u => u.Type = UserType.FB);// used the usertype to flag the user as event user already shared

                return Util.CreateJsonResponse<ApplicationUser>(sidx, sord, page, rows, ef.Friends.Union(ef.EFriends), (Func<IEnumerable<ApplicationUser>, Array>)delegate(IEnumerable<ApplicationUser> rd)
                {
                    return (
                        from u in rd
                        select new
                        {
                            FriendID = u.Id,
                            cell = new string[] { u.Id.ToString(), u.UserName, u.Type == UserType.Application ? "" : u.Id.ToString() }
                        }).ToArray();
                }
                    );
            
        }

        // POST: /Event/ShareConfirmed/5/6
        [HttpPost, ActionName("Share")]
        [ValidateAntiForgeryToken]
        public ActionResult ShareConfirmed(int EventID, string UserID)
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
                    NotificationManager.AddEventShareNotification(ev.EventID, ev.Name, UserID, db, State.UserID, State.CurrentUserName);
                    return Json(new HttpStatusCodeResult(HttpStatusCode.OK));
                }
                return Global.BadRequest<JsonResult>();
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
        public ActionResult Index()
        {
            return View();
        }
        public JsonResult MRecord(int EventID)
        {
            var events = db.Events.Where(ev => ev.UserID == State.UserID && ev.EventID == EventID).ToList();
            if(events.Count < 1)
                events = db.EventUsers.Where(eu => eu.UserID == State.UserID && eu.EventID == EventID).Select(eu => eu.Event).ToList();
            if (events.Count < 1)
            {
                return Global.BadRequest<JsonResult>();
            }
            foreach (var ev in events)
            {
                ev.Amount = db.Expenses.Where(e => e.EventID == ev.EventID).Select(ex => ex.Amount).DefaultIfEmpty(0).Sum();
            }
            return Util.Package<JsonResult>(events.Select(ev => new {
                EventID = ev.EventID,
                Amount = ev.Amount,
                BudgetAmount = ev.Budget.BudgetAmount,
                BudgetDuration = ev.Budget.BudgetDuration.GetHashCode(),
                BudgetDurationText = ev.Budget.BudgetDuration.String(),
                Budgeted = ev.Budgeted.GetHashCode(),
                BudgetedText = ev.Budgeted.String(),
                EventDate = ev.EventDate.ToUTCDateString(),
                EventStatus = ev.EventStatus.String(),
                Name = ev.Name,
                UserName = ev.User.UserName,
                Editable = ev.UserID == State.UserID
            }));
        }
        public JsonResult MList(int? page, int? rows)
        {
           
                var mevents = db.Events.Where(ev => ev.UserID == State.UserID).ToList();
                var fevents = db.EventUsers.Where(eu => eu.UserID == State.UserID).Select(eu => eu.Event).ToList();
                var events = mevents.Union(fevents);
                
                return Util.CreateJsonResponse<Event>("EventDate", "desc", page, rows, events, ResultType.Mobile, (Func<IEnumerable<Event>, Array>)delegate(IEnumerable<Event> rd)
                {
                    foreach (var ev in rd)
                    {
                        ev.Amount = db.Expenses.Where(e => e.EventID == ev.EventID).Select(ex => ex.Amount).DefaultIfEmpty(0).Sum();
                    }
                    return (
                        from ev in rd
                        select new
                        {
                            EventID = ev.EventID,
                            Amount = ev.Amount,
                            BudgetAmount = ev.Budget.BudgetAmount,
                            BudgetDuration = ev.Budget.BudgetDuration.GetHashCode(),
                            BudgetDurationText = ev.Budget.BudgetDuration.String(),

                            EventDate = ev.EventDate.ToDateDisplayString(),
                            EventStatus = ev.EventStatus.String(),
                            Name = ev.Name,
                            UserName = ev.UserID == State.UserID? "":ev.User.UserName,
                            Editable = ev.UserID == State.UserID

                        }).ToArray();

                }
                    );
            
        }
        public JsonResult MUpcomingList()
        {
            if (true)
            {
                var mevents = db.Events.Where(ev => ev.UserID == State.UserID && ev.EventDate >= DateTime.Today).ToList();
                var fevents = db.EventUsers.Where(eu => eu.UserID == State.UserID && eu.Event.EventDate >= DateTime.Today).Select(eu => eu.Event).ToList();
                var events = mevents.Union(fevents).ToList();

                return (JsonResult)Util.CreateJsonResponse<Event>("EventDate", "asc", 1, 5, events, ResultType.Web, (Func<IEnumerable<Event>, Array>)delegate(IEnumerable<Event> rd)
                {
                    return (
                        from ev in rd
                        select new
                        {
                            EventID = ev.EventID,
                            Name = ev.Name,
                            EventDate = ev.EventDate.ToDateDisplayString(),
                            Editable = ev.UserID == State.UserID

                        }).ToArray();
                }
                    );
            }
            else
                return Global.BadRequest<JsonResult>();
        }
        public JsonResult JList(string sidx, string sord, int page, int rows)
        {
            return GetList(sidx, sord, page, rows, ResultType.Web);
        }
        private JsonResult GetList(string sidx, string sord, int page, int rows, ResultType rt)
        {
            
                var events = db.Users.Find(State.UserID).Events;
                foreach (var ev in events)
                {
                    ev.Amount = db.Expenses.Where(e => e.EventID == ev.EventID).Select(ex => ex.Amount).DefaultIfEmpty(0).Sum();
                }
                return Util.CreateJsonResponse<Event>(sidx, sord, page, rows, events,rt, (Func<IEnumerable<Event>, Array>)delegate(IEnumerable<Event> rd)
                {
                    if (rt == ResultType.Web)
                    {
                        return (
                        from ev in rd
                        select new
                        {
                            EventID = ev.EventID,
                            cell = new string[] { ev.EventID.ToString(), ev.Name, ev.EventDate.ToDateString(), ev.Budget.BudgetAmount.ToString(), ev.Budget.BudgetDuration.String(), ev.Budgeted.String(), ev.Amount.ToString(), ev.EventStatus.String(), (ev.SharedFriends.Count > 0).ToString() }
                        }).ToArray();
                    }
                    else
                    {
                        return (
                        from ev in rd
                        select new
                        {
                            EventID = ev.EventID,
                            Amount = ev.Amount,
                            BudgetAmount = ev.Budget.BudgetAmount,
                            BudgetDuration = ev.Budget.BudgetDuration.GetHashCode(),
                            BudgetDurationText = ev.Budget.BudgetDuration.String(),
                            EventDate = ev.EventDate.ToDateString(),
                            EventStatus = ev.EventStatus.String(),
                            Name = ev.Name,
                            UserName = ev.User.UserName,
                            Editable = ev.UserID == State.UserID
                        }).ToArray();
                    }
                    
                }
                    );
            
        }
        public JsonResult MOtherList(int page, int rows)
        {
            return (JsonResult)GetOtherList("EventID", "desc", page, rows, ResultType.Mobile);
        }
        public JsonResult JOtherList(string sidx, string sord, int page, int rows)
        {
            return GetOtherList(sidx, sord, page, rows, ResultType.Web);
        }

        private JsonResult GetOtherList(string sidx, string sord, int page, int rows, ResultType rt)
        {
            if (Request.IsAjaxRequest())
            {
                var events = db.Users.Find(State.UserID).OtherEvents.Select(ev=>ev.Event);
                foreach (var ev in events)
                {
                    ev.Amount = db.Expenses.Where(e => e.EventID == ev.EventID).Select(ex => ex.Amount).DefaultIfEmpty(0).Sum();
                }
                return Util.CreateJsonResponse<Event>(sidx, sord, page, rows, events, (Func<IEnumerable<Event>, Array>)delegate(IEnumerable<Event> rd)
                {
                    if (rt == ResultType.Web)
                    {
                        return (
                        from ev in rd
                        select new
                        {
                            EventID = ev.EventID,
                            cell = new string[] { ev.EventID.ToString(), ev.Name, ev.EventDate.ToDateString(), ev.Budget.BudgetAmount.ToString(), ev.Amount.ToString(), ev.Budget.BudgetDuration.String(), ev.EventStatus.String(), ev.Budgeted.String() }
                        }).ToArray();
                    }
                    else
                    {
                        return (
                       from ev in rd
                       select new
                       {
                           EventID = ev.EventID,
                           Amount = ev.Amount,
                           BudgetAmount = ev.Budget.BudgetAmount,
                           BudgetDuration = ev.Budget.BudgetDuration.GetHashCode(),
                           BudgetDurationText = ev.Budget.BudgetDuration.String(),
                           EventDate = ev.EventDate.ToUTCDateString(),
                           EventStatus = ev.EventStatus.String(),
                           Name = ev.Name,
                           UserName = ev.User.UserName,
                           IsMyEvent = ev.UserID == State.UserID
                       }).ToArray();
                    }
                }
                    );
            }
            else
                return Json(new HttpStatusCodeResult(HttpStatusCode.BadRequest));

        }
        [HttpPost]
        public JsonResult MEdit(int EventID, string Name, DateTime EventDate, int BudgetDuration, double BudgetAmount, YesNoOptions Budgeted)
        {
            return Edit<JsonResult>(EventID, Name, EventDate, BudgetDuration, BudgetAmount, Budgeted);
        }
        [ValidateAntiForgeryToken]
        [HttpPost]
        public JsonResult JEdit(int EventID, string Name, DateTime EventDate, int BudgetDuration, double BudgetAmount, YesNoOptions Budgeted)
        {
            return Edit<JsonResult>(EventID, Name, EventDate, BudgetDuration, BudgetAmount, Budgeted);
        }

        private T Edit<T>(int EventID, string Name, DateTime EventDate, int BudgetDuration, double BudgetAmount, YesNoOptions Budgeted) where T : JsonResult
        {
            if (ModelState.IsValid)
            {
                Event ev = new Event();
                if (EventID == 0) //add
                {
                    ev.UserID = State.UserID;
                    ev.Name = Name;
                    ev.ReminderDate = EventDate;
                    ev.EventDate = EventDate;
                    ev.CreatedDate = DateTime.Now;
                    ev.EventStatus = EventStatus.Active;
                    ev.Budgeted = Budgeted;

                    Budget budget = new Budget();
                    budget.BudgetAmount = BudgetAmount;
                    budget.BudgetDuration = (RepeatPattern)BudgetDuration;
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
                        return Global.BadRequest<T>();
                    ev.Name = Name;
                    ev.ReminderDate = EventDate;
                    ev.EventDate = EventDate;
                    ev.Budgeted = Budgeted;

                    ev.Budget.BudgetAmount = BudgetAmount;
                    ev.Budget.BudgetDuration = (RepeatPattern)BudgetDuration;

                    db.Entry(ev).State = EntityState.Modified;
                }
                db.SaveChanges();
                return Repository.Success<T>(ev.EventID);
                
            }
            return Repository.Failure<T>("Model state is invalid.");
        }
        [HttpPost]
        public JsonResult MDelete(int EventID)
        {
            return Delete<JsonResult>(EventID);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public JsonResult JDelete(int EventID)
        {
            return Delete<JsonResult>(EventID);
        }

        private T Delete<T>(int EventID) where T : JsonResult
        {
            
                try
                {
                    Event ev = db.Events.Where(ex => ex.UserID == State.UserID && ex.EventID == EventID).FirstOrDefault();
                    if (ev != null)
                    {
                        db.Events.Remove(ev);
                        db.SaveChanges();
                        return Repository.Success<T>(ev.EventID);
                    }
                }
                catch (Exception ex)
                {
                    
                }

            return Repository.DelFailure<T>();
        }
        public string JYesNoOptions()
        {
            string selectStr = string.Empty;
            selectStr += string.Format("<option value='{0}'>{1}</option>", YesNoOptions.No.GetHashCode(), YesNoOptions.No.String());
            selectStr += string.Format("<option value='{0}'>{1}</option>", YesNoOptions.Yes.GetHashCode(), YesNoOptions.Yes.String());
            selectStr = "<select>" + selectStr + "</select>";
            return selectStr;
        }
        public string JEvents()
        {
            string selectStr = "<option value=''></option>";

            if (Request.IsAjaxRequest())
            {
                var events = db.Events.Where(ev=>ev.UserID == State.UserID);
                    selectStr += "<optgroup label='My Events' />";
                foreach (var ev in events)
                {
                    selectStr += string.Format("<option value='{0}'>{1}</option>", ev.EventID, ev.Name);
                }
                var fevents = db.EventUsers.Where(eu => eu.UserID == State.UserID).Select(eu => eu.Event).ToList();

                selectStr += "<optgroup label='Other Events' />";
                foreach (var ev in fevents)
                {
                    selectStr += string.Format("<option value='{0}'>{1}</option>", ev.EventID, ev.Name);
                }
            }

            selectStr = "<select>" + selectStr + "</select>";
            return selectStr;
        }

        public JsonResult MEvents()
        {
           
                GroupList<MutableTuple<int, string>> groups = new GroupList<MutableTuple<int, string>>();

                var mevents = db.Events.Where(ev => ev.UserID == State.UserID && ev.EventStatus == EventStatus.Active).ToList();
                var fevents = db.EventUsers.Where(eu => eu.UserID == State.UserID && eu.Event.EventStatus == EventStatus.Active).Select(eu => eu.Event).ToList();
                var events = mevents.Union(fevents);

                foreach (var ev in events)
                {
                    groups.AddGroupDetails(ev.UserID == State.UserID ? "" : ev.User.UserName, new MutableTuple<int, string>(ev.EventID, ev.Name));
                }

                return Util.Package<JsonResult>(groups.Groups.Select(g =>
                    new
                    {
                        UserName = g.Name,
                        Events = g.GroupDetails.Select(mt => new { EventID = mt.Item1, Name = mt.Item2 })
                    }), events.Count());


           
        }
    }
}
