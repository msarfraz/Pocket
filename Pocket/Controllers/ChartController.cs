using DotNet.Highcharts;
using DotNet.Highcharts.Enums;
using DotNet.Highcharts.Helpers;
using DotNet.Highcharts.Options;
using Pocket.Common;
using Pocket.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Pocket.Common;
using Pocket.Models;
using System.Collections;
using System.Drawing;

namespace Pocket.Controllers
{
    public class ChartController : Controller
    {
        private QDbContext db = new QDbContext();

        public ActionResult Index()
        {
            return View();
        }
        #region Category Chart
        public ActionResult CategoryChart(int? Month, int? Year)
        {
            var cats = getCategoryBudgetAndAmount(DateTime.Now.Month, DateTime.Now.Year);

            Tuple<Highcharts, Highcharts> model = new Tuple<Highcharts, Highcharts>(getCategoryPieChart(cats, "Categories Share"), getCategoryColumnChart(cats, "Categories Budget"));
            return View(model);
        }
        [ValidateAntiForgeryToken]
        [HttpPost]
        public JsonResult CategoryChartData(int Month, int Year)
        {
            var cats = getCategoryBudgetAndAmount(Month, Year);
            JsonResult jr = new JsonResult();
            object[] piesSeries = cats.Where(a => a.Value.Item2 > 0).Select(a => new object[] { a.Key, a.Value.Item2 }).ToArray();
            object[] colSeries0 = cats.Select(a=>(object)a.Value.Item2).ToArray();
            object[] colSeries1 = cats.Select(a=>(object)a.Value.Item1).ToArray();
            jr.Data = new object[] { piesSeries, colSeries0, colSeries1 }.ToArray();
            
            return jr;

        }

        /// <summary>
        /// It is used to create pie chart for category and subcategory
        /// </summary>
        /// <param name="cats"></param>
        /// <returns></returns>
        private Highcharts getCategoryPieChart(Dictionary<string, MutableTuple<double, double>> cats, string chartTitle)
        {
            Highcharts chart = new DotNet.Highcharts.Highcharts("piechart");
            chart.InitChart(new Chart() { DefaultSeriesType = ChartTypes.Pie, Type = ChartTypes.Pie, Options3d = new ChartOptions3d() { Alpha = 45, Enabled = true } })
                .SetTitle(new Title() { Text = chartTitle });

            chart.SetPlotOptions(new PlotOptions()
            {
                Pie = new PlotOptionsPie()
                {
                    DataLabels = new PlotOptionsPieDataLabels()
                    {
                        Format = "<b>{point.name}</b>: {point.percentage:.1f} %",
                    },
                    InnerSize = new PercentageOrPixel(100),
                    Depth = 45
                }
            });

            chart.SetSeries(new Series
            {
                Name = "Amount",
                Data = new Data(cats.Where(a => a.Value.Item2 > 0).Select(a => new object[] { a.Key, a.Value.Item2 }).ToArray())
            });
            return chart;
        }
        private Highcharts getCategoryColumnChart(Dictionary<string, MutableTuple<double, double>> cats, string chartTitle)
        {
            Highcharts chart = new DotNet.Highcharts.Highcharts("colchart");
            chart.InitChart(new Chart() { DefaultSeriesType = ChartTypes.Column, Type = ChartTypes.Column, Options3d = new ChartOptions3d() { Alpha = 45, Enabled = true } })
                .SetTitle(new Title() { Text = chartTitle });

            chart.SetPlotOptions(new PlotOptions()
            {
                Column = new PlotOptionsColumn()
                {
                    PointPadding = 0.2,
                    BorderWidth = 0
                }
                
            });
            chart.SetXAxis(new XAxis()
                {
                    Categories = cats.Keys.ToArray()
                });
            chart.SetYAxis(new YAxis(){
                Min = 0,
                Title = new YAxisTitle(){
                     Text = "Amount"
                }
            });
            string[] str = new string[]{};
            chart.SetSeries(new Series[]
            {
                new Series{
                    Name = "Amount",
                    Type = ChartTypes.Column,
                    Data = new Data(cats.Select(a=>(object)a.Value.Item2).ToArray())
                },
                new Series{
                    Name = "Budget",
                    Type = ChartTypes.Column,
                    Data = new Data(cats.Select(a=>(object)a.Value.Item1).ToArray())
                },
                new Series{
                    Name = "Amount",
                    Type = ChartTypes.Pie,
                    PlotOptionsPie = new PlotOptionsPie{ Size = new PercentageOrPixel(100), 
                        Center = new PercentageOrPixel[]{new PercentageOrPixel(80), new PercentageOrPixel(100)},
                        ShowInLegend = false,
                        Depth = 10
                    },
                    Data = new Data(cats.Where(a => a.Value.Item2 > 0).Select(a => new object[] { a.Key, a.Value.Item2 }).ToArray())
                    
                }
            });
            return chart;
        }
        private Dictionary<string, MutableTuple<double, double>> getCategoryBudgetAndAmount(int Month, int Year)
        {
            Dictionary<string, MutableTuple<double, double>> data = new Dictionary<string, MutableTuple<double, double>>(); // KeyValuePair<budget,amount>

            var cats = db.Users.Find(State.UserID).Categories;
            foreach (var cat in cats)
            {
                double budget = 0;
                foreach (var scat in cat.Subcategories)
                {
                    budget += scat.Budget.BudgetAmount;
                }
                data.Add(cat.Name, new MutableTuple<double, double>(budget, 0));
            }

            // Get Amount Data

            var expenses = db.Users.Find(State.UserID).Expenses.Where(ex => ex.ExpenseDate.Month == Month && ex.ExpenseDate.Year == Year);

            foreach (var exp in expenses)
            {
                data[exp.Subcategory.Category.Name].Item2 += exp.Amount;

            }
            return data;
            
        }
        #endregion

