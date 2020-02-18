namespace Pocket.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class sharedAccountCategory : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.account_user",
                c => new
                    {
                        user_id = c.String(nullable: false, maxLength: 128),
                        account_id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.user_id, t.account_id })
                .ForeignKey("dbo.accounts", t => t.account_id)
                .ForeignKey("dbo.AspNetUsers", t => t.user_id)
                .Index(t => t.user_id)
                .Index(t => t.account_id);
            
            CreateTable(
                "dbo.category_user",
                c => new
                    {
                        user_id = c.String(nullable: false, maxLength: 128),
                        category_id = c.Int(nullable: false),
                        display = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.user_id, t.category_id })
                .ForeignKey("dbo.categories", t => t.category_id)
                .ForeignKey("dbo.AspNetUsers", t => t.user_id)
                .Index(t => t.user_id)
                .Index(t => t.category_id);
            
            AddColumn("dbo.AspNetUsers", "PageSettings", c => c.String());
            AddColumn("dbo.categories", "display", c => c.Int(nullable: false));
            AddColumn("dbo.events", "amount", c => c.Double(nullable: false));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.account_user", "user_id", "dbo.AspNetUsers");
            DropForeignKey("dbo.category_user", "user_id", "dbo.AspNetUsers");
            DropForeignKey("dbo.category_user", "category_id", "dbo.categories");
            DropForeignKey("dbo.account_user", "account_id", "dbo.accounts");
            DropIndex("dbo.category_user", new[] { "category_id" });
            DropIndex("dbo.category_user", new[] { "user_id" });
            DropIndex("dbo.account_user", new[] { "account_id" });
            DropIndex("dbo.account_user", new[] { "user_id" });
            DropColumn("dbo.events", "amount");
            DropColumn("dbo.categories", "display");
            DropColumn("dbo.AspNetUsers", "PageSettings");
            DropTable("dbo.category_user");
            DropTable("dbo.account_user");
        }
    }
}
