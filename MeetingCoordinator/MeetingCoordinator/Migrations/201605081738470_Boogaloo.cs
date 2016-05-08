namespace MeetingCoordinator.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Boogaloo : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.AttendeeJoinMeeting", "AttendeeId", "dbo.Attendees");
            DropForeignKey("dbo.AttendeeJoinMeeting", "MeetingId", "dbo.Meetings");
            DropForeignKey("dbo.Meetings", "Owner_ID", "dbo.Attendees");
            DropIndex("dbo.Meetings", new[] { "Owner_ID" });
            DropIndex("dbo.AttendeeJoinMeeting", new[] { "AttendeeId" });
            DropIndex("dbo.AttendeeJoinMeeting", new[] { "MeetingId" });
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        Name = c.String(nullable: false, maxLength: 256, storeType: "nvarchar"),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        RoleId = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        Email = c.String(maxLength: 256, storeType: "nvarchar"),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(unicode: false),
                        SecurityStamp = c.String(unicode: false),
                        PhoneNumber = c.String(unicode: false),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(precision: 0),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256, storeType: "nvarchar"),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        ClaimType = c.String(unicode: false),
                        ClaimValue = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        ProviderKey = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        UserId = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            AddColumn("dbo.Attendees", "Meeting_ID", c => c.Int());
            AddColumn("dbo.Meetings", "Attendee_ID", c => c.Int());
            AddColumn("dbo.Meetings", "Attendee_ID1", c => c.Int());
            AlterColumn("dbo.Meetings", "Owner_ID", c => c.Int());
            CreateIndex("dbo.Attendees", "Meeting_ID");
            CreateIndex("dbo.Meetings", "Owner_ID");
            CreateIndex("dbo.Meetings", "Attendee_ID");
            CreateIndex("dbo.Meetings", "Attendee_ID1");
            AddForeignKey("dbo.Attendees", "Meeting_ID", "dbo.Meetings", "ID");
            AddForeignKey("dbo.Meetings", "Attendee_ID", "dbo.Attendees", "ID");
            AddForeignKey("dbo.Meetings", "Attendee_ID1", "dbo.Attendees", "ID");
            AddForeignKey("dbo.Meetings", "Owner_ID", "dbo.Attendees", "ID");
            DropTable("dbo.AttendeeJoinMeeting");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.AttendeeJoinMeeting",
                c => new
                    {
                        AttendeeId = c.Int(nullable: false),
                        MeetingId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.AttendeeId, t.MeetingId });
            
            DropForeignKey("dbo.Meetings", "Owner_ID", "dbo.Attendees");
            DropForeignKey("dbo.Meetings", "Attendee_ID1", "dbo.Attendees");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.Meetings", "Attendee_ID", "dbo.Attendees");
            DropForeignKey("dbo.Attendees", "Meeting_ID", "dbo.Meetings");
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.Meetings", new[] { "Attendee_ID1" });
            DropIndex("dbo.Meetings", new[] { "Attendee_ID" });
            DropIndex("dbo.Meetings", new[] { "Owner_ID" });
            DropIndex("dbo.Attendees", new[] { "Meeting_ID" });
            AlterColumn("dbo.Meetings", "Owner_ID", c => c.Int(nullable: false));
            DropColumn("dbo.Meetings", "Attendee_ID1");
            DropColumn("dbo.Meetings", "Attendee_ID");
            DropColumn("dbo.Attendees", "Meeting_ID");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetRoles");
            CreateIndex("dbo.AttendeeJoinMeeting", "MeetingId");
            CreateIndex("dbo.AttendeeJoinMeeting", "AttendeeId");
            CreateIndex("dbo.Meetings", "Owner_ID");
            AddForeignKey("dbo.Meetings", "Owner_ID", "dbo.Attendees", "ID", cascadeDelete: true);
            AddForeignKey("dbo.AttendeeJoinMeeting", "MeetingId", "dbo.Meetings", "ID", cascadeDelete: true);
            AddForeignKey("dbo.AttendeeJoinMeeting", "AttendeeId", "dbo.Attendees", "ID", cascadeDelete: true);
        }
    }
}
