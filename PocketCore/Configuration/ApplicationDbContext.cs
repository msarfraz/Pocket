using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PocketCore.Configuration;
using PocketCore.Models;

namespace Pocket.Models
{
    public class ApplicationDbContext : IdentityDbContext<
        ApplicationUser, ApplicationRole, string,
        IdentityUserClaim<string>, ApplicationUserRole, IdentityUserLogin<string>,
        IdentityRoleClaim<string>, IdentityUserToken<string>>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
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
        public DbSet<ApplicationUserRole> UserRoles { get; set; }
        public DbSet<ScheduleJob> ScheduleJobs { get; set; }
        public DbSet<CustomerRequest> CustomerRequests { get; set; }
        public DbSet<Saving> Savings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
            // modelBuilder.Conventions.Remove<ManyToManyCascadeDeleteConvention>();

            /*modelBuilder.Entity<Friend>()
                .HasRequired(u => u.UserFriend)
                .WithMany()
                .HasForeignKey(u => u.FriendID);
                */
            modelBuilder.Entity<ApplicationUser>(b =>
            {
                // Each User can have many UserClaims
                b.HasMany(e => e.Claims)
                    .WithOne()
                    .HasForeignKey(uc => uc.UserId)
                    .IsRequired();

                // Each User can have many UserLogins
                b.HasMany(e => e.Logins)
                    .WithOne()
                    .HasForeignKey(ul => ul.UserId)
                    .IsRequired();

                // Each User can have many UserTokens
                b.HasMany(e => e.Tokens)
                    .WithOne()
                    .HasForeignKey(ut => ut.UserId)
                    .IsRequired();

                // Each User can have many entries in the UserRole join table
                b.HasMany(e => e.UserRoles)
                    .WithOne(e => e.User)
                    .HasForeignKey(ur => ur.UserId)
                    .IsRequired();
            });

            modelBuilder.Entity<ApplicationRole>(b =>
            {
                // Each Role can have many entries in the UserRole join table
                b.HasMany(e => e.UserRoles)
                    .WithOne(e => e.Role)
                    .HasForeignKey(ur => ur.RoleId)
                    .IsRequired();
            });

            modelBuilder.ApplyConfiguration(new RoleConfiguration());

            base.OnModelCreating(modelBuilder);

        }
        
    }
}