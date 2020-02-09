using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Pocket.Models
{
    [Table("expensecomments")]
    public class ExpenseComment
    {
        [Required()]
        [Column("comment_id", Order = 0)]
        [Key()]
        public int CommentID { get; set; }

        [Display(Name = "Comments", Description = "Comment By")]
        [Required()]
        [Column("user_id")]
        public int UserID { get; set; }
        public virtual User User { get; set; }

        [Required()]
        [Column("expense_id")]
        public int ExpenseID { get; set; }
        public virtual Expense Expense { get; set; }

        [Required()]
        [Display(Name = "Comments", Description = "Expense Comments")]
        [Column("comment")]
        [StringLength(500)]
        public string Comment { get; set; }

        [Required()]
        [Display(Name = "Date", Description = "Commented On")]
        [Column("comment_date")]
        public DateTime CommentDate { get; set; }
    }
}