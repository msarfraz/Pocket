namespace Pocket.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class recursive : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.subcategories", new[] { "budget_id" });
            AlterColumn("dbo.subcategories", "budget_id", c => c.Int(nullable: false));
            CreateIndex("dbo.subcategories", "budget_id");
        }
        
        public override void Down()
        {
            DropIndex("dbo.subcategories", new[] { "budget_id" });
            AlterColumn("dbo.subcategories", "budget_id", c => c.Int());
            CreateIndex("dbo.subcategories", "budget_id");
        }
    }
}
