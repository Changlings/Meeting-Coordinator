using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using MySql.Data.Entity;

namespace MeetingCoordinator.Models
{
  [DbConfigurationType(typeof(MySqlEFConfiguration))]
  public class ApplicationDbContext : DbContext
  {
    public ApplicationDbContext() : base("DefaultConnection")
    {
    }

    public DbSet<Meeting> Meetings { get; set; }
    public DbSet<Room> Rooms { get; set; }
    public DbSet<Attendee> Attendees { get; set; }

    protected override void OnModelCreating(DbModelBuilder modelBuilder)
    {
      modelBuilder.Properties<String>().Configure(c => c.HasColumnType("longtext"));

      //each meeting has multiple attendees and each attendee has multiple meetings
      modelBuilder.Entity<Attendee>()
        .HasMany<Meeting>(a => a.AttendingMeetings)
        .WithMany(m => m.Attendees)
        .Map(mc =>
        {
          mc.ToTable("AttendeeJoinMeeting");
          mc.MapLeftKey("AttendeeId");
          mc.MapRightKey("MeetingId");
        });

      //each meeting has one owner. but each attendee can own multiple meetings
      modelBuilder.Entity<Meeting>()
        .HasRequired<Attendee>(m => m.Owner)
        .WithMany(a => a.OwnMeetings);

      base.OnModelCreating(modelBuilder);
    }
  }
}