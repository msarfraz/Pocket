using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Pocket.Common
{
    public enum RepeatPattern
    {
        None = 0,
        Daily = 1,
        Weekly = 7,
        Monthly = 30,
        Yearly = 365
    }
    public enum BudgetType
    {
        Subcategory = 1,
        Event = 2,

    }
    public enum TargetStatus
    {
        InActive = 0,
        Active = 1
    }
    public enum FriendStatus
    {
        Pending = 1,
        Approved = 2
    }
    public enum ScheduleStatus
    {
        Pending = 1,
        Error = 2,
        InProcess = 3,
        Complete = 4
    }
    public enum ScheduleType
    {
        Expense =1,
        Income =2
    }
    public static class Global
    {
        
        public static string RepeatToString(RepeatPattern rec)
        {
            string rep = string.Empty;

            switch (rec)
            {
                case RepeatPattern.None:
                    rep = string.Empty;
                    break;
                case RepeatPattern.Daily:
                    rep = "Daily";
                    break;
                case RepeatPattern.Weekly:
                    rep = "Weekly";
                    break;
                case RepeatPattern.Monthly:
                    rep = "Monthly";
                    break;
                case RepeatPattern.Yearly:
                    rep = "Yearly";
                    break;
                default:
                    rep = string.Empty;
                    break;
            }
            return rep;
        }
    }
}