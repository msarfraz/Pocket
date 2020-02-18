using Newtonsoft.Json;
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
        public string UserID { get; set; }
        [JsonIgnoreAttribute]
        public virtual ApplicationUser User { get; set; }

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

        [Required()]
        [Column("created_date")]
        public DateTime CreatedDate { get; set; }

        [Required()]
        [Column("modified_date")]
        public DateTime ModifiedDate { get; set; }

        [JsonIgnoreAttribute]
        public virtual Schedule Schedule { get; set; }
        [JsonIgnoreAttribute]
        public virtual Event Event { get; set; }
        [JsonIgnoreAttribute]
        public virtual Subcategory Subcategory { get; set; }
        [JsonIgnoreAttribute]
        public virtual Account Account { get; set; }
        [JsonIgnoreAttribute]
        public virtual Payee Payee { get; set; }
        [JsonIgnoreAttribute]
        public virtual Vendor Vendor { get; set; }
        [JsonIgnoreAttribute]
        public virtual ICollection<ExpenseComment> Comments { get; set; }
    }
}