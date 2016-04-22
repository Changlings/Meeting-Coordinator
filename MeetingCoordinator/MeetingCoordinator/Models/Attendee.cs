using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MeetingCoordinator.Models
{
    public class Attendee
    {
        [Key]
        public int ID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public virtual ICollection<Meeting> AttendingMeetings { get; set; }

        public virtual ICollection<Meeting> OwnMeetings { get; set; } 
    }
}