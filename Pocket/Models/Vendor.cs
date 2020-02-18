using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Pocket.Models
{
    [Table("vendors")]
    public class Vendor
    {
        [Required()]
        [Column("vendor_id")]
        [Key()]
        public int VendorID { get; set; }

        [Required()]
        [Column("user_id", Order = 1)]
        [ForeignKey("User")]
        public string UserID { get; set; }

        [JsonIgnoreAttribute]
        public virtual ApplicationUser User { get; set; }

        [Required()]
        [Display(Name = "Vendor Name", Description = "Name of Vendor")]
        [Column("vendor_name")]
        [StringLength(500)]
        public string Name { get; set; }

        
    }
}