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
using Microsoft.AspNet.Identity;
using Pocket.Extensions;

namespace Pocket.Controllers
{
    [Authorize]
    public class QAccountController : Controller
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

        // GET: /Account/List
        public ActionResult Index()
        {
            return View();
        }
        public JsonResult MRecord(int AccountID)
        {
            var accounts = db.Accounts.Where(acc => acc.UserID == State.UserID && acc.AccountID == AccountID).ToList();
            if (accounts.Count < 1)
            {
                accounts = db.AccountUsers.Where(au => au.UserID == State.UserID && au.AccountID == AccountID).Select(au => au.Account).ToList();
            }
            if (accounts.Count < 1)
            {
                return Global.BadRequest<JsonResult>();
            }
            accounts = Global.SetAccountCurrentAmount(db, accounts.ToList()).ToList();
            return Util.Package<JsonResult>(accounts.Select(acc => new
                       {
                           AccountID = acc.AccountID,
                           Name = acc.Name,
                           InitialAmount = acc.InitialAmount,
                           CurrentAmount = acc.CurrentAmount,
                           AccountTypeText = acc.AccountType.String(),
                           AccountType = acc.AccountType.GetHashCode(),
                           UserName = acc.UserID == State.UserID ? "": acc.User.UserName,
                           Editable = acc.UserID == State.UserID
                       }), accounts.Count());
        }
        public JsonResult MList()
        {
                IEnumerable<Account> maccounts = db.Accounts.Where(acc => acc.UserID == State.UserID).ToList();
                var faccounts = db.AccountUsers.Where(au => au.UserID == State.UserID).Select(au => au.Account).ToList();
                var accounts = maccounts.Union(faccounts).ToList();

                accounts = Global.SetAccountCurrentAmount(db, accounts).ToList();

                return Util.Package<JsonResult>(accounts.GroupBy(acc => new { id = acc.UserID, name = acc.User.UserName }).Select(g => new
                {
                    UserName = g.Key.id == State.UserID ? "" : g.Key.name,
                    Accounts = g.Select(acc => new
                    {
                        AccountID = acc.AccountID,
                        Name = acc.Name,
                        InitialAmount = acc.InitialAmount,
                        CurrentAmount = acc.CurrentAmount,
                        AccountTypeText = acc.AccountType.String(),
                        AccountType = acc.AccountType.GetHashCode(),
                        UserName = acc.UserID == State.UserID ? "" : acc.User.UserName,
                        Editable = acc.UserID == State.UserID
                    })

                }));

        }
        public JsonResult JList(string sidx, string sord, int page, int rows)
        {
            return GetList(sidx, sord, page, rows, ResultType.Web);
        }
        // GET: /Account/
        private JsonResult GetList(string sidx, string sord, int? page, int? rows, ResultType rt)
        {
                IEnumerable<Account> accounts = db.Accounts.Where(acc => acc.UserID == State.UserID);
                accounts = Global.SetAccountCurrentAmount(db, accounts.ToList());

                return Util.CreateJsonResponse<Account>(sidx, sord, page, rows, accounts,rt, (Func<IEnumerable<Account>, Array>)delegate(IEnumerable<Account> rd)
                {
                    if (rt == ResultType.Web)
                    {
                        return (
                        from account in rd
                        select new
                        {
                            PayeeID = account.AccountID,
                            cell = new string[] { account.AccountID.ToString(), account.Name.ToString(), account.InitialAmount.ToString(), account.AccountType.String(), account.CurrentAmount.ToString(), (account.SharedFriends.Count > 0).ToString() }
                        }).ToArray();
                    }
                    else
                    {
                        return (
                       from acc in rd
                       select new
                       {
                           AccountID = acc.AccountID,
                           Name = acc.Name,
                           InitialAmount = acc.InitialAmount,
                           CurrentAmount = acc.CurrentAmount,
                           AccountTypeText = acc.AccountType.String(),
                           AccountType = acc.AccountType.GetHashCode()
                       }).ToArray();
                    }
                }
                    );

        }
        // GET: /Account/JListOther
        public JsonResult JListOther(string sidx, string sord, int page, int rows)
        {
                var accounts = db.Users.Find(State.UserID).OtherAccounts.Select(oa=>oa.Account).ToList();
                Global.SetAccountCurrentAmount(db, accounts);

                return Util.CreateJsonResponse<Account>(sidx, sord, page, rows, accounts, (Func<IEnumerable<Account>, Array>)delegate(IEnumerable<Account> rd)
                {
                    return (
                        from account in rd
                        select new
                        {
                            PayeeID = account.AccountID,
                            cell = new string[] { account.AccountID.ToString(), account.Name.ToString(), account.InitialAmount.ToString(), account.AccountType.String(), account.CurrentAmount.ToString(), (account.User.UserName + "," + string.Join(",", account.SharedFriends.Select(au => au.User.UserName).ToArray())).TrimEnd(',') }
                        }).ToArray();
                }
                    );

        }
        [HttpPost]
        public JsonResult MEdit([Bind(Include = "AccountID,Name,InitialAmount,AccountType")] int AccountID, string Name, double InitialAmount, AccountType accountType)
        {
            return Edit<JsonResult>(AccountID, Name, InitialAmount, accountType);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public JsonResult JEdit([Bind(Include = "AccountID,Name,InitialAmount,AccountType")] int AccountID, string Name, double InitialAmount, AccountType accountType)
        {
            return Edit<JsonResult>(AccountID, Name, InitialAmount, accountType);
        }
        private T Edit<T>([Bind(Include = "AccountID,Name,InitialAmount,AccountType")] int AccountID, string Name, double InitialAmount, AccountType accountType)where T:JsonResult
        {
            if (ModelState.IsValid)
            {
                Account account = null;
                bool add = false;
                if(AccountID <= 0)
                {
                    add = true;
                    account = new Account();
                    account.UserID = State.UserID;
                }
                else
                {
                    account = db.Accounts.Where(acc => acc.AccountID == AccountID && acc.UserID == State.UserID).SingleOrDefault();
                    if(account == null)
                       return Repository.Failure<T>();
                }

                account.Name = Name;
                account.InitialAmount = InitialAmount;
                account.AccountType = accountType;

                if (add) //add
                {
                    db.Accounts.Add(account);
                }
                else
                {
                    db.Entry(account).State = EntityState.Modified;
                }
                db.SaveChanges();
                return Repository.Success<T>(account.AccountID);
            }
            return Repository.Failure<T>();
        }
        public JsonResult MAccounts()
        {
            GroupList<MutableTuple<int, string>> groups = new GroupList<MutableTuple<int, string>>();

            var maccounts = db.Accounts.Where(acc => acc.UserID == State.UserID).ToList();
            var faccounts = db.Users.Find(State.UserID).OtherAccounts.Select(oa => oa.Account).ToList();
            var accounts = maccounts.Union(faccounts).ToList();

            foreach (var acc in accounts)
            {
                groups.AddGroupDetails(acc.UserID == State.UserID ? "" : acc.User.UserName, new MutableTuple<int, string>(acc.AccountID, acc.Name));
            }
            return Util.Package<JsonResult>(groups.Groups.Select(g =>
                   new
                   {
                       UserName = g.Name,
                       Accounts = g.GroupDetails.Select(mt => new { AccountID = mt.Item1, Name = mt.Item2 })
                   }), accounts.Count());
        }
        public string JAccounts()
        {
            var accounts = db.Users.Find(State.UserID).Accounts;
            IEnumerable<Account> otherAccounts = db.Users.Find(State.UserID).OtherAccounts.Select(oa => oa.Account);

            string selectStr = string.Empty;

            if (Request.IsAjaxRequest())
            {
                selectStr += "<optgroup label='My Accounts' />";
                    foreach (var account in accounts)
                    {
                        selectStr += string.Format("<option value='{0}'>{1}</option>", account.AccountID, account.Name);
                    }

                if(otherAccounts.Count() > 0)
                {
                    selectStr += "<optgroup label='Shared Accounts' />";
                    foreach (var account in otherAccounts)
                    {
                        selectStr += string.Format("<option value='{0}'>{1}</option>", account.AccountID, account.Name);
                    }
                }
                
            }

            selectStr = "<select>" + selectStr + "</select>";
            return selectStr;
            //return Json(selectStr, JsonRequestBehavior.AllowGet);
        }
        public string JSourceAccounts()
        {
            var accounts = db.Users.Find(State.UserID).Accounts;
            IEnumerable<Account> otherAccounts = db.Users.Find(State.UserID).OtherAccounts.Select(oa => oa.Account);

            string selectStr = string.Empty;

            if (Request.IsAjaxRequest())
            {
                foreach (var account in accounts)
                {
                    selectStr += string.Format("<option value='{0}'>{1}</option>", account.AccountID, account.Name);
                }
            }

            selectStr = "<select>" + selectStr + "</select>";
            return selectStr;
            //return Json(selectStr, JsonRequestBehavior.AllowGet);
        }
        public string JAccountTypes()
        {
            string selectStr = string.Empty;
            
            selectStr += string.Format("<option value='{0}'>{1}</option>",AccountType.Debit.GetHashCode(), AccountType.Debit.String());
            selectStr += string.Format("<option value='{0}'>{1}</option>", AccountType.Credit.GetHashCode(), AccountType.Credit.String());
            selectStr += string.Format("<option value='{0}'>{1}</option>", AccountType.Saving.GetHashCode(), AccountType.Saving.String());

            selectStr = "<select>" + selectStr + "</select>";
            return selectStr;
        }
        [HttpPost]
        public JsonResult MDelete(int AccountID)
        {
            return Delete<JsonResult>(AccountID);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public JsonResult JDelete(int AccountID)
        {
            return Delete<JsonResult>(AccountID);
        }

        private T Delete<T>(int AccountID) where T : JsonResult
        {
                try
                {
                    Account account = db.Accounts.Where(acc => acc.UserID == State.UserID && acc.AccountID == AccountID).FirstOrDefault();
                    if (account != null)
                    {
                        db.Accounts.Remove(account);
                        db.SaveChanges();
                        return Repository.Success<T>(account.AccountID);
                    }
                }
                catch (Exception ex)
                {
                    
                }
return Repository.DelFailure<T>();
        }

        public JsonResult MTransferRecord(int TransferID)
        {
            var transfers = db.AccountTransfers.Where(tr => tr.UserID == State.UserID && tr.TransferID == TransferID).ToList();
            return Util.Package<JsonResult>(transfers.Select(transfer => new
            {
                TransferID = transfer.TransferID,
                TransferDate = transfer.TransferDate.ToUTCDateString(),
                SourceAccount = transfer.SourceAccountID,
                SourceAccountText = transfer.SourceAccount.Name,
                TargetAccount = transfer.TargetAccountID,
                TargetAccountText = transfer.TargetAccount.Name,
                Amount = transfer.Amount.ToString(),
                Description = transfer.Description 
            }));
        }
        public JsonResult MTransferList(int page, int rows)
        {
            return (JsonResult) TransferList("TransferID", "desc", page, rows, ResultType.Mobile);
        }

        // GET: /Account/
        public JsonResult JTransferList(string sidx, string sord, int page, int rows)
        {
            return TransferList(sidx, sord, page, rows, ResultType.Web);
        }

        private JsonResult TransferList(string sidx, string sord, int page, int rows, ResultType rt)
        {
                var transfers = db.AccountTransfers.Where(acc => acc.UserID == State.UserID).ToList();

                return Util.CreateJsonResponse<AccountTransfer>(sidx, sord, page, rows, transfers, rt, (Func<IEnumerable<AccountTransfer>, Array>)delegate(IEnumerable<AccountTransfer> rd)
                {
                    if (rt == ResultType.Web)
                    {
                        return (
                        from transfer in rd
                        select new
                        {
                            TransferID = transfer.TransferID,
                            cell = new string[] { transfer.TransferID.ToString(), transfer.TransferDate.ToDateString(), transfer.SourceAccount.Name, transfer.TargetAccount.Name, transfer.Amount.ToString(), transfer.Description }
                        }).ToArray();
                    }
                    else
                    {
                        return (
                        from transfer in rd
                        select new
                        {
                            TransferID = transfer.TransferID,
                            TransferDate = transfer.TransferDate.ToDateDisplayString(), 
                            SourceAccount = transfer.SourceAccountID,
                            SourceAccountText = transfer.SourceAccount.Name, 
                            TargetAccount = transfer.TargetAccountID,
                            TargetAccountText = transfer.TargetAccount.Name, 
                            Amount = transfer.Amount.ToString(), 
                            Description = transfer.Description 
                        }).ToArray();
                    }
                    
                }
                    );
            

        }
        public JsonResult MTransferEdit(int TransferID, DateTime TransferDate, int SourceAccount, int TargetAccount, double amount, string description)
        {
            return TransferEdit<JsonResult>(TransferID, TransferDate, SourceAccount, TargetAccount, amount, description);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public JsonResult JTransferEdit( int TransferID, DateTime TransferDate, int SourceAccount, int TargetAccount, double amount, string description)
        {
            return TransferEdit<JsonResult>(TransferID, TransferDate, SourceAccount, TargetAccount, amount, description);
        }

        private T TransferEdit<T>( int TransferID,DateTime TransferDate, int SourceAccount, int TargetAccount, double amount, string description) where T:JsonResult
        {
                Account saccount = Global.geAllUserAccounts(db).Where(acc => acc.AccountID == SourceAccount).SingleOrDefault();
                Account taccount = Global.geAllUserAccounts(db).Where(acc => acc.AccountID == TargetAccount).SingleOrDefault();
                if (saccount == null || taccount == null)
                {
                    return Global.BadRequest<T>();
                }
                if (saccount.AccountID == taccount.AccountID)
                {
                    return Repository.Failure<T>("Source and Target accounts cannot be same.");
                }
                AccountTransfer transfer = null;
                
                if(TransferID != 0)
                    transfer = db.AccountTransfers.Where(tran => tran.TransferID == TransferID && tran.UserID == State.UserID).FirstOrDefault();
                
                bool add = false;
                if (transfer == null) //add
                {
                    transfer = new AccountTransfer();
                    transfer.UserID = State.UserID;
                    add = true;
                }
                transfer.SourceAccountID = SourceAccount;
                transfer.TargetAccountID = TargetAccount;
                transfer.Amount = amount;
                transfer.TransferDate = TransferDate;
                transfer.Description = description;
                if (add)
                    db.AccountTransfers.Add(transfer);
                else
                    db.Entry(transfer).State = EntityState.Modified;

                db.SaveChanges();
                return Repository.Success<T>(transfer.TransferID);
        }
        public JsonResult MTransferDelete(int TransferID)
        {
            return DeleteTransfer<JsonResult>(TransferID);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public JsonResult JTransferDelete(int TransferID)
        {
            return DeleteTransfer<JsonResult>(TransferID);
        }

        private T DeleteTransfer<T>(int TransferID) where T : JsonResult
        {
                try
                {
                    AccountTransfer transfer = db.AccountTransfers.Where(t => t.UserID == State.UserID && t.TransferID == TransferID).FirstOrDefault();
                    if (transfer != null)
                    {
                        db.AccountTransfers.Remove(transfer);
                        db.SaveChanges();
                        return Repository.Success<T>(transfer.TransferID);
                    }
                }
                catch (Exception ex)
                {
                    
                }
return Repository.DelFailure<T>();
        }
        // GET: /Account/Share/5
        public ActionResult Share(int id)
        {
            Account acc = db.Users.Find(State.UserID).Accounts.Find(e => e.AccountID == id);
            if (acc == null)
            {
                return HttpNotFound();
            }
            else
                return View(acc);
        }
        public ActionResult JShare(string sidx, string sord, int page, int rows)
        {
            ApplicationUser u = db.Users.Find(State.UserID);
            int id = Util.ParseInt(Request.Params["AccountID"]);
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Account ev = u.Accounts.Find(a => a.AccountID == id);
            if (ev == null)
            {
                return HttpNotFound();
            }

                List<ApplicationUser> friends = u.Friends.Where(f => f.Status == Common.FriendStatus.Approved).Select(f=>f.UserFriend).ToList();

                List<ApplicationUser> afriends = u.Accounts.Find(a => a.AccountID == id).SharedFriends.Select(sf=>sf.User).ToList();

                friends = friends.Except(afriends).ToList(); // friends with which account is not shared yet
                // we have to identify later in the anonymous func that which users are already shared and which ones are yet to be shared. we will use
                // usertype to flag the users. Not a good approach and may be changed later

                afriends.ForEach(af => af.Type = UserType.Application); // used the usertype to flag the user as event user already shared

                friends.ForEach(f => f.Type = UserType.FB);// used the usertype to flag the user as event user already shared

                return Util.CreateJsonResponse<ApplicationUser>(sidx, sord, page, rows, friends.Union(afriends), (Func<IEnumerable<ApplicationUser>, Array>)delegate(IEnumerable<ApplicationUser> rd)
                {
                    return (
                        from usr in rd
                        select new
                        {
                            FriendID = usr.Id,
                            cell = new string[] { usr.Id.ToString(), usr.UserName, usr.Type == UserType.Application ? "" : usr.Id.ToString() }
                        }).ToArray();
                }
                    );
        }

        // POST: /Event/ShareConfirmed/5/6
        [HttpPost, ActionName("Share")]
        [ValidateAntiForgeryToken]
        public ActionResult ShareConfirmed(int AccountID, string UserID)
        {
                Account acc = db.Users.Find(State.UserID).Accounts.Find(a => a.AccountID == AccountID);
                Friend fr = db.Users.Find(State.UserID).Friends.Find(f => f.FriendID == UserID);
                if (acc != null && fr != null)
                {
                    AccountUser au = new AccountUser();
                    au.AccountID = AccountID;
                    au.UserID = UserID;
                    db.AccountUsers.Add(au);
                    NotificationManager.AddAccountShareNotification(acc.AccountID, acc.Name, UserID, db, State.UserID, State.CurrentUserName);
                    try
                    {

                    db.SaveChanges();
                    return Json(new HttpStatusCodeResult(HttpStatusCode.OK));
                    }
                    catch (Exception ex)
                    {
                        ex.ToString();
                    }
                }
                return Json(new HttpStatusCodeResult(HttpStatusCode.BadRequest));
        }
    }
}
