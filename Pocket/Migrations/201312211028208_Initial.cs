namespace Pocket.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.accounts",
                c => new
                    {
                        account_id = c.Int(nullable: false, identity: true),
                        user_id = c.Int(nullable: false),
                        account_name = c.String(nullable: false, maxLength: 500),
                        initial_amount = c.Double(nullable: false),
                        current_amount = c.Double(nullable: false),
                    })
                .PrimaryKey(t => t.account_id)
                .ForeignKey("dbo.users", t => t.user_id)
                .Index(t => t.user_id);
            
            CreateTable(
                "dbo.users",
                c => new
                    {
                        user_id = c.Int(nullable: false, identity: true),
                        login_id = c.String(maxLength: 500),
                        user_pass = c.String(maxLength: 500),
                        secret_key = c.String(maxLength: 500),
                        first_name = c.String(maxLength: 500),
                        last_name = c.String(maxLength: 500),
                        email = c.String(maxLength: 500),
                        user_type = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.user_id);
            
            CreateTable(
                "dbo.categories",
                c => new
                    {
                        category_id = c.Int(nullable: false, identity: true),
                        user_id = c.Int(nullable: false),
                        category_name = c.String(nullable: false, maxLength: 500),
                        icon_id = c.Int(),
                        budget_amount = c.Double(nullable: false),
                    })
                .PrimaryKey(t => t.category_id)
                .ForeignKey("dbo.icons", t => t.icon_id)
                .ForeignKey("dbo.users", t => t.user_id)
                .Index(t => t.icon_id)
                .Index(t => t.user_id);
            
            CreateTable(
                "dbo.icons",
                c => new
                    {
                        icon_id = c.Int(nullable: false, identity: true),
                        user_id = c.Int(nullable: false),
                        icon_name = c.String(nullable: false, maxLength: 500),
                    })
                .PrimaryKey(t => t.icon_id)
                .ForeignKey("dbo.users", t => t.user_id)
                .Index(t => t.user_id);
            
            CreateTable(
                "dbo.subcategories",
                c => new
                    {
                        subcategory_id = c.Int(nullable: false, identity: true),
                        category_id = c.Int(nullable: false),
                        subcategory_name = c.String(nullable: false, maxLength: 500),
                        budget_id = c.Int(),
                        icon_id = c.Int(),
                    })
                .PrimaryKey(t => t.subcategory_id)
                .ForeignKey("dbo.budgets", t => t.budget_id)
                .ForeignKey("dbo.categories", t => t.category_id)
                .ForeignKey("dbo.icons", t => t.icon_id)
                .Index(t => t.budget_id)
                .Index(t => t.category_id)
                .Index(t => t.icon_id);
            
            CreateTable(
                "dbo.budgets",
                c => new
                    {
                        budget_id = c.Int(nullable: false, identity: true),
                        budget_amount = c.Double(nullable: false),
                        budget_duration = c.Int(nullable: false),
                        user_id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.budget_id)
                .ForeignKey("dbo.users", t => t.user_id)
                .Index(t => t.user_id);
            
            CreateTable(
                "dbo.events",
                c => new
                    {
                        user_id = c.Int(nullable: false),
                        event_id = c.Int(nullable: false, identity: true),
                        event_name = c.String(nullable: false, maxLength: 500),
                    })
                .PrimaryKey(t => t.event_id)
                .ForeignKey("dbo.users", t => t.user_id)
                .Index(t => t.user_id);
            
            CreateTable(
                "dbo.friends",
                c => new
                    {
                        friend_id = c.Int(nullable: false, identity: true),
                        user_id = c.Int(nullable: false),
                        friend_name = c.String(nullable: false, maxLength: 500),
                    })
                .PrimaryKey(t => t.friend_id)
                .ForeignKey("dbo.users", t => t.user_id)
                .Index(t => t.user_id);
            
            CreateTable(
                "dbo.expenses",
                c => new
                    {
                        expense_id = c.Int(nullable: false, identity: true),
                        user_id = c.Int(nullable: false),
                        account_id = c.Int(nullable: false),
                        subcategory_id = c.Int(nullable: false),
                        payee_id = c.Int(),
                        event_id = c.Int(),
                        amount = c.Double(nullable: false),
                        expense_date = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.expense_id)
                .ForeignKey("dbo.accounts", t => t.account_id)
                .ForeignKey("dbo.events", t => t.event_id)
                .ForeignKey("dbo.payees", t => t.payee_id)
                .ForeignKey("dbo.subcategories", t => t.subcategory_id)
                .ForeignKey("dbo.users", t => t.user_id)
                .Index(t => t.account_id)
                .Index(t => t.event_id)
                .Index(t => t.payee_id)
                .Index(t => t.subcategory_id)
                .Index(t => t.user_id);
            
            CreateTable(
                "dbo.payees",
                c => new
                    {
                        payee_id = c.Int(nullable: false, identity: true),
                        user_id = c.Int(nullable: false),
                        payee_name = c.String(nullable: false, maxLength: 500),
                    })
                .PrimaryKey(t => t.payee_id)
                .ForeignKey("dbo.users", t => t.user_id)
                .Index(t => t.user_id);
            
            CreateTable(
                "dbo.income",
                c => new
                    {
                        income_id = c.Int(nullable: false, identity: true),
                        user_id = c.Int(nullable: false),
                        account_id = c.Int(nullable: false),
                        source_id = c.Int(nullable: false),
                        amount = c.Double(nullable: false),
                        income_date = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.income_id)
                .ForeignKey("dbo.accounts", t => t.account_id)
                .ForeignKey("dbo.sources", t => t.source_id)
                .ForeignKey("dbo.users", t => t.user_id)
                .Index(t => t.account_id)
                .Index(t => t.source_id)
                .Index(t => t.user_id);
            
            CreateTable(
                "dbo.sources",
                c => new
                    {
                        source_id = c.Int(nullable: false, identity: true),
                        user_id = c.Int(nullable: false),
                        source_name = c.String(nullable: false, maxLength: 500),
                        icon_id = c.Int(),
                    })
                .PrimaryKey(t => t.source_id)
                .ForeignKey("dbo.icons", t => t.icon_id)
                .ForeignKey("dbo.users", t => t.user_id)
                .Index(t => t.icon_id)
                .Index(t => t.user_id);
            
            CreateTable(
                "dbo.vendors",
                c => new
                    {
                        user_id = c.Int(nullable: false),
                        vendor_id = c.Int(nullable: false, identity: true),
                        vendor_name = c.String(nullable: false, maxLength: 500),
                    })
                .PrimaryKey(t => t.vendor_id)
                .ForeignKey("dbo.users", t => t.user_id)
                .Index(t => t.user_id);
            
            CreateTable(
                "dbo.FriendEvents",
                c => new
                    {
                        Friend_FriendID = c.Int(nullable: false),
                        Event_EventID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Friend_FriendID, t.Event_EventID })
                .ForeignKey("dbo.friends", t => t.Friend_FriendID)
                .ForeignKey("dbo.events", t => t.Event_EventID)
                .Index(t => t.Friend_FriendID)
                .Index(t => t.Event_EventID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.accounts", "user_id", "dbo.users");
            DropForeignKey("dbo.vendors", "user_id", "dbo.users");
            DropForeignKey("dbo.income", "user_id", "dbo.users");
            DropForeignKey("dbo.income", "source_id", "dbo.sources");
            DropForeignKey("dbo.sources", "user_id", "dbo.users");
            DropForeignKey("dbo.sources", "icon_id", "dbo.icons");
            DropForeignKey("dbo.income", "account_id", "dbo.accounts");
            DropForeignKey("dbo.expenses", "user_id", "dbo.users");
            DropForeignKey("dbo.expenses", "subcategory_id", "dbo.subcategories");
            DropForeignKey("dbo.expenses", "payee_id", "dbo.payees");
            DropForeignKey("dbo.payees", "user_id", "dbo.users");
            DropForeignKey("dbo.expenses", "event_id", "dbo.events");
            DropForeignKey("dbo.expenses", "account_id", "dbo.accounts");
            DropForeignKey("dbo.events", "user_id", "dbo.users");
            DropForeignKey("dbo.friends", "user_id", "dbo.users");
            DropForeignKey("dbo.FriendEvents", "Event_EventID", "dbo.events");
            DropForeignKey("dbo.FriendEvents", "Friend_FriendID", "dbo.friends");
            DropForeignKey("dbo.categories", "user_id", "dbo.users");
            DropForeignKey("dbo.subcategories", "icon_id", "dbo.icons");
            DropForeignKey("dbo.subcategories", "category_id", "dbo.categories");
            DropForeignKey("dbo.subcategories", "budget_id", "dbo.budgets");
            DropForeignKey("dbo.budgets", "user_id", "dbo.users");
            DropForeignKey("dbo.categories", "icon_id", "dbo.icons");
            DropForeignKey("dbo.icons", "user_id", "dbo.users");
            DropIndex("dbo.accounts", new[] { "user_id" });
            DropIndex("dbo.vendors", new[] { "user_id" });
            DropIndex("dbo.income", new[] { "user_id" });
            DropIndex("dbo.income", new[] { "source_id" });
            DropIndex("dbo.sources", new[] { "user_id" });
            DropIndex("dbo.sources", new[] { "icon_id" });
            DropIndex("dbo.income", new[] { "account_id" });
            DropIndex("dbo.expenses", new[] { "user_id" });
            DropIndex("dbo.expenses", new[] { "subcategory_id" });
            DropIndex("dbo.expenses", new[] { "payee_id" });
            DropIndex("dbo.payees", new[] { "user_id" });
            DropIndex("dbo.expenses", new[] { "event_id" });
            DropIndex("dbo.expenses", new[] { "account_id" });
            DropIndex("dbo.events", new[] { "user_id" });
            DropIndex("dbo.friends", new[] { "user_id" });
            DropIndex("dbo.FriendEvents", new[] { "Event_EventID" });
            DropIndex("dbo.FriendEvents", new[] { "Friend_FriendID" });
            DropIndex("dbo.categories", new[] { "user_id" });
            DropIndex("dbo.subcategories", new[] { "icon_id" });
            DropIndex("dbo.subcategories", new[] { "category_id" });
            DropIndex("dbo.subcategories", new[] { "budget_id" });
            DropIndex("dbo.budgets", new[] { "user_id" });
            DropIndex("dbo.categories", new[] { "icon_id" });
            DropIndex("dbo.icons", new[] { "user_id" });
            DropTable("dbo.FriendEvents");
            DropTable("dbo.vendors");
            DropTable("dbo.sources");
            DropTable("dbo.income");
            DropTable("dbo.payees");
            DropTable("dbo.expenses");
            DropTable("dbo.friends");
            DropTable("dbo.events");
            DropTable("dbo.budgets");
            DropTable("dbo.subcategories");
            DropTable("dbo.icons");
            DropTable("dbo.categories");
            DropTable("dbo.users");
            DropTable("dbo.accounts");
        }
    }
}
