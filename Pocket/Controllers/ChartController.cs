using Pocket.Common;
using Pocket.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Collections;
using System.Drawing;
using Pocket.ViewModels;
using Pocket.Extensions;

namespace Pocket.Controllers
{
    [Authorize]
    public class ChartController : Controller
    {
        private QDbContext db = new QDbContext();

        public ActionResult CategoryChart()
        {
            return View();
        }
        #region Category Chart
        
        [ValidateAntiForgeryToken]
        [HttpPost]
        public JsonResult JCategoryChartData(int Month, int Year)
        {
            return CategoryChartData<JsonResult>(Month, Year, false);

        }
        public JsonResult MCategoryChartData(string YearMonth)
        {
            var dt = Util.FromDateTime(YearMonth);

            return CategoryChartData<JsonResult>(dt.Month, dt.Year, false);

            
        }
        public T CategoryChartData<T>(int Month, int Year, bool includeEvents) where T:JsonResult
        {
            var cats = Repository.GetCategoryBudgetAmount(db, Month, Year, false);
            if (includeEvents)
            {
                var events = getEventBudgetAmount(db, Month, Year);
                cats = cats.Union(events).ToList();
            }
            var Data = cats.Select(a => new { name =  a.Name, data = new object[]{ a.Amount, a.Budget }}).ToArray();
            if (Data.Length == 0) // if there is no category/event to show. select all categories so that chart is displayed.
            {
              //  Data = new object[][] { new object[] { "No Category", 0, 0 } };
            }
            var labels = cats.Select(a => a.Name).ToArray();
            var expenses = new { name = "Expense", data = cats.Select(a => a.Amount ).ToArray() };
            var budget = new { name = "Budget", data = cats.Select(a => a.Budget).ToArray() };
            var data = new  {
            categories = labels,
            series = new object[] { expenses, budget }
            } ;

            return Util.Package<T>(data);
        }
        
        #endregion

        #region Subcategory Chart

        public ActionResult SubcatChart()
        {
            
            var cats = db.Categories.Where(c=>c.UserID == State.UserID && c.Display == DisplaySetting.Yes);
            var ucats = db.CategoryUsers.Where(uc => uc.UserID == State.UserID && uc.Display == DisplaySetting.Yes).Select(uc => uc.Category);

            
            ChartData model = new ChartData(db);
            
            model.Categories = cats.ToList();
            model.OtherCategories = ucats.ToList();

            return View(model);
        }
        
        
        [ValidateAntiForgeryToken]
        [HttpPost]
        public JsonResult JSubcategoryChartData(int Month, int Year, int CategoryID)
        {
            return SubcategoryChartData<JsonResult>(Month, Year, CategoryID);

        }
        public JsonResult MSubcategoryChartData(string YearMonth, int CategoryID)
        {
            var dt = Util.FromDateTime(YearMonth);
            return SubcategoryChartData<JsonResult>(dt.Month, dt.Year, CategoryID);

        }
        private T SubcategoryChartData<T>(int Month, int Year, int CategoryID) where T:JsonResult
        {
            var scats = getSubCategoryBudgetAndAmount(Month, Year, CategoryID);
            
            var Data = scats.Select(a => new object[] { a.Name, a.Amount, a.Budget }).ToArray();
            if (Data.Length == 0) // if there is no subcategory to show. select all categories so that chart is displayed.
            {
                Data = new object[][] { new object[] { "No Subcategory", 0, 0 } };
            }
            var labels = scats.Select(a => a.Name).ToArray();
            var expenses = new { name = "Expense", data = scats.Select(a => a.Amount).ToArray() };
            var budget = new { name = "Budget", data = scats.Select(a => a.Budget).ToArray() };
            var data = new
            {
                categories = labels,
                series = new object[] { expenses, budget }
            };

//            var labels = new object[] { new object[] { "Subcategory", "Expense", "Budget" } };
  //          var data = labels.Union(Data);

            return Util.Package<T>(data);

        }
        private List<SubcategoryBudgetAmount> getSubCategoryBudgetAndAmount(int Month, int Year, int categoryID)
        {
            List<SubcategoryBudgetAmount> scats = new List<SubcategoryBudgetAmount>();
            var cats = Repository.GetCategoryBudgetAmount(db, Month, Year, false, categoryID);
            if (cats.Count > 0)
                scats = cats[0].Subcategories;
            return scats;
            
        }
        #endregion

