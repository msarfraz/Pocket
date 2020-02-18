using Pocket.Models;
using Pocket.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Pocket.Extensions;
namespace Pocket.Common
{
    public static class Util
    {
        public static object GetPropertyValue(object obj, string property)
        {
            return obj.GetType().GetProperty(property).GetValue(obj);
        }
        public static JsonResult CreateJsonResponse<T>(string sidx, string sord, int page, int rows, IEnumerable<T> rowdata, Func<IEnumerable<T>, Array> UIdata)
        {
            return CreateJsonResponse<T>(sidx, sord, page, rows, rowdata, ResultType.Web, UIdata);
        }
        public static JsonResult CreateJsonResponse<T>(string sidx, string sord, int? page, int? rows, IEnumerable<T> rowdata, ResultType rt, Func<IEnumerable<T>, Array> UIdata) 
        {
            int totalRecords = rowdata.Count();
            int pageIndex = (page.HasValue ? page.Value : 1) - 1;
            int pageSize = rows.HasValue ? rows.Value : totalRecords;
            int totalPages = (int)Math.Ceiling((float)totalRecords / (float)pageSize);
            IEnumerable<T> result;
            //Global.ApplicationName
            if (sidx != Global.SortNotRequired)
            {
                IOrderedEnumerable<T> ioi = null;
                foreach (var si in sidx.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries))
                {
                    string[] ss = si.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    string sd;
                    if (ss.Length == 1)
                        sd = sord;
                    else if (ss.Length == 2)
                        sd = ss[1];
                    else continue;

                    if (sd == "asc") // sord
                    {
                        if (ioi == null)
                            ioi = rowdata.OrderBy(p => Util.GetPropertyValue(p, ss[0]));
                        else
                            ioi = ioi.ThenBy(p => Util.GetPropertyValue(p, ss[0]));
                    }
                    else
                    {
                        if (ioi == null)
                            ioi = rowdata.OrderByDescending(p => Util.GetPropertyValue(p, ss[0]));
                        else
                            ioi = ioi.ThenByDescending(p => Util.GetPropertyValue(p, ss[0]));
                    }

                }
                result = ioi.Skip(pageIndex * pageSize).Take(pageSize);
            }
            else
            {
                result = rowdata.Skip(pageIndex * pageSize).Take(pageSize);
            }
            
            var uidata = UIdata(result);
            
            var jsonData = new
            {
                total = totalPages,
                page = pageIndex + 1,
                records = totalRecords,
                rows = uidata
            };
            JsonResult ret = null;
            if (rt == ResultType.Web)
                ret = new JsonResult();
            else
                ret = new JsonResult();

            ret.Data = jsonData;
            ret.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            
            return ret;
        }
        public static T Package<T>(object data, int totalRecords = 1, int totalPages = 1, int pageIndex=0) where T:JsonResult
        {
            var jsonData = new
            {
                total = totalPages,
                page = pageIndex + 1,
                records = totalRecords,
                rows = data
            };
            JsonResult ret = new JsonResult(); // Activator.CreateInstance<T>();

            ret.Data = jsonData;
            ret.JsonRequestBehavior = JsonRequestBehavior.AllowGet;

            return ret as T;
        }
        //public static JsonResult CreateJsonPResponse<T>(int page, int rows, IQueryable<T> rowdata, Func<IEnumerable<T>, Array> UIdata)
        //{
        //    int pageIndex = page - 1;
        //    int pageSize = rows;
        //    int totalRecords = rowdata.Count();
        //    int totalPages = (int)Math.Ceiling((float)totalRecords / (float)pageSize);
            
        //    IEnumerable<T> result;

        //    result = rowdata.Skip(pageIndex * pageSize).Take(pageSize);


        //    var uidata = UIdata(result);
        //    var jsonpData = new
        //    {
        //        total = totalPages,
        //        page = page,
        //        records = totalRecords,
        //        rows = uidata
        //    };

        //    JsonResult jpr = new JsonResult(jsonpData);

