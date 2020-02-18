namespace Pocket.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class notification : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.notifications", "notification_title", c => c.String(nullable: false, maxLength: 50));
            AddColumn("dbo.notifications", "notification_mobile_url", c => c.String(nullable: false, maxLength: 100));
        }
        
        public override void Down()
        {
            DropColumn("dbo.notifications", "notification_mobile_url");
            DropColumn("dbo.notifications", "notification_title");
        }
    }
}