        #region Events chart

        public ActionResult EventChart()
        {
            var cats = getEventBudgetAmount(db, DateTime.Now.Month, DateTime.Now.Year);

            return View(cats);
        }


        [ValidateAntiForgeryToken]
        [HttpPost]
        public JsonResult JEventChartData(int Month, int Year)
        {
            return EventChartData<JsonResult>(Month, Year);
        }

        public JsonResult MEventChartData(string YearMonth)
        {
            var dt = Util.FromDateTime(YearMonth);
            return EventChartData<JsonResult>(dt.Month, dt.Year);

        }
        private T EventChartData<T>(int Month, int Year) where T:JsonResult
        {
            var events = getEventBudgetAmount(db, Month, Year);

            var Data = events.Select(a => new object[] { a.Name, a.Amount, a.Budget }).ToArray();
            if (Data.Length == 0) // if there is no event, add a dummy
            {
                Data = new object[][] { new object[] { "No Event", 0, 0 } };
            }
            
            var labels = events.Select(a => a.Name).ToArray();
            var expenses = new { name = "Expense", data = events.Select(a => a.Amount).ToArray() };
            var budget = new { name = "Budget", data = events.Select(a => a.Budget).ToArray() };
            var data = new
            {
                categories = labels,
                series = new object[] { expenses, budget }
            };

            //var labels = new object[] { new object[] { "Subcategory", "Expense", "Budget" } };
            //var data = labels.Union(Data);

            return Util.Package<T>(data);
        }
        private static List<CategoryBudgetAmount> getEventBudgetAmount(QDbContext db, int Month, int Year)
        {
            // events of the month
            var monthEvents = db.Events.Where( ev => ev.UserID == State.UserID && ev.EventDate.Month == Month && ev.EventDate.Year == Year);
            // expenses occurred in the month for any events
            var allevents = db.Events.Where(ev => ev.UserID == State.UserID).Select(ev=>ev.EventID).Union(db.EventUsers.Where(evu => evu.UserID == State.UserID).Select(evu => evu.Event.EventID)).ToArray();
            var exEvents = db.Expenses.Where(ex => ex.ExpenseDate.Month == Month && ex.ExpenseDate.Year == Year && ex.EventID.HasValue && allevents.Contains(ex.EventID.Value)).DefaultIfEmpty().Select(ex=>ex.Event);
            var events = exEvents.Union(monthEvents).Where(ev=>ev != null).ToList();

            List<CategoryBudgetAmount> cab = new List<CategoryBudgetAmount>();
            foreach (var ev in events)
            {
                double actuals = db.Expenses.Where(ex => ex.EventID == ev.EventID).Select(ex => ex.Amount).DefaultIfEmpty(0).Sum();
                cab.Add(new CategoryBudgetAmount { Name = ev.Name, Amount = actuals, Budget = ev.Budget.BudgetAmount });
            }
            return cab;
        }
        #endregion

        #region Homepage Chart
        public ActionResult HomepageChart()
        {

            return View();
        }

