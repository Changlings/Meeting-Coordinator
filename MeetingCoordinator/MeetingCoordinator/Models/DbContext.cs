using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using MySql.Data.Entity;

namespace MeetingCoordinator.Models
{
    /// <summary>
    /// Configure this DB context to connect to MySql
    /// </summary>
    [DbConfigurationType(typeof(MySqlEFConfiguration))]
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext() : base("DefaultConnection")
        {
        }

        /// <summary>
        /// Register our Meeting model class as a collection of Database Entities
        /// </summary>
        public DbSet<Meeting> Meetings { get; set; }
        /// <summary>
        /// Register our Room model class as a collection of Database Entities
        /// </summary>
        public DbSet<Room> Rooms { get; set; }
        /// <summary>
        /// Register our Attendee model as a collection of Database Entities
        /// </summary>
        public DbSet<Attendee> Attendees { get; set; }

        /// <summary>
        /// This is run when the Update-Database command is executed. Here is where
        /// we'll tell Entity Framework how to relate our models to each other
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // There is a bug in the entity framework with connecting to MySQL. 
            // Some columns do not get their types configured properly (a 700-something long 
            // character string is inserted into an NVARCHAR2(256) column. Yikes!). This
            // solves that rare occurrence.
            modelBuilder.Properties<string>().Configure(c => c.HasColumnType("longtext"));

            // Begin building the entity relations. Entity Framework will manage the
            // pivot tables and foreign key relations for us, but we have to tell it
            // what to relate and how we want it done first.

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