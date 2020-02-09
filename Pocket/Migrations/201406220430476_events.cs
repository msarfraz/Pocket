namespace Pocket.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class events : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.events", new[] { "budget_id" });
            AddColumn("dbo.budgets", "budget_type", c => c.Int(nullable: false));
            AlterColumn("dbo.events", "event_date", c => c.DateTime(nullable: false));
            AlterColumn("dbo.events", "reminder_date", c => c.DateTime(nullable: false));
            AlterColumn("dbo.events", "budget_id", c => c.Int(nullable: false));
            CreateIndex("dbo.events", "budget_id");
        }
        
        public override void Down()
        {
            DropIndex("dbo.events", new[] { "budget_id" });
            AlterColumn("dbo.events", "budget_id", c => c.Int());
            AlterColumn("dbo.events", "reminder_date", c => c.DateTime());
            AlterColumn("dbo.events", "event_date", c => c.DateTime());
            DropColumn("dbo.budgets", "budget_type");
            CreateIndex("dbo.events", "budget_id");
        }
    }
}
