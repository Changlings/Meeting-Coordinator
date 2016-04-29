using MeetingCoordinator.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using System.Web.Script.Serialization;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;

namespace MeetingCoordinator.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db = new ApplicationDbContext();

        //        [Authorize]
        public ActionResult Index(int year = -1, int month = -1)
        {
            //System.Diagnostics.Debugger.Launch();
            //            int attendeeId = int.Parse(User.Identity.GetUserId());
            // TODO: FIX THE DAMNED OWIN STUFF
            var currentAttendee = _db.Attendees.First(a => a.FirstName == "Wes");
            year = year == -1 ? DateTime.Now.Year : year;
            month = month == -1 ? DateTime.Now.Month : month;
            var thisMonthFirstDay = new DateTime(year, month, 1);
            var nextMonthFirstDay = thisMonthFirstDay.AddMonths(1);
            var ownMeetings = currentAttendee.OwnMeetings.Where(m => m.StartTime >= thisMonthFirstDay && m.EndTime < nextMonthFirstDay).OrderBy(m => m.StartTime).ToList();
            var attendingMeetings = currentAttendee.AttendingMeetings.Where(m => m.StartTime >= thisMonthFirstDay && m.EndTime < nextMonthFirstDay).OrderBy(m => m.StartTime).ToList();
            ViewBag.meetings = ownMeetings.Union(attendingMeetings).ToList();
            ViewBag.ownMeetings = ownMeetings;
            ViewBag.attendingMeetings = attendingMeetings;
            return View();
        }

        [HttpGet]
        public ActionResult Meeting(int id)
        {
            var meetingResult = _db.Meetings.Find(id);
            if (meetingResult == null)
            {
                return Json(new {success = false, error = "No meeting with that ID found"}, JsonRequestBehavior.AllowGet);
            }
            return Json(new
            {
                title = meetingResult.Title,
                description = meetingResult.Description,
                startTime = meetingResult.StartTime,
                endTime = meetingResult.EndTime,
                attendees = meetingResult.Attendees.Select(a => new {id = a.ID, firstName = a.FirstName, lastName = a.LastName}).ToList()
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult EditMeeting()
        {
            var meetingId = int.Parse(Request.Form.Get("meeting-id"));
            var meeting = _db.Meetings.Find(meetingId);

            if (meeting == null)
            {
                return Json(new { success = false, error = "No meeting with that ID found" });
            }
            var title = Request.Form.Get("title");
            var description = Request.Form.Get("description");
            var startTime = Request.Form.Get("start-time");
            var endTime = Request.Form.Get("end-time");
            var attendeeIds = Request.Form.Get("attendees").Split(',').Select(int.Parse).ToList();

            meeting.Title = title;
            meeting.Description = description;
            meeting.EndTime = DateTime.Parse(endTime);
            meeting.StartTime = DateTime.Parse(startTime);
            meeting.Attendees = _db.Attendees.Where(a => attendeeIds.Contains(a.ID)).ToList();
            _db.SaveChangesAsync();

            return Json(new {success = true});
        }

        /*
        *   Populate the room and attendee select forms with all of the possible rooms and attendees. 
        */
        [HttpGet]
        public ActionResult GetSchedulingData()
        {
            var attendeesList = new List<Attendee>();
            var roomsList = new List<Room>();

            //TODO: remove the user actually creating the meeting from the list of attendees
            attendeesList.AddRange(_db.Attendees);
            roomsList.AddRange(_db.Rooms);

            JsonResult result = Json(new
            {
                status = true,
                attendees = attendeesList.Select(a => new
                {
                    ID = a.ID,
                    FirstName = a.FirstName,
                    LastName = a.LastName
                }),

                rooms = roomsList.Select(r => new
                {
                    ID = r.ID,
                    RoomNo = r.RoomNo
                })

            }, JsonRequestBehavior.AllowGet);


            return result;
        }

        /*
        *   
        */
        [HttpPost]
        public ActionResult CheckAvailability()
        {
            var errors = new List<String>();
            bool meetingAvailable = true;

            var startDateTimeString = Request.Form["start_time"];
            var endDateTimeString = Request.Form["end_time"];
            var selectedRoomID = Int32.Parse(Request.Form["room_id"]);

            var attendeeString = Request.Form["attendees[]"];
            var attendeeIDList = (
                from string s in attendeeString.Split(',')
                select Convert.ToInt32(s)
            ).ToList<int>();
            var attendeeList = new List<Attendee>();

            foreach (int i in attendeeIDList)
            {
                Attendee a = _db.Attendees.Find(i);
                attendeeList.Add(a);
            }

            var startDateTime = DateTime.Parse(startDateTimeString);
            var endDateTime = DateTime.Parse(endDateTimeString);

            //if an existing meeting is already using the selected room at the selected time
            Room room = _db.Rooms.First(r => r.ID == selectedRoomID);
            foreach (var m in _db.Meetings.Where(m => startDateTime <= m.EndTime && m.StartTime <= endDateTime).Where(m => selectedRoomID == m.HostingRoom.ID))
            {
                errors.Add("Room " + room.RoomNo + " is already in use during this time.");
                meetingAvailable = false;
            }

            //TODO: check if owner of this meeting is already owner of another meeting or attending another meeting during this time
            //if an attendee is already attending a meeting during the selected time
            var overlappingMeetings = _db.Meetings.Where(m => startDateTime <= m.EndTime && m.StartTime <= endDateTime).ToList();
            foreach (Meeting m in overlappingMeetings)
            {
                if(attendeeList.Count() > 0)
                {
                    //get the attendees that exist both in the selected attendees list and the attendees that are attending a meeting happening during the selected times
                    var overlappingAttendees = m.Attendees.Intersect(attendeeList).ToList();
                    foreach (var a in overlappingAttendees)
                    {
                        errors.Add(a.FirstName + " " + a.LastName + " is already attending a meeting during this time.");
                        attendeeList.Remove(a); //so this error won't be encountered again
                        meetingAvailable = false;
                    }

                }
            }

            if(endDateTime <= startDateTime)
            {
                errors.Add("End date and time must be after start date and time.");
                meetingAvailable = false;
            }

            return Json(new
            {
                status = true,
                meetingAvailable = meetingAvailable,
                errors = Json(errors)
            });
        }

        [HttpPost]
        public ActionResult SaveMeeting()
        {
            Boolean saveSuccessful = false;

            var title = Request.Form.Get("title");
            var description = Request.Form.Get("description");
            var roomId = Int32.Parse(Request.Form.Get("room_id"));

            var attendeeIds = Request.Form.Get("attendee_ids[]");
            var attendeeIdList = (
                from string s in attendeeIds.Split(',')
                select Convert.ToInt32(s)
            ).ToList<int>();

            var startTime = Request.Form.Get("start_time");
            var endTime = Request.Form.Get("end_time");

            var meeting = new Meeting();
            meeting.Title = title;
            meeting.Description = description;
            meeting.EndTime = DateTime.Parse(endTime);
            meeting.StartTime = DateTime.Parse(startTime);
            meeting.Attendees = _db.Attendees.Where(a => attendeeIdList.Contains(a.ID)).ToList();
            meeting.HostingRoom = _db.Rooms.First(r => r.ID == roomId);

            //TODO: GET RID OF HARDCODED OWNER ONCE OWIN STUFF IS FIXED
            meeting.Owner = _db.Attendees.First(a => a.FirstName == "William");

            try
            {
                _db.Meetings.Add(meeting);
                _db.SaveChangesAsync();
            }
            catch (Exception)
            {
                saveSuccessful = false;
            }

            return Json(new { success = saveSuccessful });
        }
    }
}