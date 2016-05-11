using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace MeetingCoordinator.Models
{
    /// <summary>
    /// An entity representing a record in the Meetings table.
    /// Also has properties defined to get the owner, hosting room,
    /// and attendees for a meeting
    /// </summary>
    public class Meeting
    {
        /// <summary>
        /// The unique numeric id for the meeting
        /// </summary>
        [Key]
        public int ID { get; set; }
        /// <summary>
        /// The title of the meeting
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// The description of the meeting
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// The start time (date and time combined) for this meeting.
        /// By defining our constraint here to make it a DateTime type,
        /// C# and whatever database layer we use later will always treat this
        /// data the same
        /// </summary>
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyy-MM-dd hh:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime StartTime { get; set; }
        /// <summary>
        /// The end time (time and date combined) for this meeting.
        /// For technical discussion, see StartTime
        /// </summary>
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyy-MM-dd hh:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime EndTime { get; set; }
       
        /// <summary>
        /// Define the owner Attendee as an accessible property
        /// on this entity.
        /// </summary>
        public virtual Attendee Owner { get; set; }
        /// <summary>
        /// Define the Attendees that will attend this meeting
        /// as a list of attendees.
        /// </summary>
        public virtual ICollection<Attendee> Attendees { get; set; } 

        /// <summary>
        /// Define the hosting room as a property on this model
        /// </summary>
        public virtual Room HostingRoom { get; set; }
    }
}