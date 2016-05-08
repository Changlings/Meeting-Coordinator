using System.Collections.Generic;
using MeetingCoordinator.Models;
using MySql.Data.Entity;

namespace MeetingCoordinator.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<MeetingCoordinator.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(MeetingCoordinator.Models.ApplicationDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //

            //passwords before hash =
            //wcc17 - password
            //wes - password1234
            //melinda64 - hotdog
            //talnius - hotdog1234
            //drchang - theteacher
            context.Attendees.AddOrUpdate(
                a => a.Username,
                new Models.Attendee { FirstName = "William", LastName = "Curry", Username = "wcc17", Password = "5e884898da28047151d0e56f8dc6292773603d0d6aabbdd62a11ef721d1542d8" },
                new Models.Attendee { FirstName = "Wes", LastName = "Gilleland", Username = "wes", Password = "b9c950640e1b3740e98acb93e669c65766f6670dd1609ba91ff41052ba48c6f3" },
                new Models.Attendee { FirstName = "Melinda", LastName = "Cundiff", Username = "melinda64", Password = "35602208e86ac7d6b3a63780a9538a9d1763a646d5b9f3930a0548e0983e0ca6" },
                new Models.Attendee { FirstName = "Jeremy", LastName = "Rice", Username = "talnius", Password = "4a969b0fb995b002a31f7f50742e6c83687fd9d67aa2c11f9eb88caaeb05c783" },
                new Models.Attendee { FirstName = "Kuangnan", LastName = "Chang", Username = "drchang", Password = "9e251b9b45ffa7a700e62665b09e833582045bbbee4f069527fae2706ff32dfb" }
             );

            context.Rooms.AddOrUpdate(
                r => r.RoomNo,
                new Models.Room { Capacity = 20, RoomNo = "WALL445" },
                new Models.Room { Capacity = 25, RoomNo = "WALL455" },
                new Models.Room { Capacity = 23, RoomNo = "WALL325" },
                new Models.Room { Capacity = 30, RoomNo = "COMB329" }
            );

            context.SaveChanges();

            Attendee owner = context.Attendees.First(u => u.FirstName == "Wes");

            Attendee[] attendees = new Attendee[]
            {
                    context.Attendees.First(a => a.FirstName == "William"),
                    context.Attendees.First(a => a.FirstName == "Melinda")
            };

            context.Meetings.AddOrUpdate(
                m => m.ID,
                new Models.Meeting
                {
                    Attendees = attendees,
                    Description = "This is a test meeting!",
                    EndTime = new DateTime(2016, 4, 30, 12, 30, 0),
                    StartTime = new DateTime(2016, 4, 30, 12, 0, 0),
                    HostingRoom = context.Rooms.First(r => r.RoomNo == "WALL445"),
                    Owner = owner,
                    Title = "Test Case: Please Ignore"
                }
             );
        }
    }
}
