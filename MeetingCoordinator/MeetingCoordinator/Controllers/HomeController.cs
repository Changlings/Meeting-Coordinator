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

        [HttpPost]
        public void Schedule()
        {
            String title;
            String description;
            String startTime;
            String endTime;

            try
            {
                title = Request.Form["title"];
                description = Request.Form["description"];
                startTime = Request.Form["start-time"];
                endTime = Request.Form["end-time"];
            }
            catch (Exception e)
            {
                //exception encountered
            }
        }

        [HttpPost]
        public ActionResult CheckTimes()
        {
            String startDateTimeString;
            String endDateTimeString;

            startDateTimeString = Request.Form["start_time"];
            endDateTimeString = Request.Form["end_time"];

            DateTime startDateTime = DateTime.Parse(startDateTimeString);
            DateTime endDateTime = DateTime.Parse(endDateTimeString);

            List<Room> availableRooms = new List<Room>();
            availableRooms.AddRange(db.Rooms);
            foreach(Meeting m in db.Meetings)
            {
                /*
                NOTE: this does not allow for overlap of meeting times, e.g.
                if one meeting starts at 2:00 and ends at 3:00 and the next starts at 3:00,
                then this will return a conflict. Is this the behavior we want?
                */
                if(startDateTime <= m.EndTime && m.StartTime <= endDateTime)
                {
                    if(availableRooms.Contains(m.HostingRoom))
                    {
                        availableRooms.Remove(m.HostingRoom);
                    }
                }
            }

            var serializer = new JavaScriptSerializer();
            var list = serializer.Serialize(availableRooms);
            JsonResult result = Json(new
            {
                status = true,
                data = new
                {
                    rooms = list
                }
            });

            return result;
        }

    }
}