namespace Pocket.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class first : DbMigration
    {
        public override void Up()
        {
            //RenameTable(name: "dbo.FriendEvents", newName: "users");
            //RenameTable(name: "dbo.friends", newName: "users");
            //DropForeignKey("dbo.FriendEvents", new[] { "Friend_UserID", "Friend_FriendID" }, "dbo.friends");
            //DropForeignKey("dbo.FriendEvents", "Event_EventID", "dbo.events");
            //DropForeignKey("dbo.events", "user_id", "dbo.users");
            //DropForeignKey("dbo.events", "user_id", "dbo.users");
            //DropForeignKey("dbo.friends", "friend_id", "dbo.users");
            //DropIndex("dbo.FriendEvents", new[] { "Friend_UserID", "Friend_FriendID" });
            //DropIndex("dbo.FriendEvents", new[] { "Event_EventID" });
            //DropIndex("dbo.events", new[] { "user_id" });
            //DropIndex("dbo.events", new[] { "user_id" });
            //DropIndex("dbo.friends", new[] { "friend_id" });
            //AddColumn("dbo.users", "Event_EventID", c => c.Int());
            //AddColumn("dbo.users", "EventFriends_EventID", c => c.Int());
            //AddColumn("dbo.users", "User_UserID", c => c.Int());
            //AddColumn("dbo.events", "User_UserID", c => c.Int());
            //AddColumn("dbo.events", "User_UserID1", c => c.Int());
            //CreateIndex("dbo.users", "Event_EventID");
            //CreateIndex("dbo.events", "User_UserID");
            //CreateIndex("dbo.events", "User_UserID1");
            //CreateIndex("dbo.users", "User_UserID");
            //AddForeignKey("dbo.users", "Event_EventID", "dbo.events", "event_id");
            //AddForeignKey("dbo.events", "User_UserID", "dbo.users", "user_id");
            //AddForeignKey("dbo.events", "User_UserID1", "dbo.users", "user_id");
            //AddForeignKey("dbo.users", "User_UserID", "dbo.users", "user_id");
            //DropColumn("dbo.friends", "EventFriends_EventID");
        }
        
        public override void Down()
        {
            //AddColumn("dbo.friends", "EventFriends_EventID", c => c.Int());
            //DropForeignKey("dbo.users", "User_UserID", "dbo.users");
            //DropForeignKey("dbo.events", "User_UserID1", "dbo.users");
            //DropForeignKey("dbo.events", "User_UserID", "dbo.users");
            //DropForeignKey("dbo.users", "Event_EventID", "dbo.events");
            //DropIndex("dbo.users", new[] { "User_UserID" });
            //DropIndex("dbo.events", new[] { "User_UserID1" });
            //DropIndex("dbo.events", new[] { "User_UserID" });
            //DropIndex("dbo.users", new[] { "Event_EventID" });
            //DropColumn("dbo.events", "User_UserID1");
            //DropColumn("dbo.events", "User_UserID");
            //DropColumn("dbo.users", "User_UserID");
            //DropColumn("dbo.users", "EventFriends_EventID");
            //DropColumn("dbo.users", "Event_EventID");
            //CreateIndex("dbo.friends", "friend_id");
            //CreateIndex("dbo.events", "user_id");
            //CreateIndex("dbo.events", "user_id");
            //CreateIndex("dbo.FriendEvents", "Event_EventID");
            //CreateIndex("dbo.FriendEvents", new[] { "Friend_UserID", "Friend_FriendID" });
            //AddForeignKey("dbo.friends", "friend_id", "dbo.users", "user_id");
            //AddForeignKey("dbo.events", "user_id", "dbo.users", "user_id");
            //AddForeignKey("dbo.events", "user_id", "dbo.users", "user_id");
            //AddForeignKey("dbo.FriendEvents", "Event_EventID", "dbo.events", "event_id");
            //AddForeignKey("dbo.FriendEvents", new[] { "Friend_UserID", "Friend_FriendID" }, "dbo.friends", new[] { "user_id", "friend_id" });
            //RenameTable(name: "dbo.users", newName: "friends");
            //RenameTable(name: "dbo.users", newName: "FriendEvents");
        }
    }
}
