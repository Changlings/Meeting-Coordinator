using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using MySql.Data.Entity;

namespace MeetingCoordinator.Models
{
  /// <summary>
  /// This is a default ASP configuration file. It assists in managing the application's
  /// sessions and cookies.
  /// </summary>
  //public class ApplicationUser : IdentityUser
  //{
  //  public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
  //  {
  //    var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
  //    return userIdentity;
  //  }
  //}

 // [DbConfigurationType(typeof(MySqlEFConfiguration))]
 // public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
 // {
  //  public ApplicationDbContext()
 //       : base("DefaultConnection", throwIfV1Schema: false)
//    {
//    }

//    public static ApplicationDbContext Create()
  //  {
  //    return new ApplicationDbContext();
  //  }

    //public DbSet<Meeting> Meetings { get; set; }
    //public DbSet<Attendee> Attendees { get; set; }
   // public DbSet<Room> Rooms { get; set; }
  //}
}