        #region Subcategory Chart

        public ActionResult SubcatChart()
        {
            
            var cats = db.Users.Find(State.UserID).Categories.Select(c => new SelectListItem() { Text = c.Name, Value = c.CategoryID.ToString() });
            var scats = getSubCategoryBudgetAndAmount(DateTime.Now.Month, DateTime.Now.Year, cats.Count() > 0 ? Util.ParseInt(cats.First().Value) : 0);

            var piechart = getCategoryPieChart(scats, "Subcategories Share");
            var colchart = getCategoryColumnChart(scats, "Subcategories Budget and Amount");

            Tuple<Highcharts,Highcharts, IEnumerable<SelectListItem>> model = new Tuple<Highcharts, Highcharts, IEnumerable<SelectListItem>>(piechart, colchart, cats);
            
            return View(model);
        }
        
        
        [ValidateAntiForgeryToken]
        [HttpPost]
        public JsonResult SubcatChartData(int Month, int Year, int CategoryID)
        {
            var scats = getSubCategoryBudgetAndAmount(Month, Year, CategoryID);
            JsonResult jr = new JsonResult();
            object[] piesSeries = scats.Where(a => a.Value.Item2 > 0).Select(a => new object[] { a.Key, a.Value.Item2 }).ToArray();
            object[] colSeries0 = scats.Select(a => (object)a.Value.Item2).ToArray();
            object[] colSeries1 = scats.Select(a => (object)a.Value.Item1).ToArray();
            jr.Data = new object[] { piesSeries, colSeries0, colSeries1, scats.Keys.ToArray() }.ToArray();

            return jr;

        }
        private Dictionary<string, MutableTuple<double, double>> getSubCategoryBudgetAndAmount(int Month, int Year, int categoryID)
        {
            Dictionary<string, MutableTuple<double, double>> data = new Dictionary<string, MutableTuple<double, double>>(); // KeyValuePair<budget,amount>

            var scats = db.Users.Find(State.UserID).Categories.Find(cat=>cat.CategoryID == categoryID).Subcategories;
            foreach (var scat in scats)
            {
                data.Add(scat.Name, new MutableTuple<double, double>(scat.Budget.BudgetAmount, 0));
            }

            // Get Amount Data

            var expenses = db.Users.Find(State.UserID).Expenses.Where(ex => ex.ExpenseDate.Month == Month && ex.ExpenseDate.Year == Year && ex.Subcategory.CategoryID == categoryID);

            foreach (var exp in expenses)
            {
                data[exp.Subcategory.Name].Item2 += exp.Amount;

            }
            return data;

        }
        #endregion

        #region Events chart

        public ActionResult EventChart()
        {
            var mbudget = getMonthBudget(DateTime.Now.Month, DateTime.Now.Year);
            var chart = getEventBubbleChart(getEventBudget(DateTime.Now.Month, DateTime.Now.Year, mbudget), mbudget );

            return View(chart);
        }


        [ValidateAntiForgeryToken]
        [HttpPost]
        public JsonResult EventChartData(int Month, int Year)
        {
            double mbudget = getMonthBudget(Month, Year);
            var data = getEventBudget(Month, Year, mbudget);
            var rdata = new ArrayList();
            foreach (var item in data)
            {
                rdata.Add(new { name = item.Name, data = item.Data.ArrayData });
            }
            JsonResult jr = new JsonResult();
            jr.Data = rdata.ToArray();

            return jr;

        }

