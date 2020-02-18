using Pocket.Common;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pocket.Models
{
    [Table("income")]
    public class Income
    {
        [Required()]
        [Column("income_id", Order=0)]
        [Key()]
        public int IncomeID { get; set; }

        [Required()]
        [Column("user_id")]
        public string UserID { get; set; }

        public virtual ApplicationUser User { get; set; }

        [Required()]
        [Column("account_id")]
        public int AccountID { get; set; }

        [Required()]
        [Column("source_id")]
        public int SourceID { get; set; }

        [Column("amount")]
        public double Amount { get; set; }

        [DataType(DataType.Date)]
        [Column("income_date")]
        public DateTime IncomeDate { get; set; }

        [Column("description")]
        [StringLength(500)]
        public string Description { get; set; }

        public virtual IncomeSource IncomeSource { get; set; }
        [ForeignKey("AccountID")]
        public virtual Account Account { get; set; }

        [Column("repeat")]
        public RepeatPattern Repeat { get; set; }

        [Column("schedule_id")]
        public int? ScheduleID { get; set; }
        public virtual Schedule Schedule { get; set; }
    }
}