using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;
using System.Security.Claims;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using Pocket.Common;
using System.ComponentModel.DataAnnotations;

namespace Pocket.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {

        [Required()]
        [Column("user_type")]
        public UserType Type { get; set; }


        [Column("user_page_settings")]
        public string PageSettings { get; set; }

        [Required()]
        [Column("email_confirmation_sent")]
        public DateTime EmailConfirmationSent { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }

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
        public virtual List<AccountUser> OtherAccounts { get; set; }
        public virtual List<EventUser> OtherEvents { get; set; }
        public virtual List<CategoryUser> OtherCategories { get; set; }
    }
}