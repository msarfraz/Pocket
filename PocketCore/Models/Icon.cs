using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pocket.Models
{
    [Table("icons")]
    public class Icon
    {
        [Required()]
        [Column("icon_id", Order = 0)]
        [Key()]
        public int IconID { get; set; }

        [Required()]
        [Column("user_id")]
        [ForeignKey("User")]
        public string UserID { get; set; }

        public virtual ApplicationUser User { get; set; }

        [Required()]
        [Display(Name = "Icon Name", Description = "Name of Icon")]
        [Column("icon_name")]
        [StringLength(500)]
        public string Name { get; set; }

        
    }
}