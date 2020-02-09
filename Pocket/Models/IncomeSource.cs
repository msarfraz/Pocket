using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Pocket.Models
{
    [Table("sources")]
    public class IncomeSource
    {
        [Required()]
        [Column("source_id", Order = 0)]
        [Key()]
        public int SourceID { get; set; }

        [Required()]
        [Column("user_id")]
        [ForeignKey("User")]
        public int UserID { get; set; }
        public virtual User User { get; set; }

        [Required()]
        [Display(Name = "Source Name", Description = "Source of Income")]
        [Column("source_name")]
        [StringLength(500)]
        public string Name { get; set; }

        [Display(Name = "Icon")]
        [Column("icon_id")]
        public int? IconID { get; set; }

        [ForeignKey("IconID")]
        public virtual Icon IconFile { get; set; }

    }
}