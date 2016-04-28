using MeetingCoordinator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace MeetingCoordinator.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        //        [Authorize]
        public ActionResult Index()
        {
            return View();
        }

//        [Authorize]
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

//        [Authorize]
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        /*
        I can't think of a good reason not to go ahead and also load the attendees here.
        Otherwise we would go through all meetings to get available rooms,
        Let the user choose the room (which has no affect on the attendees that can attend at that point)
        then going through all the meetings again to get the attendees. So we get the attendees here
        */
        [HttpPost]
        public ActionResult CheckAvailability()
        {
            String startDateTimeString;
            String endDateTimeString;

            startDateTimeString = Request.Form["start_time"];
            endDateTimeString = Request.Form["end_time"];

            DateTime startDateTime = DateTime.Parse(startDateTimeString);
            DateTime endDateTime = DateTime.Parse(endDateTimeString);

            List<Room> availableRooms = new List<Room>();
            List<Attendee> availableAttendees = new List<Attendee>();
            availableRooms.AddRange(db.Rooms);
            foreach(Meeting m in db.Meetings)
            {
                /*
                NOTE: this does not allow for overlap of meeting times, e.g.
                if one meeting starts at 2:00 and ends at 3:00 and the next starts at 3:00,
                then this will return a conflict. Is this the behavior we want?
                */
                if (startDateTime < endDateTime)
                {
                    if (startDateTime <= m.EndTime && m.StartTime <= endDateTime)
                    {
                        //if the room is taken, remove it from our list of room choices
                        if (availableRooms.Contains(m.HostingRoom))
                        {
                            availableRooms.Remove(m.HostingRoom);
                        }

                        //if an attendee has this meeting in their list of meetings, remove the attendee from our list of attendee choices
                    }
                }


            }

            JsonResult result = Json(new
            {
                status = true,
                rooms = Json(availableRooms)
            });

            return result;
        }
    }
}