        //    return jpr;
        //}
        public static DateTime FromDateTime(string datetime)
        {
            try
            {
                return DateTime.Parse(datetime);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
                return DateTime.Now;
        }
        public static DateTime ToDateTime(object datetime, DateTime defaultDate = new DateTime())
        {
            try
            {
                return DateTime.ParseExact(datetime.ToString(), "MM/dd/yyyy", null);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return defaultDate;
            }
        }
        public static DateTime? ToDateTime(string datetime)
        {
            try
            {
                return DateTime.ParseExact(datetime.ToString(), "MM/dd/yyyy", null);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }
        public static string ToDateString(this DateTime? dt)
        {
            if(dt.HasValue)
                return dt.Value.ToString("MM/dd/yyyy");
            else
                return string.Empty;
        }
        public static string ToDateString(this DateTime dt)
        {
                return dt.ToString("MM/dd/yyyy");
        }
        public static string ToUTCDateString(this DateTime dt)
        {
            return dt.ToString("yyyy-MM-dd");
        }
        public static string ToDateDisplayString(this DateTime dt)
        {
            return dt.ToString("dd-MMM-yyyy");
        }
        public static int MonthLastDay(this DateTime dt)
        {
            return DateTime.DaysInMonth(dt.Year, dt.Month);
        }
        public static DateTime MonthLastDate(this DateTime dt)
        {
            return new DateTime(dt.Year, dt.Month, dt.MonthLastDay());
        }
        public static DateTime MonthFirstDate(this DateTime dt)
        {
            return new DateTime(dt.Year, dt.Month, 1);
        }
        public static int DaysInMonth(this DateTime dt)
        {
            return DateTime.DaysInMonth(dt.Year, dt.Month);
        }
        public static int GetMonthsBetweenDates(DateTime dtFrom, DateTime dtTo)
        {
            if (dtFrom >= dtTo)
            {
                return 1;
            }
            if (dtFrom.Month == dtTo.Month && dtFrom.Year == dtTo.Year)
            {
                return 1;
            }
            // more than one month involve
            var totaldays = dtTo.Subtract(dtFrom).TotalDays;
            var f = dtFrom.DaysInMonth() - dtFrom.Day;
            var t = dtTo.Day;

            if (totaldays > f + t) // more than two month involve
            {
                var m = Math.Ceiling( (totaldays - (f + t)) / 30);
                return (int) m;
            }
            else
                return 2;
        }
        public static int ParseInt(string p, int defValue = 0)
        {
            try
            {
                return int.Parse(p);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return defValue;
                
            }
        }
        public static int? ToInt(string p)
        {
            try
            {
                if (p == string.Empty)
                    return null;
                return int.Parse(p);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public static string GetUserName(ApplicationUser u)
        {
            return u.UserName;
        }
        public static DateTime GetNextRunDate(RepeatPattern pattern, DateTime LastRunDate)
        {
            switch (pattern)
            {
                case RepeatPattern.None:
                    return LastRunDate.AddDays(pattern.GetHashCode());
                case RepeatPattern.Daily:
                    return LastRunDate.AddDays(pattern.GetHashCode());
                case RepeatPattern.Alternate_Days:
                    return LastRunDate.AddDays(pattern.GetHashCode());
                case RepeatPattern.Weekly:
                    return LastRunDate.AddDays(pattern.GetHashCode());
                case RepeatPattern.Bi_Weekly:
                    return LastRunDate.AddDays(pattern.GetHashCode());
                case RepeatPattern.Monthly:
                    return LastRunDate.AddMonths(1);
                case RepeatPattern.Bi_Monthly:
                    return LastRunDate.AddMonths(2);
                case RepeatPattern.Quarterly:
                    return LastRunDate.AddMonths(3);
                case RepeatPattern.Bi_Quarterly:
                    return LastRunDate.AddMonths(6);
                case RepeatPattern.Yearly:
                    return LastRunDate.AddYears(1);
                default:
                    return LastRunDate;
            }
        }
        public static string EnumToString<T>(T value )
        {
            return Enum.GetName(value.GetType(), value);
        }
        

        internal static string GetBudgetStatus(double Budget, double Amount)
        {
            var diff = Budget - Amount;
            int perc = (int)((diff / (Budget == 0 ? 1 : Budget)) * 100);
            string ptype = "";

            if (perc <= 20)
            {
                ptype = "danger";
            }
            else if (perc <= 40)
            {
                ptype = "warning";
            }
            else if (perc <= 60)
            {
                ptype = "info";
            }
            else //if (perc <= 80)
            {
                ptype = "success";
            }
            return ptype;
        }

        internal static DateTime getLastDate(int Month, int Year)
        {
            return new DateTime(Year, Month, DateTime.DaysInMonth(Year, Month));
        }

        internal static bool HasExpenseAccess(Expense ei, string userid)
        {
            bool success = false;
            if (ei.UserID == userid)
                success = true;
            else if (ei.Event.UserID == userid)
                success = true;
            else if (ei.Event.SharedFriends.Select(sf => sf.UserID).Contains(userid))
                success = true;
            else if (ei.Account.UserID == userid)
                success = true;
            else if (ei.Account.SharedFriends.Select(sf => sf.UserID).Contains(userid))
                success = true;

            return success;
        }

        internal static bool ToBool(string p)
        {
            try
            {
                return bool.Parse(p);
            }
            catch
            {
                
            }
            return false;
        }
        internal static string ToString(object p)
        {
            if (p == null)
                return string.Empty;

            return p.ToString();
        }
        public static Array ToMobileExpenses(IEnumerable<Expense> expenses)
        {
            string userid = State.UserID;
             return expenses.Select(ex => new { 
                ExpenseID = ex.ExpenseID,
                AccountID = ex.AccountID,
                AccountText = ex.Account.Name,
                Amount = ex.Amount,
                Description = ex.Description,
                EventID = ex.EventID,
                EventText = ex.EventID.HasValue ? ex.Event.Name : "",
                ExpenseDate = ex.ExpenseDate.ToUTCDateString(),
                ExpenseDateText = ex.ExpenseDate.ToDateDisplayString(),
                PayeeID = ex.PayeeID,
                PayeeText = ex.PayeeID.HasValue ? ex.Payee.Name : "",
                CategoryID = ex.Subcategory.CategoryID,
                CategoryText = ex.Subcategory.Category.Name,
                SubcategoryID = ex.SubcategoryID,
                SubcategoryText = ex.Subcategory.Name,
                VendorID = ex.VendorID,
                VendorText = ex.VendorID.HasValue ? ex.Vendor.Name : "",
                Repeat = ex.Repeat.GetHashCode(),
                RepeatText = ex.Repeat.String(),
                Editable = ex.UserID == userid
                }).ToList().ToArray();
        }
    }
    public class MutableTuple<T1,T2,T3>
    {
        public T1 Item1 { get; set; }
        public T2 Item2 { get; set; }
        public T3 Item3 { get; set; }

        public MutableTuple(T1 item1, T2 item2, T3 item3)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
        }
    }
    public class MutableTuple<T1,T2>
    {
        public T1 Item1 { get; set; }
        public T2 Item2 { get; set; }

        public MutableTuple(T1 item1, T2 item2)
        {
            Item1 = item1;
            Item2 = item2;
        }
    }
    public class Group<T>
    {
        public Group()
        {
            Name = string.Empty;
            GroupDetails = (List<T>) Activator.CreateInstance(typeof(List<T>)) ;
        }

        public string Name { get; set; }

        public List<T> GroupDetails { get; set; }
    }

    public class GroupList<T>
    {
        public GroupList()
        {
            Groups = new List<Group<T>>();
        }
        public void AddGroupDetails(string Name, T details)
        {
            bool newg = true;
            foreach (var item in Groups)
            {
                if (item.Name == Name)
                {
                    item.GroupDetails.Add(details);
                    newg = false;
                    break;
                }

            }
            if (newg)
            {
                Group<T> g = new Group<T>();
                g.Name = Name;
                g.GroupDetails.Add(details);
                Groups.Add(g);
            }
            
        }
        public List<Group<T>> Groups { get; set; }
    }
}