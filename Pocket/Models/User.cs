using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using Pocket.Common;

namespace Pocket.Models
{
    [Table("users")]
    public class User
    {
        [Required()]
        [Column("user_id", Order = 0)]
        [Key()]
        public int UserID { get; set; }

        [Display(Name = "Login ID")]
        [Column("login_id")]
        [StringLength(500)]
        public string LoginID { get; set; }

        [Display(Name = "Password", Description = "User's Password")]
        [Column("user_pass")]
        [StringLength(500)]
        public string Password { get; set; }

        [Column("secret_key")]
        [StringLength(500)]
        public string SecretKey { get; set; }

        [Display(Name = "First Name", Description = "First Name")]
        [Column("first_name")]
        [StringLength(500)]
        public string FirstName { get; set; }

        [Display(Name = "Last Name", Description = "Last Name")]
        [Column("last_name")]
        [StringLength(500)]
        public string LastName { get; set; }

        [Display(Name = "Email Address", Description = "Email Address")]
        [Column("email")]
        [EmailAddress()]
        [StringLength(500)]
        public string Email { get; set; }

        [Display(Name = "User Type", Description = "Type of User")]
        [Column("user_type")]
        public UserType Type { get; set; }

        public virtual List<Account> Accounts { get; set; }

        public virtual List<Category> Categories{ get; set; }

        public virtual ICollection<Expense> Expenses { get; set; }

        public virtual List<Income> Income { get; set; }

        public virtual List<IncomeSource> IncomeSources { get; set; }

        public virtual List<Payee> Payees { get; set; }

        public virtual List<Vendor> Vendors { get; set; }

        [ForeignKey("UserID")]
        public virtual List<Event> Events { get; set; }

        [ForeignKey("UserID")]
        public virtual List<Friend> Friends { get; set; }

        public virtual List<Budget> Budgets { get; set; }
        public virtual List<Target> Targets { get; set; }
        public virtual ICollection<Notification> Notifications{ get; set; }

    }
}