using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MeetingCoordinator.Models
{
    public class Room
    {
        [Key]
        public int ID { get; set; }
        public string RoomNo { get; set; }
        public int Capacity { get; set; }
    }
}