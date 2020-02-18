using Pocket.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Pocket.Models
{
    [Table("savings")]
    public class Saving
    {
        public Saving()
        {

        }
        [Required()]
        [Column("saving_id", Order = 0)]
        [Key()]
        public int SavingID { get; set; }

        [Required()]
        [Column("amount")]
        public double Amount { get; set; }

        
        [Required()]
        [Column("user_id")]
        public string UserID { get; set; }

        [Required()]
        [Column("target_id")]
        public int TargetID { get; set; }

        [Required()]
        [Column("account_id")]
        public int AccountID { get; set; }

        [Required()]
        [Column("saving_date")]
        public DateTime SavingDate { get; set; }

        [Required()]
        [Column("created_date")]
        public DateTime CreatedDate { get; set; }

        public virtual Target Target { get; set; }
        public virtual Account Account { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}