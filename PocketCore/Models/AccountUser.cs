using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Pocket.Models
{
    [Table("account_user")]
    public class AccountUser
    {
        [Required()]
        [Key, Column("account_id", Order = 1)]
        public int AccountID { get; set; }

        [Required()]
        [Key, Column("user_id", Order = 0)]
        public string UserID { get; set; }

        public virtual ApplicationUser User { get; set; }
        public virtual Account Account { get; set; }
        
    }
}