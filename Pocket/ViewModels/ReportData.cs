using Pocket.Common;
using Pocket.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Pocket.ViewModels
{
    public class ReportData
    {
        QDbContext db;
        public ReportData()
        {
            Expenses = new List<Expense>();
            
        }
        public ReportData(QDbContext db):this()
        {
            this.db = db;
            CurrentUser = db.Users.Find(State.UserID);
        }
        public void LoadExpenses(QDbContext db)
        {
            this.db = db;
            CurrentUser = db.Users.Find(State.UserID);
            var exp = CurrentUser.Expenses.AsQueryable();

            if (AccountID.HasValue)
                exp = exp.Where((ex,b) => ex.AccountID == AccountID);
            if (CategoryID.HasValue)
                exp = exp.Where(ex => ex.Subcategory.CategoryID == SubcategoryID);
            if (SubcategoryID.HasValue)
                exp = exp.Where(ex => ex.SubcategoryID == SubcategoryID);
            if (VendorID.HasValue)
                exp = exp.Where(ex => ex.VendorID == VendorID);
            if (EventID.HasValue)
                exp = exp.Where(ex => ex.EventID == EventID);
            if (PayeeID.HasValue)
                exp = exp.Where(ex => ex.PayeeID == PayeeID);
            Expenses = exp.ToList();
        }
    
        public void LoadIncomes()
        {

        }

        public int? AccountID { get; set; }
        public int? CategoryID { get; set; }
        public int? SubcategoryID { get; set; }
        public int? VendorID { get; set; }
        public int? EventID { get; set; }
        public int? FriendID { get; set; }
        public int? IncomeSourceID { get; set; }
        public int? PayeeID { get; set; }


        public List<Expense> Expenses{get;set;}
        public List<Income> Incomes { get; set; }

        public List<Account> Accounts { get; set; }
        public List<Account> OtherAccounts { get; set; }
        public List<Category> Categories { get; set; }
        public List<Category> OtherCategories { get; set; }
        public List<Vendor> Vendors { get; set; }
        public List<Event> MyEvents { get; set; }
        public List<Event> SharedEvents { get; set; }
        public List<Payee> Payees { get; set; }


        public ApplicationUser CurrentUser { get; set; }
    }
}