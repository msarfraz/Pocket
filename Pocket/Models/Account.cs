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
        public string UserID { get; set; }

        [JsonIgnoreAttribute]
        public virtual ApplicationUser User { get; set; }

        [Required()]
        [Display(Name = "Account Name", Description = "Name of Account")]
        [Column("account_name")]
        [StringLength(500)]
        public string Name { get; set; }

        [Column("initial_amount")]
        public double InitialAmount { get; set; }

        [Column("current_amount")]
        public double CurrentAmount { get; set; }
        
        [Column("account_type")]
        public AccountType AccountType { get; set; }

        [JsonIgnoreAttribute]
        [ForeignKey("AccountID")]
        [Display(Name = "Shared With Friends", Description = "Shared With Friends")]
        public virtual ICollection<AccountUser> SharedFriends { get; set; }
    }
}