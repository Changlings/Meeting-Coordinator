using MeetingCoordinator.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using System.Web.Script.Serialization;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using Microsoft.Owin.Security;

namespace MeetingCoordinator.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db = new ApplicationDbContext();
        private IAuthenticationManager Authentication => HttpContext.GetOwinContext().Authentication;

        /// <summary>
        /// Initialize meetings list, calendar, and other values based on the logged in attendee and return this view
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <returns>The home page</returns>
        [Authorize]
        public ActionResult Index(int year = -1, int month = -1)
        {
            var user = User.Identity;
            var attendeeID = Int32.Parse(User.Identity.Name);
            var currentAttendee = _db.Attendees.First(a => a.ID == attendeeID);

            year = year == -1 ? DateTime.Now.Year : year;
            month = month == -1 ? DateTime.Now.Month : month;
            var thisMonthFirstDay = new DateTime(year, month, 1);
            var nextMonthFirstDay = thisMonthFirstDay.AddMonths(1);
            var ownMeetings = currentAttendee.OwnMeetings.Where(m => m.StartTime >= thisMonthFirstDay && m.EndTime < nextMonthFirstDay).OrderBy(m => m.StartTime).ToList();
            var attendingMeetings = currentAttendee.AttendingMeetings.Where(m => m.StartTime >= thisMonthFirstDay && m.EndTime < nextMonthFirstDay).OrderBy(m => m.StartTime).ToList();

            ViewBag.username = currentAttendee.Username;
            ViewBag.meetings = ownMeetings.Union(attendingMeetings).ToList();
            ViewBag.ownMeetings = ownMeetings;
            ViewBag.attendingMeetings = attendingMeetings;

            return View();
        }

        /// <summary>
        /// Retrieves a Meeting object based on the meeting id passed to it
        /// Will remove the current Attendee from the list of All Attendees
        /// passed to the view from here
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult RetrieveMeeting(int id)
        {
            var meetingResult = _db.Meetings.Find(id);

            if (meetingResult == null)
            {
                return Json(new { success = false, error = "No meeting with that ID found" }, JsonRequestBehavior.AllowGet);
            }

            int attendeeID = Int32.Parse(User.Identity.Name);
            return Json(new
            {
                id = meetingResult.ID,
                title = meetingResult.Title,
                description = meetingResult.Description,
                startTime = meetingResult.StartTime,
                endTime = meetingResult.EndTime,
                selectedRoom = meetingResult.HostingRoom,
                selectedAttendees = meetingResult.Attendees.Select(a => new
                {
                    ID = a.ID,
                    FirstName = a.FirstName,
                    LastName = a.LastName
                }),
                allAttendees = _db.Attendees.Select(a => new
                {
                    ID = a.ID,
                    FirstName = a.FirstName,
                    LastName = a.LastName
                }).Where(a => a.ID != attendeeID),
                allRooms = _db.Rooms.Select(r => new
                {
                    ID = r.ID,
                    RoomNo = r.RoomNo
                })
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Get all meetings for the current Attendee for the selected month
        /// </summary>
        /// <param name="month"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetMeetingsForMonth(DateTime month)
        {
            var attendeeID = Int32.Parse(User.Identity.Name);
            var currentAttendee = _db.Attendees.First(a => a.ID == attendeeID);

            var endTime = month.AddMonths(1);
            var ownMeetings =
              currentAttendee.OwnMeetings.Where(m => m.StartTime >= month && m.EndTime < endTime)
                .OrderBy(m => m.StartTime)
                .ToList();
            var attendingMeetings = currentAttendee.AttendingMeetings.Where(m => m.StartTime >= month && m.EndTime < endTime)
              .OrderBy(m => m.StartTime)
              .ToList();
            var meetings = ownMeetings.Union(attendingMeetings).ToList();

            return Json(new
            {
                meetings = meetings.Select(m => new
                {
                    id = m.ID,
                    title = m.Title,
                    start = m.StartTime,
                    end = m.EndTime,
                    attendees = m.Attendees.Select(a => a.ID)
                })
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Populate the room and attendee select forms with all of the possible rooms and attendees. 
        /// This method will remove the logged in attendee from the list of attendees as the logged in attendee will always be
        /// the Owner Attendee for any meetings that they can edit or create. 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetSchedulingData()
        {
            var attendeesList = new List<Attendee>();
            var roomsList = new List<Room>();

            attendeesList.AddRange(_db.Attendees);
            roomsList.AddRange(_db.Rooms);

            int currentAttendeeID = Int32.Parse(User.Identity.Name);
            Attendee currentAttendee = _db.Attendees.First(a => a.ID == currentAttendeeID);
            attendeesList.Remove(currentAttendee);

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

        /// <summary>
        /// This method will be called whenever a user has completely filled out all of the information
        /// in the meeting they are creating or editing. It will check the validity of a meeting. This validation check
        /// includes checking conflicting or overlapping meeting times, whether or not the chosen attendees
        /// or the chosen room is/are already attending/being used for a meeting. Also contains code to skip checking
        /// the validity of the meeting being edited if a meeting is being edited rather than a new one being created.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CheckAvailability()
        {
            var errors = new List<string>();
            bool meetingAvailable = true;

            var currentMeetingID = Request.Form["meeting_id"];
            var startDateTimeString = Request.Form["start_time"];
            var endDateTimeString = Request.Form["end_time"];
            var selectedRoomId = int.Parse(Request.Form["room_id"]);

            var attendeeString = Request.Form["attendees[]"];
            var attendeeIdList = (
                from string s in attendeeString.Split(',')
                select Convert.ToInt32(s)
            ).ToList<int>();
            var attendeeList = attendeeIdList.Select(i => _db.Attendees.Find(i)).ToList();

            var startDateTime = DateTime.Parse(startDateTimeString);
            var endDateTime = DateTime.Parse(endDateTimeString);

            //if an existing meeting is already using the selected room at the selected time
            Room room = _db.Rooms.First(r => r.ID == selectedRoomId);
            foreach (var m in _db.Meetings.Where(m => startDateTime <= m.EndTime && m.StartTime <= endDateTime).Where(m => selectedRoomId == m.HostingRoom.ID))
            {
                //don't check the meeting being edited if the user is editing a meeting rather than creating a new one
                if(currentMeetingID != null)
                {
                    //don't check this meeting if currentMeetingID is passed ot this method (aka, if we're editing a meeting instead of creating a new one)
                    if(Int32.Parse(currentMeetingID) == m.ID)
                    {
                        continue;
                    }
                }

                errors.Add("Room " + room.RoomNo + " is already in use during this time.");
                meetingAvailable = false;
            }

            int currentAttendeeID = Int32.Parse(User.Identity.Name);
            Attendee currentAttendee = _db.Attendees.First(a => a.ID == currentAttendeeID);
            var overlappingMeetings = _db.Meetings.Where(m => startDateTime <= m.EndTime && m.StartTime <= endDateTime).ToList();
            foreach (Meeting m in overlappingMeetings)
            {
                //don't check the meeting being edited if the user is editing a meeting rather than creating a new one
                if (currentMeetingID != null)
                {
                    //don't check this meeting if currentMeetingID is passed ot this method (aka, if we're editing a meeting instead of creating a new one)
                    if (Int32.Parse(currentMeetingID) == m.ID)
                    {
                        continue;
                    }
                }

                if (attendeeList.Any())
                {
                    //get the attendees that exist both in the selected attendees list and the attendees that are attending a meeting happening during the selected times
                    var overlappingAttendees = m.Attendees.Intersect(attendeeList).ToList();
                    overlappingAttendees.Add(m.Owner); //also add the owner of the other meeting to the overlapping attendees to make sure they also don't have a meeting conflicting with this one
                    foreach (var a in overlappingAttendees)
                    {
                        errors.Add(a.FirstName + " " + a.LastName + " is already attending a meeting during this time.");
                        attendeeList.Remove(a); //so this error won't be encountered again
                        meetingAvailable = false;
                    }

                }

                //make sure that the attendee creating this meeting isn't already attending an overlapping meeting
                if(m.Owner == currentAttendee)
                {
                    errors.Add("You are already attending a meeting during this time.");
                    meetingAvailable = false;
                }
            }

            if (endDateTime <= startDateTime)
            {
                errors.Add("End date and time must be after start date and time.");
                meetingAvailable = false;
            }

            return Json(new
            {
                status = errors.Count == 0,
                meetingAvailable = meetingAvailable,
                errors = Json(errors)
            });
        }

        /// <summary>
        /// Save a meeting. Will save a brand new meeting if user is creating one or will overwrite
        /// an existing meeting if a user is editing a meeting.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SaveMeeting()
        {
            var id = Request.Form.Get("id");
            var title = Request.Form.Get("title");
            var description = Request.Form.Get("description");
            var startTime = Request.Form.Get("start_time");
            var endTime = Request.Form.Get("end_time");

            // Create a personal event
            if (Request.Form.Get("is_personal_event") != null && Request.Form.Get("is_personal_event") != "false")
            {
                var m = new Meeting
                {
                    Title = title,
                    Description = description,
                    StartTime = DateTime.Parse(startTime),
                    EndTime = DateTime.Parse(endTime)
                };
                // Entity framework is being a nuisance. Hacking "updates" to a meeting
                if (id != null)
                {
                    var lookup_id = int.Parse(id);
                    this._db.Meetings.Remove(this._db.Meetings.First(mtng => mtng.ID == lookup_id));
                    this._db.SaveChanges();
                }
                _db.Meetings.Add(m);
                _db.SaveChanges();
                return Json(new
                {
                    success = true,
                    meeting = new
                    {
                        id = m.ID,
                        title = m.Title,
                        start = m.StartTime,
                        end = m.EndTime,
                        is_personal_event = true
                    }
                });
            }

            // Create 
            var roomId = int.Parse(Request.Form.Get("room_id"));

            var attendeeIds = Request.Form.Get("attendee_ids[]");
            var attendeeIdList = (
                from string s in attendeeIds.Split(',')
                select Convert.ToInt32(s)
            ).ToList<int>();

            

            var attendeeID = Int32.Parse(User.Identity.Name);
            var meeting = new Meeting
            {
                Title = title,
                Description = description,
                EndTime = DateTime.Parse(endTime),
                StartTime = DateTime.Parse(startTime),
                Attendees = _db.Attendees.Where(a => attendeeIdList.Contains(a.ID)).ToList(),
                HostingRoom = _db.Rooms.First(r => r.ID == roomId),
                Owner = _db.Attendees.First(a => a.ID == attendeeID)
            };

            try
            {
                if (id != null)
                {
                    this._db.Meetings.Remove(this._db.Meetings.Find(int.Parse(id)));
                }

                this._db.Meetings.Add(meeting);
                this._db.SaveChanges();
                return Json(new
                {
                    success = true,
                    meeting = new
                    {
                        id = meeting.ID,
                        title = meeting.Title,
                        start = meeting.StartTime,
                        end = meeting.EndTime
                    }
                });
            }
            catch (Exception e)
            {
                return Json(new { success = false, error = e.Message });
            }
        }

        // SUPER INSECURE BUT IIS IS REALY STUPID
        // ABOUT DELETE VERBS
        [HttpGet]
        public ActionResult DeleteMeeting(int id)
        {
            try
            {
                this._db.Meetings.Remove(this._db.Meetings.First(m => m.ID == id));
                this._db.SaveChanges();
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { success = false, error = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}