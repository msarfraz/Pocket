using Pocket.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Pocket.Models
{
    [Table("expenses")]
    public class Expense
    {
        [Required()]
        [Column("expense_id", Order = 0)]
        [Key()]
        public int ExpenseID { get; set; }

        [Required()]
        [Column("user_id")]
        public int UserID { get; set; }
        public virtual User User { get; set; }

        [Required()]
        [Column("account_id")]
        public int AccountID { get; set; }

        [Required()]
        [Column("subcategory_id")]
        public int SubcategoryID { get; set; }

        [Column("payee_id")]
        public int? PayeeID { get; set; }

        [Column("vendor_id")]
        public int? VendorID { get; set; }

        [Column("event_id")]
        public int? EventID { get; set; }

        [Column("amount")]
        public double Amount { get; set; }

        [Column("expense_date")]
        public DateTime ExpenseDate { get; set; }

        [Column("description")]
        [StringLength(500)]
        public string Description { get; set; }

        [Column("repeat")]
        public RepeatPattern Repeat { get; set; }

        [Column("schedule_id")]
        public int? ScheduleID { get; set; }

        public virtual Schedule Schedule { get; set; }
        public virtual Event Event { get; set; }
        public virtual Subcategory Subcategory { get; set; }
        public virtual Account Account { get; set; }
        public virtual Payee Payee { get; set; }
        public virtual Vendor Vendor { get; set; }
        public virtual ICollection<ExpenseComment> Comments { get; set; }
    }
}