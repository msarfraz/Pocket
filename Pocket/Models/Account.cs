using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Pocket.Models
{
    [Table("accounts")]
    public class Account
    {
        [Required()]
        [Column("account_id", Order = 0)]
        [Key()]
        public int AccountID { get; set; }

        [Required()]
        [Column("user_id")]
        [ForeignKey("User")]
        public int UserID { get; set; }

        public virtual User User { get; set; }

        [Required()]
        [Display(Name = "Account Name", Description = "Name of Account")]
        [Column("account_name")]
        [StringLength(500)]
        public string Name { get; set; }

        [Column("initial_amount")]
        public double InitialAmount { get; set; }

        [Column("current_amount")]
        public double CurrentAmount { get; set; }
    }
}