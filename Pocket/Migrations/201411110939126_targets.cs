namespace Pocket.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class targets : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.savings",
                c => new
                    {
                        saving_id = c.Int(nullable: false, identity: true),
                        amount = c.Double(nullable: false),
                        user_id = c.String(nullable: false, maxLength: 128),
                        target_id = c.Int(nullable: false),
                        account_id = c.Int(nullable: false),
                        saving_date = c.DateTime(nullable: false),
                        created_date = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.saving_id)
                .ForeignKey("dbo.accounts", t => t.account_id)
                .ForeignKey("dbo.targets", t => t.target_id)
                .ForeignKey("dbo.AspNetUsers", t => t.user_id)
                .Index(t => t.user_id)
                .Index(t => t.target_id)
                .Index(t => t.account_id);
            
            AddColumn("dbo.events", "budgeted", c => c.Int(nullable: false));
            AddColumn("dbo.targets", "initial_amount", c => c.Double(nullable: false));
            AddColumn("dbo.targets", "created_date", c => c.DateTime(nullable: false));
            AddColumn("dbo.targets", "budget_id", c => c.Int(nullable: false));
            AddColumn("dbo.targets", "budgeted", c => c.Int(nullable: false));
            CreateIndex("dbo.targets", "budget_id");
            AddForeignKey("dbo.targets", "budget_id", "dbo.budgets", "budget_id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.savings", "user_id", "dbo.AspNetUsers");
            DropForeignKey("dbo.savings", "target_id", "dbo.targets");
            DropForeignKey("dbo.savings", "account_id", "dbo.accounts");
            DropForeignKey("dbo.targets", "budget_id", "dbo.budgets");
            DropIndex("dbo.savings", new[] { "account_id" });
            DropIndex("dbo.savings", new[] { "target_id" });
            DropIndex("dbo.savings", new[] { "user_id" });
            DropIndex("dbo.targets", new[] { "budget_id" });
            DropColumn("dbo.targets", "budgeted");
            DropColumn("dbo.targets", "budget_id");
            DropColumn("dbo.targets", "created_date");
            DropColumn("dbo.targets", "initial_amount");
            DropColumn("dbo.events", "budgeted");
            DropTable("dbo.savings");
        }
    }
}
