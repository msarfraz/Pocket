namespace Pocket.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class users : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.AspNetUsers", name: "Type", newName: "user_type");
            RenameColumn(table: "dbo.AspNetUsers", name: "PageSettings", newName: "user_page_settings");
            AddColumn("dbo.AspNetUsers", "email_confirmation_sent", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "email_confirmation_sent");
            RenameColumn(table: "dbo.AspNetUsers", name: "user_page_settings", newName: "PageSettings");
            RenameColumn(table: "dbo.AspNetUsers", name: "user_type", newName: "Type");
        }
    }
}
