namespace MeetingCoordinator.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initial_migration : DbMigration
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
                "dbo.Meetings",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Title = c.String(unicode: false),
                        Description = c.String(unicode: false),
                        StartTime = c.DateTime(nullable: false, precision: 0),
                        EndTime = c.DateTime(nullable: false, precision: 0),
                        HostingRoom_ID = c.Int(),
                        Owner_ID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Rooms", t => t.HostingRoom_ID)
                .ForeignKey("dbo.Attendees", t => t.Owner_ID, cascadeDelete: true)
                .Index(t => t.HostingRoom_ID)
                .Index(t => t.Owner_ID);
            
            CreateTable(
                "dbo.Rooms",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        RoomNo = c.String(unicode: false),
                        Capacity = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.AttendeeJoinMeeting",
                c => new
                    {
                        AttendeeId = c.Int(nullable: false),
                        MeetingId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.AttendeeId, t.MeetingId })
                .ForeignKey("dbo.Attendees", t => t.AttendeeId, cascadeDelete: true)
                .ForeignKey("dbo.Meetings", t => t.MeetingId, cascadeDelete: true)
                .Index(t => t.AttendeeId)
                .Index(t => t.MeetingId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AttendeeJoinMeeting", "MeetingId", "dbo.Meetings");
            DropForeignKey("dbo.AttendeeJoinMeeting", "AttendeeId", "dbo.Attendees");
            DropForeignKey("dbo.Meetings", "Owner_ID", "dbo.Attendees");
            DropForeignKey("dbo.Meetings", "HostingRoom_ID", "dbo.Rooms");
            DropIndex("dbo.AttendeeJoinMeeting", new[] { "MeetingId" });
            DropIndex("dbo.AttendeeJoinMeeting", new[] { "AttendeeId" });
            DropIndex("dbo.Meetings", new[] { "Owner_ID" });
            DropIndex("dbo.Meetings", new[] { "HostingRoom_ID" });
            DropTable("dbo.AttendeeJoinMeeting");
            DropTable("dbo.Rooms");
            DropTable("dbo.Meetings");
            DropTable("dbo.Attendees");
        }
    }
}
