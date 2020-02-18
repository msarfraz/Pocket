using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Pocket.Models
{
    [Serializable] 
    [Table("payees")]
    public class Payee
    {
        [Required()]
        [Column("payee_id", Order = 0)]
        [Key()]
        public int PayeeID { get; set; }

        [Required()]
        [Column("user_id", Order = 1)]
        [ForeignKey("User")]
        public string UserID { get; set; }

        [JsonIgnoreAttribute]
        public virtual ApplicationUser User { get; set; }

        [Required()]
        [Display(Name = "Payee Name", Description = "Name of Payee")]
        [Column("payee_name")]
        [StringLength(500)]
        public string Name { get; set; }

        
    }
}