        private List<Series> getEventBudget(int Month, int Year, double mbudget)
        {
            var events = db.Users.Find(State.UserID).Events.Where(ev => ev.EventDate.Month == Month && ev.EventDate.Year == Year).OrderBy(ev=>ev.EventDate);
            List<Series> series = new List<Series>();
            foreach (var ev in events)
            {
                series.Add(
                    new Series
                    {
                        Name = ev.Name,
                        Data = new Data(new object[]{
                            new { x = ev.EventDate.Day, y = ev.Budget.BudgetAmount, z = Math.Round((ev.Budget.BudgetAmount / mbudget) * 100, 0), name = ev.Name }    
                        })
                    }
                    );

            }
            return series;
        }
        private Highcharts getEventBubbleChart(List<Series> series, double mbudget)
        {
            Highcharts chart = new DotNet.Highcharts.Highcharts("eventchart");
            chart.InitChart(new Chart() { Type = ChartTypes.Bubble })
                .SetTitle(new Title() { Text = "Events" });

            chart.SetPlotOptions(new PlotOptions()
            {
                Bubble = new PlotOptionsBubble()
                {
                    Tooltip = new PlotOptionsBubbleTooltip
                    {
                        HeaderFormat = "<b>{series.name}</b><br>",
                        PointFormat = "Date {point.x} , Budget {point.y}"
                    }
                }

            });
            chart.SetXAxis(new XAxis()
            {
                AllowDecimals = false,
                Min = 1,
                Max = 31,
                Title = new XAxisTitle(){
                    Text = "Date"
                }
            });
            chart.SetYAxis(new YAxis()
            {
                AllowDecimals = false,
                Min = 0,
                Max = mbudget,
                Title = new YAxisTitle()
                {
                    Text = "Amount"
                }
                
            });
            
            
            chart.SetSeries(
                series.ToArray()    
            );
            return chart;
        }
        private double getMonthBudget(int month, int year)
        {
            double mbudget = 0;
            var cats = db.Users.Find(State.UserID).Categories;
            foreach (var cat in cats)
            {
                foreach (var scat in cat.Subcategories)
                {
                    mbudget += scat.Budget.BudgetAmount;
                }
            }

            return mbudget;
        }
        #endregion

        #region Homepage Chart
        public ActionResult HomepageChart()
        {
            int month = DateTime.Now.Month;
            int year = DateTime.Now.Year;

            var mbudget = getMonthBudget(DateTime.Now.Month, DateTime.Now.Year);
            var series = getEventBudget(month, year, mbudget);
            series.Add(getIncomeSeries(month, year));
            series.AddRange(getExpenseSeries(month, year, mbudget));


            Highcharts chart = getComboChart(series, mbudget);

            return View(chart);
        }
        Series getIncomeSeries(int Month, int Year)
        {
            var myincome = from inc in db.Income
                           where inc.IncomeDate.Month == Month && inc.IncomeDate.Year == Year
                           group inc by inc.IncomeDate.Day into g
                           orderby g.Key
                           select new {IDate = g.Key, IAmount = g.Sum(inc=>inc.Amount) };
            List<object> tmp = new List<object>();
            foreach (var item in myincome)
            {
                object[] objs = new object[] {item.IDate, item.IAmount };
                tmp.Add(objs);
            }

            Series s = new Series
            {
                Name = "Income",
                Type = ChartTypes.Line,
                Data = new Data(tmp.ToArray())
            };
            return s;
        }
        List<Series> getExpenseSeries(int Month, int Year, double mbudget)
        {
            var myexpense = from exp in db.Expenses
                            where exp.ExpenseDate.Month == Month && exp.ExpenseDate.Year == Year
                            group exp by exp.ExpenseDate.Day into g
                           orderby g.Key
                           select new { IDate = g.Key, IAmount = g.Sum(e => e.Amount) };

            List<object> es = new List<object>();
            List<object> bs = new List<object>();

            if (myexpense.Count() > 0) // insert a budget entry in the start of month
            {
                if (myexpense.First().IDate != 1)
                    bs.Add(new object[] { 1, mbudget });
            }
            else
                bs.Add(new object[] { 1, mbudget });

            foreach (var item in myexpense)
            {
                object[] objs = new object[] { item.IDate, item.IAmount };
                es.Add(objs);

                mbudget -= item.IAmount;
                bs.Add(new object[] { item.IDate, mbudget });
            }

            List<Series> s = new List<Series>();
            s.Add(new Series
            {
                Name = "Expense",
                Type = ChartTypes.Line,
                Data = new Data(es.ToArray())
            });
            s.Add(new Series
            {
                Name = "Budget",
                Type = ChartTypes.Line,
                Data = new Data(bs.ToArray())
            });

            return s;
        }
        private Highcharts getComboChart(List<Series> series, double mbudget)
        {
            Highcharts chart = new DotNet.Highcharts.Highcharts("eventchart");
            chart.InitChart(new Chart() { Type = ChartTypes.Bubble })
                .SetTitle(new Title() { Text = "Month Summary" });

            chart.SetPlotOptions(new PlotOptions()
            {
                Bubble = new PlotOptionsBubble()
                {
                    Tooltip = new PlotOptionsBubbleTooltip
                    {
                        HeaderFormat = "<b>{series.name}</b><br>",
                        PointFormat = "Date {point.x} , Budget {point.y}"
                    }
                }

            });
            chart.SetXAxis(new XAxis()
            {
                AllowDecimals = false,
                Min = 1,
                Max = 31,
                Title = new XAxisTitle()
                {
                    Text = "Date"
                }
            });
            chart.SetYAxis(new YAxis()
            {
                AllowDecimals = false,
                Min = 0,
                //Max = mbudget,
                Title = new YAxisTitle()
                {
                    Text = "Amount"
                }

            });


            chart.SetSeries(
                series.ToArray()
            );
            return chart;
        }

