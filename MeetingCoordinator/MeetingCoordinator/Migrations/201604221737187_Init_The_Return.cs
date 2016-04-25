namespace MeetingCoordinator.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Init_The_Return : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Meetings", "Title", c => c.String(unicode: false));
            AddColumn("dbo.Meetings", "Description", c => c.String(unicode: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Meetings", "Description");
            DropColumn("dbo.Meetings", "Title");
        }
    }
}