        public JsonResult MBudgetChartData(string yearMonth)
        {
            DateTime dt = DateTime.Now;
            dt = new DateTime(dt.Year, dt.Month - 1, 1);

            var budget = Pocket.Common.Global.getMonthBudget(db, dt.Month, dt.Year, false);

            var expense = Pocket.Common.Global.getMonthExpense(db, dt.Month, dt.Year);
            var diff = budget - expense;
            int perc = (int)((diff / (budget == 0 ? 1 : budget)) * 100);

            return Util.Package<JsonResult>(new object[]{ new { value = perc }});

        }
        public JsonResult MHomeChartData(string yearMonth)
        {
            int year, month;
            year =  DateTime.Now.Year;
            month = DateTime.Now.Month;
            if(!string.IsNullOrEmpty(yearMonth) && yearMonth.Split('-').Length > 0)
            {
                year = Util.ParseInt( yearMonth.Split('-')[0], year);
                month = Util.ParseInt(yearMonth.Split('-')[1], month);
            }
            return HomeChartData<JsonResult>(year, month);

        }
        public JsonResult JHomeChartData(int? year, int? month)
        {

            return HomeChartData<JsonResult>(year.HasValue?year.Value:DateTime.Today.Year, month.HasValue?month.Value:DateTime.Today.Month);

        }
        private T HomeChartData<T>(int year, int month) where T : JsonResult
        {
            DateTime dt = new DateTime(year, month, 1);
            int Month = dt.Month;
            int Year = dt.Year;
         /*   HomeChartSeries series = new HomeChartSeries(getExpenseSeries(db, Month, Year),
                getIncomeSeries(db, Month, Year),
                getBudgetSeries(db, Month, Year, true),
                getBudgetSeries(db, Month, Year, false),
                getEventSeries(db, Month, Year),
                getTargetSeries(db, Month, Year), Month, Year);*/

            ArrayList al = new ArrayList();
            // categories
            var days = new ArrayList();
            for (int i = 0; i <= DateTime.DaysInMonth(year, month); i++ )
            { 
                days.Add(i);
            }
      /*          al.Add(days.ToArray());

            // data
            
            al.Add(getBudgetSeries(db, month, year,true).Select(hcp => hcp.Amount).ToArray());
            al.Add(getBudgetSeries(db, month, year,false).Select(hcp => hcp.Amount).ToArray());
            al.Add(getEventSeries(db, month, year).Select(hcp => hcp.Amount).ToArray());
            al.Add(getTargetSeries(db, month, year).Select(hcp => hcp.Amount).ToArray()); */

            var labels = days.ToArray();
            var expenses = new { name = "Expense", data = getExpenseSeries(db, month, year).Select(hcp => new object[] { hcp.Day, hcp.Amount }).ToArray() };
            var income = new { name = "Income", data = getIncomeSeries(db, month, year).Select(hcp => new object[]{ hcp.Day, hcp.Amount }).ToArray() };
            var budget = new { name = "Budget", data = getBudgetSeries(db, month, year, true).Select(hcp => new object[] { hcp.Day, hcp.Amount }).ToArray() };
            var limit = new { name = "Limit", data = getBudgetSeries(db, month, year, false).Select(hcp => new object[] { hcp.Day, hcp.Amount }).ToArray() };
        //    var events = new { name = "Events", type = "bubble", data = getEventSeries(db, month, year).Select(hcp => new {x = hcp.Day, y = hcp.Amount, z=-1, name = hcp.Name}).ToArray() };
         //   var targets = new { name = "Targets", data = getTargetSeries(db, month, year).Select(hcp => hcp.Amount).ToArray() };

            var data = new
            {
                categories = labels,
                series = new object[] { expenses, income, budget, limit }
            };

            return Util.Package<T>(data);

        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public JsonResult HomepageChartData(int Month, int Year)
        {

            HomeChartSeries series = new HomeChartSeries(
                getExpenseSeries(db, Month, Year),
                getIncomeSeries(db, Month, Year),
                getBudgetSeries(db, Month, Year, true),
                getBudgetSeries(db, Month, Year, false),
                getEventSeries(db, Month, Year),
                getTargetSeries(db, Month, Year), Month, Year);

            return Util.Package<JsonResult>(series);

        }

        static List<HomeChartPoint> getIncomeSeries(QDbContext db, int Month, int Year)
        {
            var myincome = from inc in db.Income
                           where inc.UserID == State.UserID && inc.IncomeDate.Month == Month && inc.IncomeDate.Year == Year
                           group inc by inc.IncomeDate.Day into g
                           orderby g.Key
                           select new {IDate = g.Key, IAmount = g.Sum(inc=>inc.Amount) };
            List<HomeChartPoint> incomes = new List<HomeChartPoint>();
            double monthIncome = 0;
            //foreach (var item in myincome)
            //{
            //    monthIncome += item.IAmount;
            //    HomeChartPoint hcp = new HomeChartPoint { Day = item.IDate, Amount = monthIncome };
            //    incomes.Add(hcp);
            //}
            int max = DateTime.DaysInMonth(Year, Month);

            for (int i = 1; i <= max; i++)
            {
                var item = myincome.FirstOrDefault(a => a.IDate == i);
                if (item != null)
                {
                    monthIncome += item.IAmount;

                    incomes.Add(new HomeChartPoint { Day = i, Amount = monthIncome });
                }
                    
            }
            return incomes;
        }
        static List<HomeChartPoint> getExpenseSeries(QDbContext db, int Month, int Year)
        {
            var mcats = db.Categories.Where(cat => cat.UserID == State.UserID);
            var fcats = db.CategoryUsers.Where(cu => cu.UserID == State.UserID).Select(cu=>cu.Category);
            var allcats = mcats.Union(fcats);

            var mevents = db.Events.Where(ev => ev.UserID == State.UserID && ev.Budgeted == YesNoOptions.Yes);
            var fevents = db.EventUsers.Where(eu => eu.UserID == State.UserID && eu.Event.Budgeted == YesNoOptions.Yes).Select(eu=>eu.Event);
            var allevents = mevents.Union(fevents);

            var cexpense = from exp in db.Expenses
                            join cats in allcats on exp.Subcategory.CategoryID equals cats.CategoryID
                            
                            where exp.ExpenseDate.Month == Month && exp.ExpenseDate.Year == Year && 
                            (!exp.EventID.HasValue || (exp.Event.Budgeted == YesNoOptions.No && exp.UserID == State.UserID))
                                select exp;

            var eexpense = from exp in db.Expenses
                            join evs in allevents on exp.EventID equals evs.EventID

                            where exp.ExpenseDate.Month == Month && exp.ExpenseDate.Year == Year
                               select exp;
                            /*group exp by exp.ExpenseDate.Day into g
                            orderby g.Key
                            select new { IDate = g.Key, IAmount = g.Sum(e => e.Amount) };*/

            var allexpense = cexpense.Union(eexpense);

            var myexpense = from exp in allexpense
                      group exp by exp.ExpenseDate.Day into g
                      orderby g.Key
                      select new { IDate = g.Key, IAmount = g.Sum(e => e.Amount) };

            List<HomeChartPoint> es = new List<HomeChartPoint>();

            double eamount = 0;
            /*foreach (var item in myexpense)
            {
                eamount += item.IAmount;
                HomeChartPoint hcp = new HomeChartPoint { Day = item.IDate, Amount = eamount };
                es.Add(hcp);

            }*/
            int max = DateTime.DaysInMonth(Year, Month);
            for (int i = 1; i <= max; i++)
            {
                var item = myexpense.FirstOrDefault(a => a.IDate == i);
                if (item != null)
                {
                    eamount += item.IAmount;

                    es.Add(new HomeChartPoint { Day = i, Amount = eamount });
                }
                    
            }
            return es;
        }
        private static List<HomeChartPoint> getBudgetSeries(QDbContext db, int Month, int Year, bool ConstantBudget)
        {
            double mbudget = Pocket.Common.Global.getMonthBudget(db, Month, Year, ConstantBudget);

            List<HomeChartPoint> bs = new List<HomeChartPoint>();
            bs.Add(new HomeChartPoint {Day = 1, Amount = mbudget });
            bs.Add(new HomeChartPoint {Day = DateTime.DaysInMonth(Year, Month), Amount = mbudget });
           /* for (int i = 1; i <= DateTime.DaysInMonth(Year, Month); i++)
            {
                bs.Add(new HomeChartPoint { Day = i, Amount = mbudget });
            }*/
            return bs;

        }
        private static List<HomeChartPoint> getEventSeries(QDbContext db, int Month, int Year)
        {
            var events = db.Users.Find(State.UserID).Events.Where(ev => ev.EventDate.Month == Month && ev.EventDate.Year == Year).OrderBy(ev => ev.EventDate);
            List<HomeChartPoint> series = new List<HomeChartPoint>();
            foreach (var ev in events)
            {
                //double actuals = db.Expenses.Where(ex => ex.EventID == ev.EventID).Select(ex => ex.Amount).DefaultIfEmpty(0).Sum();
                //double amount = Math.Max(ev.Budget.BudgetAmount, actuals);
                series.Add(
                    new HomeChartPoint
                    {
                        Name = ev.Name,
                        Day = ev.EventDate.Day,
                        Amount = ev.Budget.BudgetAmount
                    }
                    );
            }
            return series;
        }
        private static List<HomeChartPoint> getTargetSeries(QDbContext db, int Month, int Year)
        {
            var targets = db.Targets.Where(t => t.UserID == State.UserID && t.TargetDate.Month == Month && t.TargetDate.Year == Year).OrderBy(ev => ev.TargetDate);
            List<HomeChartPoint> series = new List<HomeChartPoint>();
            foreach (var t in targets)
            {
                series.Add(
                    new HomeChartPoint
                    {
                        Name = t.Name,
                        Day = t.TargetDate.Day,
                        Amount = t.TargetAmount
                    }
                    );
            }
            return series;
        }
        #endregion

        #region Target Chart
        public ActionResult TargetChart()
        {
            int month = DateTime.Now.Month;
            int year = DateTime.Now.Year;

            var mbudget = Pocket.Common.Global.getMonthBudget(db, DateTime.Now.Month, DateTime.Now.Year, false);
            

            return View();
        }

        
        #endregion

        #region Money Flow Chart
        public ActionResult MoneyFlowChart()
        {
            int month = DateTime.Now.Month;
            int year = DateTime.Now.Year;

            return View();
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public JsonResult JMoneyFlowChartData(int Month, int Year)
        {
            return MoneyFlowChartData<JsonResult>(Month, Year);

        }
        public JsonResult MMoneyFlowChartData(string YearMonth)
        {
            var dt = Util.FromDateTime(YearMonth);

            return MoneyFlowChartData<JsonResult>(dt.Month, dt.Year);

            
        }
        public T MoneyFlowChartData<T>(int Month, int Year) where T : JsonResult
        {
            DateTime dtNow = new DateTime(Year, Month, 1);
            DateTime dtTo = dtNow.AddMonths(1);

            List<MFlowNode> nodes = new List<MFlowNode>();
            MFlowNode parent = new MFlowNode { ID = "p_0", Name = "Accounts", Tooltip = "Accounts" };
            nodes.Add(parent);
            var accounts = Pocket.Common.Global.geAllUserAccounts(db).ToList();
            foreach (var account in accounts)
            {
                var tExpenses = db.Expenses.Where(ex => ex.AccountID == account.AccountID && ex.ExpenseDate < dtNow).
                                            Select(ex => ex.Amount).DefaultIfEmpty(0).Sum();
                var tIncomes = db.Income.Where(inc => inc.AccountID == account.AccountID && inc.IncomeDate < dtNow).
                                            Select(i => i.Amount).DefaultIfEmpty(0).Sum();
                var transferIns = db.AccountTransfers.Where(t => t.TargetAccountID == account.AccountID && t.TransferDate < dtNow).
                                            Select(i => i.Amount).DefaultIfEmpty(0).Sum();
                var transferOuts = db.AccountTransfers.Where(t => t.SourceAccountID == account.AccountID && t.TransferDate < dtNow).
                                            Select(i => i.Amount).DefaultIfEmpty(0).Sum();
                var tsavings = db.Savings.Where(s => s.AccountID == account.AccountID && s.SavingDate < dtNow).
                                            Select(i => i.Amount).DefaultIfEmpty(0).Sum();

                account.CurrentAmount = tIncomes + transferIns - tExpenses - transferOuts - tsavings;
               // account.CurrentAmount += account.InitialAmount;

                MFlowNode accnode = new MFlowNode { ID = "acc_" + account.AccountID, Amount = account.CurrentAmount, Name = account.Name, ParentID = parent.ID };

                var expenses = db.Expenses.Where(ex => ex.AccountID == account.AccountID && ex.ExpenseDate < dtTo && ex.ExpenseDate >= dtNow).
                                            Select(ex => ex.Amount).DefaultIfEmpty(0).Sum();
                MFlowNode exnode = new MFlowNode { ID = "ex_" + account.AccountID, Amount = expenses, Name = "Expenses", ParentID = accnode.ID };

                var incomes = db.Income.Where(inc => inc.AccountID == account.AccountID && inc.IncomeDate < dtTo && inc.IncomeDate >= dtNow).
                                            Select(i => i.Amount).DefaultIfEmpty(0).Sum();
                MFlowNode incnode = new MFlowNode { ID = "inc_" + account.AccountID, Amount = incomes, Name = "Income", ParentID = exnode.ID };

                var tIns = db.AccountTransfers.Where(t => t.TargetAccountID == account.AccountID && t.TransferDate < dtTo && t.TransferDate >= dtNow).
                                            Select(i => i.Amount).DefaultIfEmpty(0).Sum();
                MFlowNode tinode = new MFlowNode { ID = "ti_" + account.AccountID, Amount = tIns, Name = "Transfer In", ParentID = incnode.ID };

                var tOuts = db.AccountTransfers.Where(t => t.SourceAccountID == account.AccountID && t.TransferDate < dtTo && t.TransferDate >= dtNow).
                                            Select(i => i.Amount).DefaultIfEmpty(0).Sum();
                MFlowNode tonode = new MFlowNode { ID = "to_" + account.AccountID, Amount = tOuts, Name = "Transfer Out", ParentID = tinode.ID };

                var savings = db.Savings.Where(s => s.AccountID == account.AccountID && s.SavingDate < dtNow && s.SavingDate >= dtNow).
                                            Select(i => i.Amount).DefaultIfEmpty(0).Sum();
                MFlowNode snode = new MFlowNode { ID = "sav_" + account.AccountID, Amount = savings, Name = "Savings", ParentID = tonode.ID };

                var remaining = account.CurrentAmount + incomes + tIns - expenses - tOuts - savings;
                MFlowNode rnode = new MFlowNode { ID = "rem_" + account.AccountID, Amount = remaining, Name = "Remaining", ParentID = snode.ID };

                nodes.Add(accnode);
                nodes.Add(exnode);
                nodes.Add(incnode);
                nodes.Add(tinode);
                nodes.Add(tonode);
                nodes.Add(snode);
                nodes.Add(rnode);
            }
            parent.Amount = nodes.Where(node => node.ID.StartsWith("acc_")).Select(n => n.Amount).DefaultIfEmpty(0).Sum();
            var Data = nodes.Select(n => new object[] { new {v = n.ID, f = n.Text}, n.ParentID, n.Tooltip }).ToArray();

            return Util.Package<T>(Data);

        }
        #endregion
    }
}