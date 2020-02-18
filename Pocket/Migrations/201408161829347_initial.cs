namespace Pocket.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.accounts",
                c => new
                    {
                        account_id = c.Int(nullable: false, identity: true),
                        user_id = c.String(nullable: false, maxLength: 128),
                        account_name = c.String(nullable: false, maxLength: 500),
                        initial_amount = c.Double(nullable: false),
                        current_amount = c.Double(nullable: false),
                        account_type = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.account_id)
                .ForeignKey("dbo.AspNetUsers", t => t.user_id)
                .Index(t => t.user_id);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Type = c.Int(nullable: false),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.budgets",
                c => new
                    {
                        budget_id = c.Int(nullable: false, identity: true),
                        budget_amount = c.Double(nullable: false),
                        budget_duration = c.Int(nullable: false),
                        budget_type = c.Int(nullable: false),
                        user_id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.budget_id)
                .ForeignKey("dbo.AspNetUsers", t => t.user_id)
                .Index(t => t.user_id);
            
            CreateTable(
                "dbo.categories",
                c => new
                    {
                        category_id = c.Int(nullable: false, identity: true),
                        user_id = c.String(nullable: false, maxLength: 128),
                        category_name = c.String(nullable: false, maxLength: 500),
                        icon_id = c.Int(),
                        budget_amount = c.Double(nullable: false),
                    })
                .PrimaryKey(t => t.category_id)
                .ForeignKey("dbo.icons", t => t.icon_id)
                .ForeignKey("dbo.AspNetUsers", t => t.user_id)
                .Index(t => t.user_id)
                .Index(t => t.icon_id);
            
            CreateTable(
                "dbo.icons",
                c => new
                    {
                        icon_id = c.Int(nullable: false, identity: true),
                        user_id = c.String(nullable: false, maxLength: 128),
                        icon_name = c.String(nullable: false, maxLength: 500),
                    })
                .PrimaryKey(t => t.icon_id)
                .ForeignKey("dbo.AspNetUsers", t => t.user_id)
                .Index(t => t.user_id);
            
            CreateTable(
                "dbo.subcategories",
                c => new
                    {
                        subcategory_id = c.Int(nullable: false, identity: true),
                        category_id = c.Int(nullable: false),
                        subcategory_name = c.String(nullable: false, maxLength: 500),
                        budget_id = c.Int(nullable: false),
                        icon_id = c.Int(),
                    })
                .PrimaryKey(t => t.subcategory_id)
                .ForeignKey("dbo.budgets", t => t.budget_id)
                .ForeignKey("dbo.categories", t => t.category_id)
                .ForeignKey("dbo.icons", t => t.icon_id)
                .Index(t => t.category_id)
                .Index(t => t.budget_id)
                .Index(t => t.icon_id);
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.events",
                c => new
                    {
                        user_id = c.String(nullable: false, maxLength: 128),
                        event_id = c.Int(nullable: false, identity: true),
                        event_name = c.String(nullable: false, maxLength: 500),
                        event_date = c.DateTime(nullable: false),
                        reminder_date = c.DateTime(nullable: false),
                        created_date = c.DateTime(nullable: false),
                        budget_id = c.Int(nullable: false),
                        event_status = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.event_id)
                .ForeignKey("dbo.budgets", t => t.budget_id)
                .ForeignKey("dbo.AspNetUsers", t => t.user_id)
                .Index(t => t.user_id)
                .Index(t => t.budget_id);
            
            CreateTable(
                "dbo.eventuser",
                c => new
                    {
                        user_id = c.String(nullable: false, maxLength: 128),
                        event_id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.user_id, t.event_id })
                .ForeignKey("dbo.events", t => t.event_id)
                .ForeignKey("dbo.AspNetUsers", t => t.user_id)
                .Index(t => t.user_id)
                .Index(t => t.event_id);
            
            CreateTable(
                "dbo.expenses",
                c => new
                    {
                        expense_id = c.Int(nullable: false, identity: true),
                        user_id = c.String(nullable: false, maxLength: 128),
                        account_id = c.Int(nullable: false),
                        subcategory_id = c.Int(nullable: false),
                        payee_id = c.Int(),
                        vendor_id = c.Int(),
                        event_id = c.Int(),
                        amount = c.Double(nullable: false),
                        expense_date = c.DateTime(nullable: false),
                        description = c.String(maxLength: 500),
                        repeat = c.Int(nullable: false),
                        schedule_id = c.Int(),
                        created_date = c.DateTime(nullable: false),
                        modified_date = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.expense_id)
                .ForeignKey("dbo.accounts", t => t.account_id)
                .ForeignKey("dbo.events", t => t.event_id)
                .ForeignKey("dbo.payees", t => t.payee_id)
                .ForeignKey("dbo.schedules", t => t.schedule_id)
                .ForeignKey("dbo.subcategories", t => t.subcategory_id)
                .ForeignKey("dbo.AspNetUsers", t => t.user_id)
                .ForeignKey("dbo.vendors", t => t.vendor_id)
                .Index(t => t.user_id)
                .Index(t => t.account_id)
                .Index(t => t.subcategory_id)
                .Index(t => t.payee_id)
                .Index(t => t.vendor_id)
                .Index(t => t.event_id)
                .Index(t => t.schedule_id);
            
            CreateTable(
                "dbo.expensecomments",
                c => new
                    {
                        comment_id = c.Int(nullable: false, identity: true),
                        user_id = c.String(nullable: false, maxLength: 128),
                        expense_id = c.Int(nullable: false),
                        comment = c.String(nullable: false, maxLength: 500),
                        comment_date = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.comment_id)
                .ForeignKey("dbo.expenses", t => t.expense_id)
                .ForeignKey("dbo.AspNetUsers", t => t.user_id)
                .Index(t => t.user_id)
                .Index(t => t.expense_id);
            
            CreateTable(
                "dbo.payees",
                c => new
                    {
                        payee_id = c.Int(nullable: false, identity: true),
                        user_id = c.String(nullable: false, maxLength: 128),
                        payee_name = c.String(nullable: false, maxLength: 500),
                    })
                .PrimaryKey(t => t.payee_id)
                .ForeignKey("dbo.AspNetUsers", t => t.user_id)
                .Index(t => t.user_id);
            
            CreateTable(
                "dbo.schedules",
                c => new
                    {
                        schdule_id = c.Int(nullable: false, identity: true),
                        user_id = c.String(nullable: false, maxLength: 128),
                        schedule_name = c.String(),
                        last_run = c.DateTime(nullable: false),
                        next_run = c.DateTime(nullable: false),
                        status = c.Int(nullable: false),
                        date_created = c.DateTime(nullable: false),
                        schedule_type = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.schdule_id)
                .ForeignKey("dbo.AspNetUsers", t => t.user_id)
                .Index(t => t.user_id);
            
            CreateTable(
                "dbo.vendors",
                c => new
                    {
                        user_id = c.String(nullable: false, maxLength: 128),
                        vendor_id = c.Int(nullable: false, identity: true),
                        vendor_name = c.String(nullable: false, maxLength: 500),
                    })
                .PrimaryKey(t => t.vendor_id)
                .ForeignKey("dbo.AspNetUsers", t => t.user_id)
                .Index(t => t.user_id);
            
            CreateTable(
                "dbo.friends",
                c => new
                    {
                        user_id = c.String(nullable: false, maxLength: 128),
                        friend_id = c.String(nullable: false, maxLength: 128),
                        Status = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.user_id, t.friend_id })
                .ForeignKey("dbo.AspNetUsers", t => t.friend_id)
                .ForeignKey("dbo.AspNetUsers", t => t.user_id)
                .Index(t => t.user_id)
                .Index(t => t.friend_id);
            
            CreateTable(
                "dbo.income",
                c => new
                    {
                        income_id = c.Int(nullable: false, identity: true),
                        user_id = c.String(nullable: false, maxLength: 128),
                        account_id = c.Int(nullable: false),
                        source_id = c.Int(nullable: false),
                        amount = c.Double(nullable: false),
                        income_date = c.DateTime(nullable: false),
                        description = c.String(maxLength: 500),
                        repeat = c.Int(nullable: false),
                        schedule_id = c.Int(),
                    })
                .PrimaryKey(t => t.income_id)
                .ForeignKey("dbo.accounts", t => t.account_id)
                .ForeignKey("dbo.sources", t => t.source_id)
                .ForeignKey("dbo.schedules", t => t.schedule_id)
                .ForeignKey("dbo.AspNetUsers", t => t.user_id)
                .Index(t => t.user_id)
                .Index(t => t.account_id)
                .Index(t => t.source_id)
                .Index(t => t.schedule_id);
            
            CreateTable(
                "dbo.sources",
                c => new
                    {
                        source_id = c.Int(nullable: false, identity: true),
                        user_id = c.String(nullable: false, maxLength: 128),
                        source_name = c.String(nullable: false, maxLength: 500),
                        icon_id = c.Int(),
                    })
                .PrimaryKey(t => t.source_id)
                .ForeignKey("dbo.icons", t => t.icon_id)
                .ForeignKey("dbo.AspNetUsers", t => t.user_id)
                .Index(t => t.user_id)
                .Index(t => t.icon_id);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.notifications",
                c => new
                    {
                        notification_id = c.Int(nullable: false, identity: true),
                        user_id = c.String(nullable: false, maxLength: 128),
                        notification_text = c.String(nullable: false, maxLength: 500),
                        generated_by = c.String(nullable: false),
                        notification_date = c.DateTime(nullable: false),
                        notification_url = c.String(nullable: false, maxLength: 100),
                        notification_status = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.notification_id)
                .ForeignKey("dbo.AspNetUsers", t => t.user_id)
                .Index(t => t.user_id);
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.targets",
                c => new
                    {
                        target_id = c.Int(nullable: false, identity: true),
                        target_amount = c.Double(nullable: false),
                        expected_date = c.DateTime(nullable: false),
                        target_date = c.DateTime(nullable: false),
                        target_name = c.String(nullable: false, maxLength: 500),
                        target_status = c.Int(nullable: false),
                        user_id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.target_id)
                .ForeignKey("dbo.AspNetUsers", t => t.user_id)
                .Index(t => t.user_id);
            
            CreateTable(
                "dbo.AccountTransfers",
                c => new
                    {
                        transfer_id = c.Int(nullable: false, identity: true),
                        user_id = c.String(nullable: false, maxLength: 128),
                        transfer_date = c.DateTime(nullable: false),
                        description = c.String(maxLength: 500),
                        amount = c.Double(nullable: false),
                        source_account = c.Int(nullable: false),
                        target_account = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.transfer_id)
                .ForeignKey("dbo.accounts", t => t.source_account)
                .ForeignKey("dbo.accounts", t => t.target_account)
                .ForeignKey("dbo.AspNetUsers", t => t.user_id)
                .Index(t => t.user_id)
                .Index(t => t.source_account)
                .Index(t => t.target_account);
            
            CreateTable(
                "dbo.customer_requests",
                c => new
                    {
                        customer_request_id = c.Int(nullable: false, identity: true),
                        user_id = c.String(maxLength: 128),
                        date_created = c.DateTime(nullable: false),
                        request_type = c.Int(nullable: false),
                        customer_name = c.String(),
                        customer_email = c.String(),
                        request_title = c.String(),
                        request_body = c.String(),
                    })
                .PrimaryKey(t => t.customer_request_id)
                .ForeignKey("dbo.AspNetUsers", t => t.user_id)
                .Index(t => t.user_id);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
            CreateTable(
                "dbo.schedule_jobs",
                c => new
                    {
                        schdule_job_id = c.Int(nullable: false, identity: true),
                        user_id = c.String(nullable: false, maxLength: 128),
                        status = c.Int(nullable: false),
                        date_created = c.DateTime(nullable: false),
                        jobs_processed = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.schdule_job_id)
                .ForeignKey("dbo.AspNetUsers", t => t.user_id)
                .Index(t => t.user_id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.schedule_jobs", "user_id", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.customer_requests", "user_id", "dbo.AspNetUsers");
            DropForeignKey("dbo.AccountTransfers", "user_id", "dbo.AspNetUsers");
            DropForeignKey("dbo.AccountTransfers", "target_account", "dbo.accounts");
            DropForeignKey("dbo.AccountTransfers", "source_account", "dbo.accounts");
            DropForeignKey("dbo.accounts", "user_id", "dbo.AspNetUsers");
            DropForeignKey("dbo.targets", "user_id", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.notifications", "user_id", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.income", "user_id", "dbo.AspNetUsers");
            DropForeignKey("dbo.income", "schedule_id", "dbo.schedules");
            DropForeignKey("dbo.income", "source_id", "dbo.sources");
            DropForeignKey("dbo.sources", "user_id", "dbo.AspNetUsers");
            DropForeignKey("dbo.sources", "icon_id", "dbo.icons");
            DropForeignKey("dbo.income", "account_id", "dbo.accounts");
            DropForeignKey("dbo.friends", "user_id", "dbo.AspNetUsers");
            DropForeignKey("dbo.friends", "friend_id", "dbo.AspNetUsers");
            DropForeignKey("dbo.expenses", "vendor_id", "dbo.vendors");
            DropForeignKey("dbo.vendors", "user_id", "dbo.AspNetUsers");
            DropForeignKey("dbo.expenses", "user_id", "dbo.AspNetUsers");
            DropForeignKey("dbo.expenses", "subcategory_id", "dbo.subcategories");
            DropForeignKey("dbo.expenses", "schedule_id", "dbo.schedules");
            DropForeignKey("dbo.schedules", "user_id", "dbo.AspNetUsers");
            DropForeignKey("dbo.expenses", "payee_id", "dbo.payees");
            DropForeignKey("dbo.payees", "user_id", "dbo.AspNetUsers");
            DropForeignKey("dbo.expenses", "event_id", "dbo.events");
            DropForeignKey("dbo.expensecomments", "user_id", "dbo.AspNetUsers");
            DropForeignKey("dbo.expensecomments", "expense_id", "dbo.expenses");
            DropForeignKey("dbo.expenses", "account_id", "dbo.accounts");
            DropForeignKey("dbo.events", "user_id", "dbo.AspNetUsers");
            DropForeignKey("dbo.eventuser", "user_id", "dbo.AspNetUsers");
            DropForeignKey("dbo.eventuser", "event_id", "dbo.events");
            DropForeignKey("dbo.events", "budget_id", "dbo.budgets");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.categories", "user_id", "dbo.AspNetUsers");
            DropForeignKey("dbo.subcategories", "icon_id", "dbo.icons");
            DropForeignKey("dbo.subcategories", "category_id", "dbo.categories");
            DropForeignKey("dbo.subcategories", "budget_id", "dbo.budgets");
            DropForeignKey("dbo.categories", "icon_id", "dbo.icons");
            DropForeignKey("dbo.icons", "user_id", "dbo.AspNetUsers");
            DropForeignKey("dbo.budgets", "user_id", "dbo.AspNetUsers");
            DropIndex("dbo.schedule_jobs", new[] { "user_id" });
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.customer_requests", new[] { "user_id" });
            DropIndex("dbo.AccountTransfers", new[] { "target_account" });
            DropIndex("dbo.AccountTransfers", new[] { "source_account" });
            DropIndex("dbo.AccountTransfers", new[] { "user_id" });
            DropIndex("dbo.targets", new[] { "user_id" });
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.notifications", new[] { "user_id" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.sources", new[] { "icon_id" });
            DropIndex("dbo.sources", new[] { "user_id" });
            DropIndex("dbo.income", new[] { "schedule_id" });
            DropIndex("dbo.income", new[] { "source_id" });
            DropIndex("dbo.income", new[] { "account_id" });
            DropIndex("dbo.income", new[] { "user_id" });
            DropIndex("dbo.friends", new[] { "friend_id" });
            DropIndex("dbo.friends", new[] { "user_id" });
            DropIndex("dbo.vendors", new[] { "user_id" });
            DropIndex("dbo.schedules", new[] { "user_id" });
            DropIndex("dbo.payees", new[] { "user_id" });
            DropIndex("dbo.expensecomments", new[] { "expense_id" });
            DropIndex("dbo.expensecomments", new[] { "user_id" });
            DropIndex("dbo.expenses", new[] { "schedule_id" });
            DropIndex("dbo.expenses", new[] { "event_id" });
            DropIndex("dbo.expenses", new[] { "vendor_id" });
            DropIndex("dbo.expenses", new[] { "payee_id" });
            DropIndex("dbo.expenses", new[] { "subcategory_id" });
            DropIndex("dbo.expenses", new[] { "account_id" });
            DropIndex("dbo.expenses", new[] { "user_id" });
            DropIndex("dbo.eventuser", new[] { "event_id" });
            DropIndex("dbo.eventuser", new[] { "user_id" });
            DropIndex("dbo.events", new[] { "budget_id" });
            DropIndex("dbo.events", new[] { "user_id" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.subcategories", new[] { "icon_id" });
            DropIndex("dbo.subcategories", new[] { "budget_id" });
            DropIndex("dbo.subcategories", new[] { "category_id" });
            DropIndex("dbo.icons", new[] { "user_id" });
            DropIndex("dbo.categories", new[] { "icon_id" });
            DropIndex("dbo.categories", new[] { "user_id" });
            DropIndex("dbo.budgets", new[] { "user_id" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.accounts", new[] { "user_id" });
            DropTable("dbo.schedule_jobs");
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.customer_requests");
            DropTable("dbo.AccountTransfers");
            DropTable("dbo.targets");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.notifications");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.sources");
            DropTable("dbo.income");
            DropTable("dbo.friends");
            DropTable("dbo.vendors");
            DropTable("dbo.schedules");
            DropTable("dbo.payees");
            DropTable("dbo.expensecomments");
            DropTable("dbo.expenses");
            DropTable("dbo.eventuser");
            DropTable("dbo.events");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.subcategories");
            DropTable("dbo.icons");
            DropTable("dbo.categories");
            DropTable("dbo.budgets");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.accounts");
        }
    }
}
