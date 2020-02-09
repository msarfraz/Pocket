using Pocket.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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
            int pageIndex = page - 1;
            int pageSize = rows;
            int totalRecords = rowdata.Count();
            int totalPages = (int)Math.Ceiling((float)totalRecords / (float)pageSize);
            IEnumerable<T> result;

            IOrderedEnumerable<T> ioi = null;
            foreach (var si in sidx.Split(new string[]{","}, StringSplitOptions.RemoveEmptyEntries))
            {
                string[] ss = si.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                string sd;
                if (ss.Length == 1)
                    sd = sord;
                else if (ss.Length == 2)
                    sd = ss[1];
                else continue;

                if(sd == "asc") // sord
                {
                    if(ioi == null)
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
                

            var uidata = UIdata(result);
            var jsonData = new
            {
                total = totalPages,
                page = page,
                records = totalRecords,
                rows = uidata
                /*(
                from question in result
                select new
                {
                    payeeid = question.PayeeID,
                    cell = new string[] { "act", question.PayeeID.ToString(), question.Name.ToString() }
                }).ToArray()*/

            };

            JsonResult jr = new JsonResult();
            jr.Data = jsonData;
            jr.JsonRequestBehavior = JsonRequestBehavior.AllowGet;

            return jr;
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
        public static string GetUserName(User u)
        {
            return u.FirstName + " " + u.LastName;
        }
        public static DateTime GetNextRunDate(RepeatPattern pattern, DateTime LastRunDate)
        {
            switch (pattern)
            {
                case RepeatPattern.None:
                    return LastRunDate.AddDays(pattern.GetHashCode());
                case RepeatPattern.Daily:
                    return LastRunDate.AddDays(pattern.GetHashCode());
                case RepeatPattern.Weekly:
                    return LastRunDate.AddDays(pattern.GetHashCode());
                case RepeatPattern.Monthly:
                    return LastRunDate.AddMonths(1);
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
}