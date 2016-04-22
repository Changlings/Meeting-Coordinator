using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace MeetingCoordinator.Models
{
    public class Meeting
    {
        [Key]
        public int ID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyy-MM-dd hh:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime StartTime { get; set; }
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyy-MM-dd hh:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime EndTime { get; set; }
       
        public virtual Attendee Owner { get; set; }
        public virtual ICollection<Attendee> Attendees { get; set; } 

        public virtual Room HostingRoom { get; set; }
    }
}