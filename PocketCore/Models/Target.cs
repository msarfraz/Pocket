using Pocket.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace Pocket.Models
{
    [Table("targets")]
    public class Target
    {
        public Target()
        {

        }
        [Required()]
        [Column("target_id", Order = 0)]
        [Key()]
        public int TargetID { get; set; }

        [Required()]
        [Column("target_amount")]
        public double TargetAmount { get; set; }

        [Required()]
        [Column("initial_amount")]
        public double InitialAmount { get; set; }

        [Required()]
        [Column("expected_date")]
        public DateTime ExpectedDate { get; set; }

        [Required()]
        [Column("target_date")]
        public DateTime TargetDate { get; set; }

        [Required()]
        [Column("created_date")]
        public DateTime CreatedDate { get; set; }

        [Required()]
        [Display(Name = "Target Name", Description = "Name of Target")]
        [Column("target_name")]
        [StringLength(500)]
        public string Name { get; set; }

        [Required()]
        [Display(Name = "Status", Description = "Status of Target")]
        [Column("target_status")]
        public TargetStatus Status { get; set; }

        [Required()]
        [Column("user_id")]
        [ForeignKey("User")]
        public string UserID { get; set; }

        [Required()]
        [Column("budget_id")]
        [ForeignKey("Budget")]
        public int BudgetID { get; set; }

        public virtual Budget Budget { get; set; }

        [Required()]
        [Column("budgeted")]
        public YesNoOptions Budgeted { get; set; }
        
        public virtual ApplicationUser User { get; set; }
        public virtual ICollection<Saving> Savings { get; set; }
    }
}