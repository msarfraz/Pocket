using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        [JsonIgnore]
        public virtual ApplicationUser User { get; set; }

        [Required()]
        [Display(Name = "Vendor Name", Description = "Name of Vendor")]
        [Column("vendor_name")]
        [StringLength(500)]
        public string Name { get; set; }

        
    }
}