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

            context.Attendees.AddOrUpdate(
                a => a.Username,
                new Models.Attendee { FirstName = "William", LastName = "Curry", Username = "wcc17", Password = "password" },
                new Models.Attendee { FirstName = "Wes", LastName = "Gilleland", Username = "wes", Password = "password1234" },
                new Models.Attendee { FirstName = "Melinda", LastName = "Cundiff", Username = "melinda64", Password = "hotdog" },
                new Models.Attendee { FirstName = "Jeremy", LastName = "Rice", Username = "talnius", Password = "hotdog1234" }
             );
        }
    }
}
