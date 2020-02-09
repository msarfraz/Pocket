using Pocket.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Pocket.Models
{
    [Table("budgets")]
    public class Budget
    {
        public Budget()
        {
            BudgetDuration = Common.RepeatPattern.Monthly;
        }
        [Required()]
        [Column("budget_id", Order = 0)]
        [Key()]
        public int BudgetID { get; set; }

        [Required()]
        [Column("budget_amount")]
        public double BudgetAmount { get; set; }

        [Required()]
        [Column("budget_duration")]
        public RepeatPattern BudgetDuration { get; set; }

        [Required()]
        [Column("budget_type")]
        public BudgetType BudgetType { get; set; }

        [Required()]
        [Column("user_id")]
        public int UserID { get; set; }

        public virtual User User { get; set; }
    }
}