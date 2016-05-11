using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MeetingCoordinator.Models
{
    /// <summary>
    /// All Users in this application are referred to as Attendees
    /// </summary>
    public class Attendee
    {
        /// <summary>
        /// The unique numeric ID of the attendee
        /// </summary>
        [Key]
        public int ID { get; set; }
        /// <summary>
        /// The attendee's real life first name
        /// </summary>
        public string FirstName { get; set; }
        /// <summary>
        /// The attendee's real life last name
        /// </summary>
        public string LastName { get; set; }
        /// <summary>
        /// The attendee's username they will use to log in with
        /// </summary>
        public string Username { get; set; }
        /// <summary>
        /// The password (hashed as a SHA 256 string) will log in with
        /// </summary>
        [DataType(DataType.Password)]
        public string Password { get; set; }
        /// <summary>
        /// Make the meetings an attendee is attending (but not owning)
        /// accessible as a property on this entity
        /// </summary>
        public virtual ICollection<Meeting> AttendingMeetings { get; set; }
        /// <summary>
        /// Make the meetings that the attendee owns accessible
        /// as a property on this meeting
        /// </summary>
        public virtual ICollection<Meeting> OwnMeetings { get; set; } 
    }
}