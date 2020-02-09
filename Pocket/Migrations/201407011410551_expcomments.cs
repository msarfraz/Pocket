namespace Pocket.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class expcomments : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.expensecomments",
                c => new
                    {
                        comment_id = c.Int(nullable: false, identity: true),
                        user_id = c.Int(nullable: false),
                        expense_id = c.Int(nullable: false),
                        comment = c.String(nullable: false, maxLength: 500),
                    })
                .PrimaryKey(t => t.comment_id)
                .ForeignKey("dbo.expenses", t => t.expense_id)
                .ForeignKey("dbo.users", t => t.user_id)
                .Index(t => t.user_id)
                .Index(t => t.expense_id);
            
            AddColumn("dbo.expenses", "description", c => c.String(maxLength: 500));
            AddColumn("dbo.targets", "target_status", c => c.Int(nullable: false, defaultValue:0));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.expensecomments", "user_id", "dbo.users");
            DropForeignKey("dbo.expensecomments", "expense_id", "dbo.expenses");
            DropIndex("dbo.expensecomments", new[] { "expense_id" });
            DropIndex("dbo.expensecomments", new[] { "user_id" });
            DropColumn("dbo.targets", "target_status");
            DropColumn("dbo.expenses", "description");
            DropTable("dbo.expensecomments");
        }
    }
}
