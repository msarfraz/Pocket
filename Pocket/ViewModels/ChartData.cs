using Pocket.Common;
using Pocket.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace Pocket.ViewModels
{
    public class ChartData
    {
        public ChartData(QDbContext db)
        {

        }

        public List<Expense> Expenses { get; set; }
        public List<Income> Incomes { get; set; }

        public List<Account> Accounts { get; set; }
        public List<Account> OtherAccounts { get; set; }
        
        public List<Category> Categories { get; set; }
        public List<Category> OtherCategories { get; set; }
        
        public List<Vendor> Vendors { get; set; }
        public List<Event> MyEvents { get; set; }
        public List<Event> SharedEvents { get; set; }
        public List<Payee> Payees { get; set; }
    }

    public class HomeChartPoint
    {
        public int Day { get; set; }
        public double Amount { get; set; }
        public string Name { get; set; }
    }

    public class HomeChartSeries
    {
        public HomeChartSeries(List<HomeChartPoint> expenses, List<HomeChartPoint> incomes, List<HomeChartPoint> averageBudget, List<HomeChartPoint> currentBudget,
            List<HomeChartPoint> events, List<HomeChartPoint> targets, int month, int year)
        {
            List<List<HomeChartPoint>> lists = new List<List<HomeChartPoint>>();
            lists.Add(incomes);
            lists.Add(expenses);
            lists.Add(averageBudget);
            lists.Add(currentBudget);
            //lists.Add(events);
            //lists.Add(targets);

            Data = new DataTable("HomeChartData");
            DataTable dt = Data;
            dt.Columns.Add("Date", typeof(int));
            dt.Columns.Add("Income", typeof(double));
            dt.Columns.Add("Expense", typeof(double));
            dt.Columns.Add("Budget", typeof(double));
            dt.Columns.Add("Limit", typeof(double));
            for (int i = 0; i < events.Count; i++)
            {
                dt.Columns.Add(events[i].Name, typeof(double));
            }
            for (int i = 0; i < targets.Count; i++)
            {
                dt.Columns.Add(targets[i].Name, typeof(double));
            }


            int days = DateTime.DaysInMonth(year, month);
            for (int day = 1; day <= days; day++)
            {
                DataRow dr = dt.NewRow();
                dr[0] = day;
                for (int j = 0; j < lists.Count; j++)
                {
                    var p = lists[j].Find(hcp => hcp.Day == day);
                    if (p != null)
                    {
                        dr[j + 1] = p.Amount;
                    }
                    else
                    {
                        if (dt.Rows.Count > 0)
                            dr[j + 1] = dt.Rows[dt.Rows.Count - 1][j + 1];
                        else
                            dr[j + 1] = 0; // default 0;
                    }
                }
                for (int i = 0; i < events.Count; i++)
                {
                    var plist = events.Where(hcp => hcp.Day == day);
                    foreach (var e in plist)
                    {
                        dr[e.Name] = e.Amount;
                    }
                }
                for (int i = 0; i < targets.Count; i++)
                {
                    var plist = targets.Where(hcp => hcp.Day == day);
                    foreach (var t in plist)
                    {
                        dr[t.Name] = t.Amount;
                    }
                }  
                dt.Rows.Add(dr);
            }
        }

        public DataTable Data { get; set; }

        public object[][] ToArray()
        {
            List<string> cols = new List<string>();
            foreach (DataColumn dc in Data.Columns)
            {
                cols.Add(dc.ColumnName);
            }

            List<object[]> ret = new List<object[]>();
            ret.Add(cols.ToArray());
            
            var d = Data.Select().Select(dr => dr.ItemArray).ToArray();
            var data = ret.Union(d);

            return data.ToArray();
        }
    }

    public class MFlowNode
    {
        public string ID { get; set; }
        public string Text { get 
        {
            return Name + "<div style='color:red; font-style:italic'>" + Amount + "</div>";
        } 
        }
        public string Name { get; set; }
        public double Amount { get; set; }
        public string ParentID { get; set; }
        public string Tooltip { get; set; }
    }
}