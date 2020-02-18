using Pocket.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Pocket.ViewModels
{
    public class TargetResult
    {
        public int TargetID { get; set; }

        public double TargetAmount { get; set; }

        public DateTime ExpectedDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime TargetDate { get; set; }
        public RepeatPattern BudgetDuration { get; set; }
        public double BudgetAmount { get; set; }
        public YesNoOptions Budgeted { get; set; }
        public double InitialAmount { get; set; }

        public string Name { get; set; }

        public TargetStatus Status { get; set; }

        public double TargetPercentage { get; set; }

        public double CurrentSaving { get; set; }
        public double RequiredSaving { get; set; }
        public double TotalSaving { get; set; }
    }
}