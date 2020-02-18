using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Pocket.Models
{
    public class AccountTransfer
    {
        [Required()]
        [Column("transfer_id", Order = 0)]
        [Key()]
        public int TransferID { get; set; }

        [Required()]
        [Column("user_id")]
        [ForeignKey("User")]
        public string UserID { get; set; }
        [JsonIgnoreAttribute]
        public virtual ApplicationUser User { get; set; }
        
        [Required()]
        [Column("transfer_date")]
        public DateTime TransferDate { get; set; }

        [Column("description")]
        [StringLength(500)]
        public string Description { get; set; }

        [Column("amount")]
        public double Amount { get; set; }

        [Column("source_account")]
        [ForeignKey("SourceAccount")]
        public int SourceAccountID { get; set; }

        [JsonIgnoreAttribute]
        public virtual Account SourceAccount { get; set; }

        [Column("target_account")]
        [ForeignKey("TargetAccount")]
        public int TargetAccountID { get; set; }

        [JsonIgnoreAttribute]
        public virtual Account TargetAccount { get; set; }

    }
}