        #endregion

        #region Target Chart
        public ActionResult TargetChart()
        {
            int month = DateTime.Now.Month;
            int year = DateTime.Now.Year;

            var mbudget = getMonthBudget(DateTime.Now.Month, DateTime.Now.Year);
            
            Highcharts chart = getTargetChart(mbudget);

            return View(chart);
        }

        private Highcharts getTargetChart(double mbudget)
        {
            int userid = State.UserID;

            var targets = from t in db.Targets
                          where t.UserID == userid && t.Status == Common.TargetStatus.Active
                          orderby t.TargetDate
                          select t;
            var totalIncome = from inc in db.Income
                              where inc.UserID == userid
                              group inc by inc.User into g
                              select g.Sum(i=>i.Amount);
            var initialAmount = from acc in db.Accounts
                                where acc.UserID == userid
                                group acc by acc.UserID into g
                                select g.Sum(a => a.InitialAmount);
            var totalExpense = from exp in db.Expenses
                               where exp.UserID == userid
                               group exp by exp.UserID into g
                               select g.Sum(e => e.Amount);
            var netSaving = totalIncome.FirstOrDefault() + initialAmount.FirstOrDefault() - totalExpense.FirstOrDefault();


            Highcharts chart = new DotNet.Highcharts.Highcharts("targetchart");
            chart.InitChart(new Chart() { Type = ChartTypes.Column })
                .SetTitle(new Title() { Text = "Targets" });
            
            chart.SetPlotOptions(new PlotOptions()
            {
                Column = new PlotOptionsColumn()
                {
                    Stacking = Stackings.Normal
                    
                }

            });

            chart.SetTooltip(new Tooltip
            {
                Formatter = "function(){return '<b>'+ this.x +'</b><br/>'+" +
                        "this.series.name +': '+ this.y +'<br/>'+" + 
                        "'Total: '+ this.point.stackTotal}"
            });
            
            chart.SetXAxis(new XAxis()
            {
                Categories = new string[]{"Targets", "Saving"}
            });
            chart.SetYAxis(new YAxis()
            {
                AllowDecimals = false,
                Min = 0,
                //Max = mbudget,
                Title = new YAxisTitle()
                {
                    Text = "Amount"
                }
                

            });
            chart.SetLegend(new Legend
            {
                Align = HorizontalAligns.Right,
                X = -70,
                VerticalAlign = VerticalAligns.Top,
                Y = 20,
                Floating = true,
                BackgroundColor = new BackColorOrGradient(Color.White),
                BorderColor = Color.Black,
                BorderWidth = 1,
                Shadow = false

            });
            List<Series> series = new List<Series>();
            foreach (var t in targets)
            {
                Series s = new Series { Name = t.Name, Data = new Data(new object[] { t.TargetAmount, 0 }) };
                series.Add(s);
            }
            series.Add(new Series { Name = "Saving", Data = new Data(new object[] {0, netSaving }) }); //

            chart.SetSeries( series.ToArray() );
            return chart;
        }
        #endregion
    }
}