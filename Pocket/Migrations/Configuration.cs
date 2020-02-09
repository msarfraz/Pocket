namespace Pocket.Migrations
{
    using Pocket.Models;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using System.Web.Security;
    using WebMatrix.WebData;

    internal sealed class Configuration : DbMigrationsConfiguration<Pocket.Models.QDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = false;
            
        }

        protected override void Seed(Pocket.Models.QDbContext context)
        {
            WebMatrix.WebData.WebSecurity.InitializeDatabaseConnection("QDbContext", "Users", "User_Id", "Loginid", autoCreateTables: true);
            var roles = new SimpleRoleProvider(Roles.Provider);

            var membership = new SimpleMembershipProvider(Membership.Provider);

            if (!roles.RoleExists("Admin"))
            {
                roles.CreateRole("Admin");
            }
            if (membership.GetUser("test", false) == null)
            {
                membership.CreateUserAndAccount("test", "test");
            }
            if (!roles.GetRolesForUser("test").Contains("Admin"))
            {
                roles.AddUsersToRoles(new[] { "test" }, new[] { "admin" });
            }
            if (membership.GetUser("joe", false) == null)
            {
                membership.CreateUserAndAccount("joe", "test");
            }
             // This method will be called after migrating to the latest version.

              //You can use the DbSet<T>.AddOrUpdate() helper extension method 
              //to avoid creating duplicate seed data. E.g.
            context.Users.AddOrUpdate(
                u => u.LoginID,
                new User { LoginID = "admin", Email="admin@admin.com", FirstName="admin", LastName="admin"},
                new User { LoginID = "viewer", Email="viewer@viewer.com", FirstName = "Test", LastName = "Viewer"}
                );
            int userId = context.Users.First(u => u.LoginID=="admin").UserID;

            context.Accounts.AddOrUpdate(
                acc => acc.Name,
                new Account { Name="Debit", InitialAmount=0, CurrentAmount=0, UserID=userId},
                new Account { Name = "Debit", InitialAmount = 0, CurrentAmount = 0, UserID = userId }

                );

            context.Payees.AddOrUpdate(
                p => p.Name,
                new Payee { Name = "Mechanic", UserID = userId },
                new Payee { Name = "Doctor", UserID = userId },
                new Payee { Name = "Wife", UserID = userId }

                );

            context.Vendors.AddOrUpdate(
               v => v.Name,
               new Vendor { Name = "Walmart", UserID = userId },
               new Vendor { Name = "Amazon", UserID = userId },
               new Vendor { Name = "Ebay", UserID = userId }

               );

            context.IncomeSources.AddOrUpdate(
               i => i.Name,
               new IncomeSource { Name = "Salary", UserID = userId },
               new IncomeSource { Name = "Bonus", UserID = userId },
               new IncomeSource { Name = "Shares", UserID = userId }

               );

            context.Categories.AddOrUpdate(
               i => i.Name,
               new Category { Name = "Car", UserID = userId, Subcategories = new List<Subcategory>() },
               new Category { Name = "Groceries", UserID = userId, Subcategories = new List<Subcategory>() },
               new Category { Name = "Entertainment", UserID = userId, Subcategories = new List<Subcategory>() }

               );
        }
    }
}
