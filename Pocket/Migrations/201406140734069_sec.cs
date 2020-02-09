namespace Pocket.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class sec : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.expenses", "user_id", "dbo.users");
            DropIndex("dbo.expenses", new[] { "user_id" });
            AddColumn("dbo.expenses", "vendor_id", c => c.Int());
            CreateIndex("dbo.expenses", "vendor_id");
            AddForeignKey("dbo.expenses", "vendor_id", "dbo.vendors", "vendor_id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.expenses", "vendor_id", "dbo.vendors");
            DropIndex("dbo.expenses", new[] { "vendor_id" });
            DropColumn("dbo.expenses", "vendor_id");
            CreateIndex("dbo.expenses", "user_id");
            AddForeignKey("dbo.expenses", "user_id", "dbo.users", "user_id");
        }
    }
}
