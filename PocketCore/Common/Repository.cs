using Microsoft.AspNetCore.Mvc;
using Pocket.Models;
using Pocket.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pocket.Common
{
    public static class Repository
    {
        public static T Success<T>(int NewID, string message = "success") where T : JsonResult
        {
            T jr = Activator.CreateInstance<T>();
            
            jr.Value = new
            {
                success = true,
                message = message,
                new_id = NewID
            };

            return jr;
        }
        public static JsonResult Success(int NewID, ResultType rt)
        {
            JsonResult jr = new JsonResult(new
            {
                success = true,
                message = "success",
                new_id = NewID
            });
            
            return jr;
        }
        public static JsonResult Failure(string Message, ResultType rt)
        {
            JsonResult jr = new JsonResult(new
            {
                success = false,
                message = Message,
                new_id = 0
            });
            

            return jr;
        }
        public static T DelFailure<T>() where T : JsonResult
        {
            return Failure<T>("Unable to delete the record. Other objects might have dependency on it.");
        }
        public static T Failure<T>(string Message = "Model state is invalid.") where T : JsonResult
        {
            T jr = Activator.CreateInstance<T>();

            jr.Value = new
            {
                success = false,
                message = Message,
                new_id = 0
            };

            return jr;
        }

        public static IQueryable<Expense> GetAllAccessibleExpenses(ApplicationDbContext db, String UserID)
        {
            IQueryable<Expense> expenses = db.Expenses;
            var accounts = db.Accounts.Where(acc => acc.UserID == UserID).Union(db.AccountUsers.Where(acc => acc.UserID == UserID).Select(acc => acc.Account));
            var aexpenses = expenses.Join(accounts, acc => acc.AccountID, ex => ex.AccountID, (ex, acc) => ex);

            var cats = db.Categories.Where(cat => cat.UserID == UserID).Union(db.CategoryUsers.Where(cat => cat.UserID == UserID).Select(cat => cat.Category));

            var cexpenses = expenses.Join(cats, ex => ex.Subcategory.CategoryID, cat => cat.CategoryID, (ex, cat) => ex);

            var events = db.Events.Where(ev => ev.UserID == UserID).Union(db.EventUsers.Where(ev => ev.UserID == UserID).Select(ev => ev.Event));
            var eexpenses = expenses.Join(events, ex => ex.EventID, ev => ev.EventID, (ex, ev) => ex);
            expenses = aexpenses.Union(cexpenses).Union(eexpenses);
            return expenses;
        }
       public static IQueryable<Expense> GetEventExpenses(ApplicationDbContext db,string UserID, int EventID)
        {
            return GetExpenses(db,UserID, null, null, null,null, EventID, null, null, null, true);
        }
        public static Expense GetExpenseByID(ApplicationDbContext db,string UserID, int expenseID)
       {
           var expenses = Repository.GetAllAccessibleExpenses(db, UserID);
           return expenses.Where(ex => ex.ExpenseID == expenseID).SingleOrDefault();

       }
        public static IQueryable<Expense> GetExpenses(ApplicationDbContext db,String UserID, int? AccountID, int? SubcatID, int? PayeeID, int? VendorID, int? EventID, int? ExpenseID, DateTime? FromDate, DateTime? ToDate, bool AllUsers)
        {
                IQueryable<Expense> expenses = null;
                if (!AllUsers)
                    expenses = db.Expenses.Where(exp => exp.UserID == UserID);
                else
                    expenses = Repository.GetAllAccessibleExpenses(db, UserID);

                if (AccountID.HasValue)
                {
                    expenses = expenses.Where(exp => exp.AccountID == AccountID);

                }

                if (SubcatID.HasValue)
                {
                    expenses = expenses.Where(exp => exp.SubcategoryID == SubcatID);
                }


                if (EventID.HasValue)
                {
                    expenses = expenses.Where(exp => exp.EventID == EventID);
                }

                if (PayeeID.HasValue)
                    expenses = expenses.Where(exp => exp.PayeeID == PayeeID);
                if (VendorID.HasValue)
                    expenses = expenses.Where(exp => exp.VendorID == VendorID);
                if (ExpenseID.HasValue)
                    expenses = expenses.Where(exp => exp.ExpenseID == ExpenseID);
                if (FromDate.HasValue)
                    expenses = expenses.Where(exp => exp.ExpenseDate >= FromDate);
                if (ToDate.HasValue)
                    expenses = expenses.Where(exp => exp.ExpenseDate <= ToDate);

                return expenses;
        }
        /// <summary>
        /// category expenses w.r.t month
        /// </summary>
        /// <param name="db"></param>
        /// <param name="Month"></param>
        /// <param name="Year"></param>
        /// <param name="CategoryID"></param>
        /// <returns></returns>
        internal static List<CategoryBudgetAmount> GetCategoryBudget(ApplicationDbContext db, string UserID)
        {
            int Month = DateTime.Today.Month;
            int Year = DateTime.Today.Year;
            var cats = db.Categories.Where(c => c.UserID == UserID).ToList(); // db.Users.Find(State.UserID).Categories;
            return GetMonthlyCatBudget(db,UserID, cats);
        }
        /// <summary>
        /// category expenses w.r.t month
        /// </summary>
        /// <param name="db"></param>
        /// <param name="Month"></param>
        /// <param name="Year"></param>
        /// <param name="CategoryID"></param>
        /// <returns></returns>
        internal static List<CategoryBudgetAmount> GetSharedCategoryBudget(ApplicationDbContext db, string UserID)
        {
            int Month = DateTime.Today.Month;
            int Year = DateTime.Today.Year;

            var cats = db.CategoryUsers.Where(c => c.UserID == UserID).Select(cu=>cu.Category).ToList(); // db.Users.Find(State.UserID).Categories;
            return GetMonthlyCatBudget(db,UserID, cats);
        }
        internal static List<CategoryBudgetAmount> GetAllCategoryBudget(ApplicationDbContext db, string UserID)
        {
            int Month = DateTime.Today.Month;
            int Year = DateTime.Today.Year;
            var cats = db.Categories.Where(c => c.UserID == UserID).ToList();
            var ucats = db.CategoryUsers.Where(c => c.UserID == UserID).Select(cu => cu.Category).ToList(); // db.Users.Find(State.UserID).Categories;
            return GetMonthlyCatBudget(db,UserID, cats.Union(ucats).ToList());
        }
        internal static List<CategoryBudgetAmount> GetMonthlyCatBudget(ApplicationDbContext db, string UserID, List<Category> cats)
        {
            string userid = UserID;

            int Month = DateTime.Today.Month;
            int Year = DateTime.Today.Year;

            List<CategoryBudgetAmount> catsba = new List<CategoryBudgetAmount>();
            foreach (var cat in cats.ToList())
            {
                CategoryBudgetAmount cba = new CategoryBudgetAmount();
                cba.CategoryID = cat.CategoryID;
                cba.Name = cat.Name;
                cba.Display = cat.Display;
                cba.Shared = cat.SharedContacts.Count > 0;
                cba.Editable = cat.UserID == userid;
                cba.UserName = cat.UserID == userid?"":cat.User.UserName;
                foreach (var scat in cat.Subcategories)
                {
                    double sbudget = GetSubcatMonthlyBudget(db,UserID, scat, Month, Year, true);
                    cba.Subcategories.Add(new SubcategoryBudgetAmount { Name = scat.Name, Budget = sbudget, Amount = 0, BudgetDuration = scat.Budget.BudgetDuration, SubcategoryID = scat.SubcategoryID });
                }
                cba.Calculate();
                catsba.Add(cba);
            }
            return catsba;
        }

        /// <summary>
        /// category expenses w.r.t month
        /// </summary>
        /// <param name="db"></param>
        /// <param name="Month"></param>
        /// <param name="Year"></param>
        /// <param name="CategoryID"></param>
        /// <returns></returns>
        internal static List<CategoryBudgetAmount> GetCategoryBudgetAmount(ApplicationDbContext db,string UserID, int Month, int Year, bool ConstantBudget, int? CategoryID = null)
        {
            List<CategoryBudgetAmount> catsba = new List<CategoryBudgetAmount>();
            var cats = db.Categories.Where(c => c.UserID == UserID); // db.Users.Find(State.UserID).Categories;
            var ucats = db.CategoryUsers.Where(uc => uc.UserID == UserID).Select(uc => uc.Category);
            cats = cats.Union(ucats);

            if (CategoryID.HasValue)
                cats = cats.Where(cat => cat.CategoryID == CategoryID);
            foreach (var cat in cats.ToList())
            {
                CategoryBudgetAmount cba = new CategoryBudgetAmount();
                cba.Name = cat.Name + (cat.UserID == UserID ? "" : string.Format(" [{0}]", cat.User.UserName));
                cba.CategoryID = cat.CategoryID;
                foreach (var scat in cat.Subcategories)
                {
                    double sbudget = GetSubcatMonthlyBudget(db,UserID, scat, Month, Year, ConstantBudget);

                    double samount = GetSubcatExpenses(db, Month, Year, scat.SubcategoryID).Select(ex => ex.Amount).DefaultIfEmpty(0).Sum();
                    if (sbudget > 0 || samount > 0)
                        cba.Subcategories.Add(new SubcategoryBudgetAmount { Name = scat.Name, Budget = sbudget, Amount = samount });
                }
                cba.Calculate();
                if (cba.Budget > 0 || cba.Amount > 0)
                    catsba.Add(cba);
            }
            return catsba;
        }

        /// <summary>
        /// subcat expenses w.r.t month
        /// </summary>
        /// <param name="db"></param>
        /// <param name="Month"></param>
        /// <param name="Year"></param>
        /// <param name="SubcategoryID"></param>
        /// <returns></returns>
        internal static IQueryable<Expense> GetSubcatExpenses(ApplicationDbContext db, int Month, int Year, int SubcategoryID)
        {
            var expenses = db.Expenses.Where(ex => ex.ExpenseDate.Month == Month && ex.ExpenseDate.Year == Year && ex.SubcategoryID == SubcategoryID);
            //if (!includeEventExpenses)
                expenses = expenses.Where(ex => !ex.EventID.HasValue || (ex.Event.Budgeted == YesNoOptions.No));

            return expenses;
        }
        /// <summary>
        /// returns the monthly budget of subcategories excluding event expenses. (event expenses prev week for biweekly repeat pattern)
        /// </summary>
        /// <param name="db"></param>
        /// <param name="subcat"></param>
        /// <param name="Month"></param>
        /// <param name="Year"></param>
        /// <returns></returns>
        internal static double GetSubcatMonthlyBudget(ApplicationDbContext db,string UserID, Subcategory subcat, int Month, int Year, bool constantBudget)
        {
            double budget = 0;
            int monthDays = constantBudget ? 30 : DateTime.DaysInMonth(Year, Month);
            DateTime dt = new DateTime(Year, Month, 1);

            switch (subcat.Budget.BudgetDuration)
            {
                case RepeatPattern.None:
                    budget = 0;
                    break;
                case RepeatPattern.Daily:
                    budget = monthDays * subcat.Budget.BudgetAmount;
                    break;
                case RepeatPattern.Alternate_Days:
                    budget = (monthDays * subcat.Budget.BudgetAmount) / 2;
                    break;
                case RepeatPattern.Weekly:
                    budget = (((double)monthDays) / 7) * subcat.Budget.BudgetAmount;
                    break;
                case RepeatPattern.Bi_Weekly:
                    budget = (((double)monthDays) / 14) * subcat.Budget.BudgetAmount;
                    break;
                case RepeatPattern.Monthly:
                    budget = subcat.Budget.BudgetAmount;
                    break;
                case RepeatPattern.Bi_Monthly:
                    if (constantBudget)
                        budget = subcat.Budget.BudgetAmount / 2;
                    else
                    {
                        if (Month % 2 == 1) // first month
                            budget = subcat.Budget.BudgetAmount / 2;
                        else
                            budget = subcat.Budget.BudgetAmount - GetSubcatExpense(db,UserID, subcat, dt, false);
                    }

                    break;
                case RepeatPattern.Quarterly:
                    if (constantBudget)
                        budget = subcat.Budget.BudgetAmount / 3;
                    else
                    {
                        if (Month % 3 == 1) // first month
                            budget = budget / 3; 
                        else
                        {
                            budget = subcat.Budget.BudgetAmount / 3;
                            var currMonth = Month % 3;
                            if (currMonth == 0) // last month, full budget
                                currMonth = 3;
                            budget = budget * currMonth;

                            budget = budget - GetSubcatExpense(db,UserID, subcat, dt, false);
                        }
                        
                    }
                    break;
                case RepeatPattern.Bi_Quarterly:
                    if (constantBudget)
                        budget = subcat.Budget.BudgetAmount / 6;
                    else
                    {
                        budget = subcat.Budget.BudgetAmount / 6;
                        var currMonth = Month % 6;
                        if (currMonth == 0) // last month, full budget
                            currMonth = 6;
                        budget = budget * currMonth;

                        if (Month % 6 == 1) // first month
                            budget = subcat.Budget.BudgetAmount / 6;
                        else
                        {
                            budget = budget - GetSubcatExpense(db,UserID, subcat, dt, false);
                        }
                        
                    }
                    break;

                case RepeatPattern.Yearly:
                    if (constantBudget)
                        budget = subcat.Budget.BudgetAmount / 12;
                    else
                    {
                        if(Month == 1)
                            budget = subcat.Budget.BudgetAmount / 12;
                        else
                        {
                            budget = (subcat.Budget.BudgetAmount/12)*Month;
                            budget = budget - GetSubcatExpense(db,UserID, subcat, dt, false);
                        }
                        
                    }
                    break;
                default:
                    break;
            }

            return Math.Round(budget, 2);
        }
        /// <summary>
        /// Returns the expenses according to subcat's repeat pattern. Expenses against the events are not considered. 
        /// </summary>
        /// <param name="db"></param>
        /// <param name="subcat"></param>
        /// <param name="dtTo"></param>
        /// <param name="dateInclusive"></param>
        /// <returns></returns>
        internal static double GetSubcatExpense(ApplicationDbContext db,string UserID, Subcategory subcat, DateTime dtTo, bool dateInclusive)
        {
            IQueryable<Expense> expenses = null;

            switch (subcat.Budget.BudgetDuration)
            {
                case RepeatPattern.None:
                    break;
                case RepeatPattern.Daily:
                    expenses = db.Expenses.Where(exp => exp.UserID == UserID && exp.SubcategoryID == subcat.SubcategoryID && (exp.ExpenseDate < dtTo || (dateInclusive && exp.ExpenseDate == dtTo)));
                    break;
                case RepeatPattern.Alternate_Days:
                    var day = dtTo.Day;
                    if (day % 2 == 0) // second day
                        day = day - 1;
                    DateTime dt = new DateTime(dtTo.Year, dtTo.Month, day);
                    expenses = db.Expenses.Where(exp => exp.UserID == UserID && exp.SubcategoryID == subcat.SubcategoryID && exp.ExpenseDate >= dt && (exp.ExpenseDate < dtTo || (dateInclusive && exp.ExpenseDate == dtTo)));
                    break;
                case RepeatPattern.Weekly:
                    dt = dtTo.Subtract(new TimeSpan(dtTo.DayOfWeek.GetHashCode(), 0, 0, 0));
                    expenses = db.Expenses.Where(exp => exp.UserID == UserID && exp.SubcategoryID == subcat.SubcategoryID && exp.ExpenseDate >= dt && (exp.ExpenseDate < dtTo || (dateInclusive && exp.ExpenseDate == dtTo)));
                    break;
                case RepeatPattern.Bi_Weekly:
                    dt = dtTo.Subtract(new TimeSpan(dtTo.DayOfWeek.GetHashCode() + 7, 0, 0, 0));
                    expenses = db.Expenses.Where(exp => exp.UserID == UserID && exp.SubcategoryID == subcat.SubcategoryID && exp.ExpenseDate >= dt && (exp.ExpenseDate < dtTo || (dateInclusive && exp.ExpenseDate == dtTo)));
                    break;
                case RepeatPattern.Monthly:
                    dt = new DateTime(dtTo.Year, dtTo.Month, 1);
                    expenses = db.Expenses.Where(exp => exp.UserID == UserID && exp.SubcategoryID == subcat.SubcategoryID && exp.ExpenseDate >= dt && (exp.ExpenseDate < dtTo || (dateInclusive && exp.ExpenseDate == dtTo)));
                    break;
                case RepeatPattern.Bi_Monthly:
                    var month = dtTo.Month;
                    if (month % 2 == 0) // second month
                        month = month - 1;
                    dt = new DateTime(dtTo.Year, month, 1);
                    expenses = db.Expenses.Where(exp => exp.UserID == UserID && exp.SubcategoryID == subcat.SubcategoryID && exp.ExpenseDate >= dt && (exp.ExpenseDate < dtTo || (dateInclusive && exp.ExpenseDate == dtTo)));
                    break;
                case RepeatPattern.Quarterly:
                    month = dtTo.Month;
                    if (month % 3 == 2) // second month
                        month = month - 1;
                    if (month % 3 == 0) // third month
                        month = month - 2;
                    dt = new DateTime(dtTo.Year, month, 1);
                    expenses = db.Expenses.Where(exp => exp.UserID == UserID && exp.SubcategoryID == subcat.SubcategoryID && exp.ExpenseDate >= dt && (exp.ExpenseDate < dtTo || (dateInclusive && exp.ExpenseDate == dtTo)));
                    break;
                case RepeatPattern.Bi_Quarterly:
                    month = dtTo.Month;
                    if (month <= 6) // first half year
                        month = 1;
                    else
                        month = 7;
                    dt = new DateTime(dtTo.Year, month, 1);
                    expenses = db.Expenses.Where(exp => exp.UserID == UserID && exp.SubcategoryID == subcat.SubcategoryID && exp.ExpenseDate >= dt && (exp.ExpenseDate < dtTo || (dateInclusive && exp.ExpenseDate == dtTo)));
                    break;
                case RepeatPattern.Yearly:
                    dt = new DateTime(dtTo.Year, 1, 1);
                    expenses = db.Expenses.Where(exp => exp.UserID == UserID && exp.SubcategoryID == subcat.SubcategoryID && exp.ExpenseDate >= dt && (exp.ExpenseDate < dtTo || (dateInclusive && exp.ExpenseDate == dtTo)));
                    break;
                default:
                    break;
            }
            if (expenses != null)
            {
                //if (!considerEvents) // remove the expenses made against my events
                   // expenses = expenses.Where(ex => !(ex.EventID.HasValue && ex.Event.UserID == State.UserID));// expenses against events are not considered. events has their own budget & expenses
                expenses = expenses.Where(ex => !ex.EventID.HasValue || (ex.EventID.HasValue && ex.Event.Budgeted == YesNoOptions.No));
                return expenses.Select(ex => ex.Amount).DefaultIfEmpty(0).Sum();
            }
            return 0;
        }
    }
}