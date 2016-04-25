namespace MeetingCoordinator.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initial_electric_boogaloo : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Attendees",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        FirstName = c.String(unicode: false),
                        LastName = c.String(unicode: false),
                        Username = c.String(unicode: false),
                        Password = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.Rooms",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        RoomNo = c.String(unicode: false),
                        Capacity = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            AddColumn("dbo.Meetings", "HostingRoom_ID", c => c.Int());
            AddColumn("dbo.Meetings", "Attendee_ID", c => c.Int());
            CreateIndex("dbo.Meetings", "HostingRoom_ID");
            CreateIndex("dbo.Meetings", "Attendee_ID");
            AddForeignKey("dbo.Meetings", "HostingRoom_ID", "dbo.Rooms", "ID");
            AddForeignKey("dbo.Meetings", "Attendee_ID", "dbo.Attendees", "ID");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Meetings", "Attendee_ID", "dbo.Attendees");
            DropForeignKey("dbo.Meetings", "HostingRoom_ID", "dbo.Rooms");
            DropIndex("dbo.Meetings", new[] { "Attendee_ID" });
            DropIndex("dbo.Meetings", new[] { "HostingRoom_ID" });
            DropColumn("dbo.Meetings", "Attendee_ID");
            DropColumn("dbo.Meetings", "HostingRoom_ID");
            DropTable("dbo.Rooms");
            DropTable("dbo.Attendees");
        }
    }
}
