using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

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
        public int UserID { get; set; }

        public virtual User User { get; set; }

        [Required()]
        [Display(Name = "Icon Name", Description = "Name of Icon")]
        [Column("icon_name")]
        [StringLength(500)]
        public string Name { get; set; }

        
    }
}