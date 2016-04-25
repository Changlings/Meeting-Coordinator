namespace MeetingCoordinator.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initial_electric_boogaloo_boogaloo_all_night : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Attendees", "Meeting_ID", c => c.Int());
            AddColumn("dbo.Meetings", "Owner_ID", c => c.Int());
            CreateIndex("dbo.Attendees", "Meeting_ID");
            CreateIndex("dbo.Meetings", "Owner_ID");
            AddForeignKey("dbo.Attendees", "Meeting_ID", "dbo.Meetings", "ID");
            AddForeignKey("dbo.Meetings", "Owner_ID", "dbo.Attendees", "ID");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Meetings", "Owner_ID", "dbo.Attendees");
            DropForeignKey("dbo.Attendees", "Meeting_ID", "dbo.Meetings");
            DropIndex("dbo.Meetings", new[] { "Owner_ID" });
            DropIndex("dbo.Attendees", new[] { "Meeting_ID" });
            DropColumn("dbo.Meetings", "Owner_ID");
            DropColumn("dbo.Attendees", "Meeting_ID");
        }
    }
}
