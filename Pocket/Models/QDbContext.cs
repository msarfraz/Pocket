using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Web;

namespace Pocket.Models
{
    public class QDbContext:DbContext
    {
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<Icon> Icons { get; set; }
        public DbSet<Income> Income { get; set; }
        public DbSet<IncomeSource> IncomeSources { get; set; }
        public DbSet<Payee> Payees { get; set; }
        public DbSet<Subcategory> Subcategories { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Vendor> Vendors { get; set; }
        public DbSet<Budget> Budgets { get; set; }
        public DbSet<Pocket.Models.Event> Events { get; set; }
        public DbSet<Pocket.Models.EventUser> EventUsers { get; set; }

        public DbSet<Friend> Friends { get; set; }
        public DbSet<ExpenseComment> ExpenseComments { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Schedule> Schedules { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
            modelBuilder.Conventions.Remove<ManyToManyCascadeDeleteConvention>();
            
            //modelBuilder.Entity<Friend>()
            //.HasRequired(r => r.UserFriend)
            //.WithMany();

           // modelBuilder.Entity<Friend>()
             //   .HasRequired(r => r.User)
               // .WithMany();

    //        modelBuilder.Entity<Friend>()
    //.HasRequired(u => u.User) 
    //.WithMany()
    //.HasForeignKey(u => u.UserID);

            modelBuilder.Entity<Friend>()
                .HasRequired(u => u.UserFriend)
                .WithMany()
                .HasForeignKey(u => u.FriendID);

           // modelBuilder.Entity<User>()
           //.HasMany(t => t.Users)
           //.WithMany(t => t.Friends)
           //.Map(mc =>
           //{
           //    mc.ToTable("friends");
           //    mc.MapLeftKey("FriendId");
           //    mc.MapRightKey("UserId");
           //});
           // modelBuilder.Entity<Friend>().ToTable("friends");
            //modelBuilder.Entity<User>()
            //    .HasMany(x => x.Friends)
            //    .WithMany();
       //     modelBuilder.Entity<Friend>()
       //.HasKey(c => new { c.UserID, c.FriendID });

       //     modelBuilder.Entity<Friend>()
       //.HasMany(c => c.Users)
       //.WithRequired()
       //.HasForeignKey(c => c.UserID);

       //     modelBuilder.Entity<Friend>()
       //         .HasMany(c => c.UserFriends)
       //         .WithRequired()
       //         .HasForeignKey(c => c);  
            
            base.OnModelCreating(modelBuilder);

            

            //modelBuilder.Entity<Expense>()
              //  .HasRequired(a => a.Subcategory).WithRequiredDependent().WillCascadeOnDelete(false);
                
            /*.HasOptional(a => a.Subcategory)
                .WithOptionalDependent()
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Expense>()
                .HasOptional(a => a.User)
                .WithOptionalDependent()
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Expense>()
                .HasOptional(a => a.Account)
                .WithOptionalDependent()
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Income>()
                .HasOptional(a => a.IncomeSource)
                .WithOptionalDependent()
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Income>()
                .HasOptional(a => a.User)
                .WithOptionalDependent()
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Income>()
                .HasOptional(a => a.Account)
                .WithOptionalDependent()
                .WillCascadeOnDelete(false);*/
        }

        public System.Data.Entity.DbSet<Pocket.Models.Target> Targets { get; set; }

        

        
    }
}