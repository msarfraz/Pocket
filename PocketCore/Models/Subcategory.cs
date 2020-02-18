using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pocket.Models
{
    [Table("subcategories")]
    public class Subcategory
    {
        [Required()]
        [Column("subcategory_id", Order = 0)]
        [Key()]
        public int SubcategoryID { get; set; }

        [Required()]
        [Column("category_id")]
        [ForeignKey("Category")]
        public int CategoryID { get; set; }

       
        [Required()]
        [Display(Name = "Sub Category", Description = "Name of Sub Category")]
        [Column("subcategory_name")]
        [StringLength(500)]
        public string Name { get; set; }

        [Column("budget_id")]
        [ForeignKey("Budget")]
        public int BudgetID { get; set; }

        public virtual Budget Budget { get; set; }
        [Display(Name = "Icon")]
        [Column("icon_id")]
        public int? IconID { get; set; }

        [ForeignKey("IconID")]
        public virtual Icon IconFile { get; set; }
        public virtual Category Category { get; set; }
    }
}