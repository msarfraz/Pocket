namespace Pocket.Migrations
{
    using Microsoft.AspNet.Identity.EntityFramework;
    using Pocket.Models;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using System.Web.Security;

    internal sealed class Configuration : DbMigrationsConfiguration<Pocket.Models.QDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            AutomaticMigrationDataLossAllowed = false;
            
        }

        protected override void Seed(Pocket.Models.QDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //context.People.AddOrUpdate(
            //  p => p.FullName,
            //  new Person { FullName = "Andrew Peters" },
            //  new Person { FullName = "Brice Lambson" },
            //  new Person { FullName = "Rowan Miller" }
            //);
            
            context.Roles.AddOrUpdate(r => r.Name, new IdentityRole("Admin"));
            
            var au = context.Users.Where(u => u.UserName == "Admin").SingleOrDefault();
            if(au != null)
            {
            var ur = new IdentityUserRole {UserId = context.Users.Where(u => u.UserName == "Admin").Select(u => u.Id).SingleOrDefault(), RoleId = context.Roles.Where(r => r.Name == "Admin").Select(r => r.Id).SingleOrDefault() };
                context.UserRoles.AddOrUpdate(ur);

            }

        }
    }
}
