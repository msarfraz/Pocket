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

namespace Pocket.Models
{
    public class QDbContext: IdentityDbContext<ApplicationUser>
    {
        public QDbContext()
            : base("QDbContext")
        {

        }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<AccountUser> AccountUsers { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<CategoryUser> CategoryUsers { get; set; }
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<Icon> Icons { get; set; }
        public DbSet<Income> Income { get; set; }
        public DbSet<IncomeSource> IncomeSources { get; set; }
        public DbSet<Payee> Payees { get; set; }
        public DbSet<Subcategory> Subcategories { get; set; }
        //public DbSet<ApplicationUser> Users { get; set; }
        public DbSet<Vendor> Vendors { get; set; }
        public DbSet<Budget> Budgets { get; set; }
        public DbSet<Pocket.Models.Event> Events { get; set; }
        public DbSet<Pocket.Models.EventUser> EventUsers { get; set; }

        public DbSet<Friend> Friends { get; set; }
        public DbSet<ExpenseComment> ExpenseComments { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<Pocket.Models.Target> Targets { get; set; }
        public DbSet<AccountTransfer> AccountTransfers { get; set; }
        public DbSet<IdentityUserRole> UserRoles { get; set; }
        public DbSet<ScheduleJob> ScheduleJobs { get; set; }
        public DbSet<CustomerRequest> CustomerRequests { get; set; }
        public DbSet<Saving> Savings { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
            modelBuilder.Conventions.Remove<ManyToManyCascadeDeleteConvention>();
            
            modelBuilder.Entity<Friend>()
                .HasRequired(u => u.UserFriend)
                .WithMany()
                .HasForeignKey(u => u.FriendID);
            base.OnModelCreating(modelBuilder);

        }
        
    }
}