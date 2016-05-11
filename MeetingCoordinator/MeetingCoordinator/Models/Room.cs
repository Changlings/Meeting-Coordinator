using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MeetingCoordinator.Models
{
    /// <summary>
    /// Represents a Room record in the database.
    /// As far as our other Entities go, this is pretty
    /// dumb and straightforward. It does not know of or 
    /// depend on any other entities to exist. It's merely
    /// a repository for information about the room
    /// </summary>
    public class Room
    {
        /// <summary>
        /// The unique numeric ID of the room record
        /// </summary>
        [Key]
        public int ID { get; set; }
        /// <summary>
        /// A string value telling the room number of the room.
        /// Ex: WALL455
        /// </summary>
        public string RoomNo { get; set; }
        /// <summary>
        /// How many people can we cram in here before they start
        /// calling HR?
        /// </summary>
        public int Capacity { get; set; }
    }
}