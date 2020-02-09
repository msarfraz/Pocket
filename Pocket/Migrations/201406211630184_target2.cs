namespace Pocket.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class target2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.targets", "target_date", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.targets", "target_date");
        }
    }
}
