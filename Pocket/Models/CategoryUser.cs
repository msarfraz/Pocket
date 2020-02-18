using Pocket.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Pocket.Models
{
    [Table("category_user")]
    public class CategoryUser
    {
        [Required()]
        [Key, Column("category_id", Order = 1)]
        public int CategoryID { get; set; }

        [Required()]
        [Key, Column("user_id", Order = 0)]
        public string UserID { get; set; }

        [Column("display")]
        [Required]
        public DisplaySetting Display { get; set; }

        public virtual ApplicationUser User { get; set; }
        public virtual Category Category { get; set; }

    }
}