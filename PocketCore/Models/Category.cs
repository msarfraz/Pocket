using Pocket.Common;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pocket.Models
{
    [Table("categories")]
    public class Category
    {
        [Required()]
        [Column("category_id", Order = 0)]
        [Key()]
        public int CategoryID { get; set; }

        [Required()]
        [Column("user_id")]
        [ForeignKey("User")]
        public string UserID { get; set; }

        public virtual ApplicationUser User { get; set; }

        [Required()]
        [Display(Name = "Category Name", Description = "Name of Category")]
        [Column("category_name")]
        [StringLength(500)]
        public string Name { get; set; }

        [Display(Name = "Icon")]
        [Column("icon_id")]
        public int? IconID { get; set; }

        [ForeignKey("IconID")]
        public virtual Icon IconFile { get; set; }

        [Column("budget_amount")]
        public double BudgetAmount { get; set; }

        public virtual List<Subcategory> Subcategories { get; set; }
     
        [ForeignKey("CategoryID")]
        [Display(Name = "Shared With Friends", Description = "Shared With Friends")]
        public virtual List<CategoryUser> SharedContacts { get; set; }

        [Column("display")]
        public DisplaySetting Display { get; set; }
    }

}