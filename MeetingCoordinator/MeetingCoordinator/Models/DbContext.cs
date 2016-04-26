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
        public ApplicationDbContext() : base("DefaultConnection") { }
        public DbSet<Meeting> Meetings { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Attendee> Attendees { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Properties<String>().Configure(c => c.HasColumnType("longtext"));
//            modelBuilder.Entity<Attendee>().HasMany(x => x.AttendingMeetings).WithRequired().HasForeignKey(x => x.ID);
//            modelBuilder.Entity<Attendee>().HasMany(x => x.OwnMeetings).WithRequired().HasForeignKey(x => x.ID);
            base.OnModelCreating(modelBuilder);
        }
    }
}