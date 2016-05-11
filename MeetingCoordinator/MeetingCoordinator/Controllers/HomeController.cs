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
        /// <summary>
        /// Store a constant open connection to the database for the life of the application.
        /// This is okay to do for this application since we're only really using one controller
        /// outside of the authentication process. If this were split up into multiple controllers,
        /// then we would have to figure out a way to share a database context. Existing contexts
        /// tend to not share changes with each other, so there could be some data inconsistency
        /// </summary>
        private readonly ApplicationDbContext _db = new ApplicationDbContext();
        /// <summary>
        /// Store a reference to the current OWIN authntication middleware. This assists in, you 
        /// guessed it, authenticating a user!
        /// </summary>
        private IAuthenticationManager Authentication => HttpContext.GetOwinContext().Authentication;

        /// <summary>
        /// Initialize meetings list, calendar, and other values based on the logged in attendee and return this view.
        /// This method is run every time the Home page is refreshed as well as when the user succesfully logs in
        /// </summary>
        /// <returns>The home page</returns>
        [Authorize]
        public ActionResult Index()
        {
            // Grab the current user's record from the database
            var attendeeID = int.Parse(User.Identity.Name);
            var currentAttendee = _db.Attendees.First(a => a.ID == attendeeID);
            // We need to get all meetings and events for the current month since the 
            // home page will show a calendar and event list with said meetings. To do that,
            // the first day of the current month and the first day of the last month are used
            // in the range checks for fetching from the database
            var thisMonthFirstDay = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var nextMonthFirstDay = thisMonthFirstDay.AddMonths(1); // Can I just say that I love the DateTime library? <3
            /* MySQL equivalent of 
                 SELECT        attendees.*, meetings.*
                   FROM       attendeejoinmeeting INNER JOIN
                   attendees ON attendeejoinmeeting.AttendeeId = attendees.ID INNER JOIN
                   meetings ON attendeejoinmeeting.MeetingId = meetings.ID AND attendees.ID = meetings.Owner_ID
                   WHERE meeting.StartTime >= thisMonthFirstDay AND meeting.EndTime < nextMonthFirstDay
                   ORDER BY m.StartTime ASC;
             */
            var ownMeetings = currentAttendee.OwnMeetings.Where(m => m.StartTime >= thisMonthFirstDay && m.EndTime < nextMonthFirstDay).OrderBy(m => m.StartTime).ToList();
            // Get the current user's attending meetings (not necessarily ones they own)
            var attendingMeetings = currentAttendee.AttendingMeetings.Where(m => m.StartTime >= thisMonthFirstDay && m.EndTime < nextMonthFirstDay).OrderBy(m => m.StartTime).ToList();

            // Attach the username, the user's meetings, their own meetings,
            // and attending meetings to the global ViewBag object for the 
            // Razor templating engine to use while rendering the home page
            // template.
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
        /// <param name="id">The meeting id to use to find the meeting in the database</param>
        /// <returns></returns>
        [HttpGet]
        [Authorize]
        public ActionResult RetrieveMeeting(int id)
        {
            var meetingResult = _db.Meetings.Find(id);

            if (meetingResult == null)
            {
                return Json(new { success = false, error = "No meeting with that ID found" }, JsonRequestBehavior.AllowGet);
            }

            int attendeeID = int.Parse(User.Identity.Name);
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
                allAttendees = _db.Attendees.Where(a => a.ID != attendeeID)
                .Select(a => new
                {
                    ID = a.ID,
                    FirstName = a.FirstName,
                    LastName = a.LastName
                }),
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
        [Authorize]
        public ActionResult GetMeetingsForMonth(DateTime month)
        {
            var attendeeID = int.Parse(User.Identity.Name);
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
                    is_personal_event = m.Attendees.Count == 0 && m.HostingRoom == null,
                    is_own_event = ownMeetings.Contains(m),
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
        /// <returns>JSON with rooms and attendees (excluding the current attendee)</returns>
        [HttpGet]
        [Authorize]
        public ActionResult GetSchedulingData()
        {
            // Get the current user's ID
            int currentAttendeeID = int.Parse(User.Identity.Name);

            // Return all rooms and attendees (not including the current logged in attendee) 
            return Json(new
            {
                status = true,
                attendees = this._db.Attendees.Where(a => a.ID != currentAttendeeID).Select(a => new Attendee
                {
                    ID = a.ID,
                    FirstName = a.FirstName,
                    LastName = a.LastName
                }).ToList(),
                rooms = this._db.Rooms.Select(r => new Room
                {
                    ID = r.ID,
                    RoomNo = r.RoomNo
                }).ToList()

            }, JsonRequestBehavior.AllowGet);
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
        [Authorize]
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
                    if(int.Parse(currentMeetingID) == m.ID)
                    {
                        continue;
                    }
                }

                errors.Add("Room " + room.RoomNo + " is already in use during this time.");
                meetingAvailable = false;
            }

            int currentAttendeeID = int.Parse(User.Identity.Name);
            Attendee currentAttendee = _db.Attendees.First(a => a.ID == currentAttendeeID);
            var overlappingMeetings = _db.Meetings.Where(m => startDateTime <= m.EndTime && m.StartTime <= endDateTime).ToList();
            foreach (Meeting m in overlappingMeetings)
            {
                //don't check the meeting being edited if the user is editing a meeting rather than creating a new one
                if (currentMeetingID != null)
                {
                    //don't check this meeting if currentMeetingID is passed ot this method (aka, if we're editing a meeting instead of creating a new one)
                    if (int.Parse(currentMeetingID) == m.ID)
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
            // Check for boundary condition
            // End times cannot be before start times
            // and the times cannot be equal... that would
            // be a short meeting!
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
        /// an existing meeting if a user is editing a meeting. This also handles personal events.
        /// In the event of updating a meeting, the record itself is not "updated" per se. Because 
        /// of a technical limitation (by design) of Entity framework not updating related entities
        /// when a base entity is changed, it is much easier and straightforward to "update"
        /// a meeting by deleting it and then inserting a new one with the updated data. Entity 
        /// Framework will handle deleting information on related Entities, so in essence this is the 
        /// same as updating, but without having to work around Entity Framework.
        /// </summary>
        /// <returns>
        /// JSON response with members "success" denoting if the operation completed successfully,
        /// "error" containing the error message if there was an error during saving,
        /// and "member" containing the details of the meeting and old meeting that was updated (if that's what happened).
        /// If "success" is false, then "error" will exist. If "success" is true, then "member" will exist. 
        /// </returns>
        [HttpPost]
        [Authorize]
        public ActionResult SaveMeeting()
        {
            // The id of the meeting sent by the client. If this is not null,
            // then it is an existing meeting and will be updating it.
            var id = Request.Form.Get("id");
            var title = Request.Form.Get("title");
            var description = Request.Form.Get("description");
            var startTime = Request.Form.Get("start_time");
            var endTime = Request.Form.Get("end_time");
            // Get the id of the currently logged in user
            var attendeeID = int.Parse(User.Identity.Name);

            // Create a personal event
            if (Request.Form.Get("is_personal_event") != null && Request.Form.Get("is_personal_event") != "false")
            {
                // Create a new Meeting entity first. This is not 
                // inserted into the database (yet)
                var m = new Meeting
                {
                    Title = title,
                    Description = description,
                    StartTime = DateTime.Parse(startTime),
                    EndTime = DateTime.Parse(endTime),
                    Owner = _db.Attendees.First(a => a.ID == attendeeID)
                };
                // Entity framework is being a nuisance. Hacking "updates" to a meeting
                // If we were supplied a meeting id, we're going to "update" it
                if (id != null)
                {
                    // Convert to an integer
                    var lookup_id = int.Parse(id);
                    // Remove the meeting by looking it up by its id and scheduling it for a delete
                    this._db.Meetings.Remove(this._db.Meetings.First(mtng => mtng.ID == lookup_id));
                    // Entity Framework actually saves changes and deletes the meeting
                    this._db.SaveChanges();
                }
                // Add the new Meeting entity to the current DB session
                _db.Meetings.Add(m);
                // And write it to the database
                _db.SaveChanges();
                
                // Event creation was succesful, so send data back to the client
                // so they can update their models and views
                return Json(new
                {
                    success = true,
                    meeting = new
                    {
                        // This will be used by the client to 
                        // update its view
                        oldMeetingID = id,
                        id = m.ID,
                        title = m.Title,
                        start = m.StartTime,
                        end = m.EndTime,
                        is_personal_event = true
                    }
                });
            }

            // Creating actual meetings (with rooms and attendees)
            var roomId = int.Parse(Request.Form.Get("room_id"));

            // This data was passed to us as a comma-separated string
            // and must be converted to a list of integers before passing
            // to Entity Framework
            var attendeeIds = Request.Form.Get("attendee_ids[]");
            var attendeeIdList = (
                from string s in attendeeIds.Split(',')
                select Convert.ToInt32(s)
            ).ToList<int>();

            

            // Create the new Meeting entity before inserting
            var meeting = new Meeting
            {
                Title = title,
                Description = description,
                EndTime = DateTime.Parse(endTime),
                StartTime = DateTime.Parse(startTime),
                // Get actual attendees based on their IDS and attach them to this entity
                Attendees = _db.Attendees.Where(a => attendeeIdList.Contains(a.ID)).ToList(),

                HostingRoom = _db.Rooms.First(r => r.ID == roomId),
                Owner = _db.Attendees.First(a => a.ID == attendeeID)
            };

            //putting this up here so i can use it in the json response
            var oldMeeting = new Meeting();
            oldMeeting.ID = -1;
            try
            {
                // Same thing as with creating a personal event. If we're passed an ID,
                // then we're going to "update" it by removing the old record and creating
                // a new one
                if (id != null)
                {
                    oldMeeting = _db.Meetings.Find(int.Parse(id));
                    this._db.Meetings.Remove(this._db.Meetings.Find(int.Parse(id)));
                }

                this._db.Meetings.Add(meeting);
                this._db.SaveChanges();

                return Json(new
                {
                    success = true,
                    meeting = new
                    {
                        //need to be able to remove the old meeting from the meetings list view by its id
                        oldMeetingID = oldMeeting.ID,
                        id = meeting.ID,
                        title = meeting.Title,
                        start = meeting.StartTime,
                        end = meeting.EndTime
                    }
                });
            }
            catch (Exception e)
            {
                // Catch all exceptions so the server doesn't fail to 
                // respond to this request. Send the error message so
                // someone can debug it later on
                return Json(new { success = false, error = e.Message });
            }
        }

        /// <summary>
        /// Delete a meeting based on its ID. Using a GET request is incredibly
        /// insecure and this would be properly done with a DELETE HTTP verb on a
        /// real production server. However, for demo purposes, this works.
        /// </summary>
        /// <param name="id">The ID of the meeting to delete</param>
        /// <returns></returns>
        [HttpGet]
        [Authorize]
        public ActionResult DeleteMeeting(int id)
        {
            try
            {
                // Get the ID of the currently logged in user
                var currentAttendee = int.Parse(User.Identity.Name);
                // find the meeting (if it exists)
                var meeting = this._db.Meetings.First(m => m.ID == id);
                // Only owners can delete their meetings
                if (meeting.Owner.ID != currentAttendee)
                {
                    // This will be caught by the outside catch statement
                    throw new Exception(@"You do not have privileges to delete this meeting");
                }
                // Schedule the meeting for removal
                this._db.Meetings.Remove(meeting);
                // Entity Framework, delete this entity!
                this._db.SaveChanges();
                // Return a response signifying success
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            // Catch-all Exception block. This will prevent the 
            // server from crashing totally on a failed request
            // for whatever reason. This will also catch the 
            // permission check error if it fails that.
            catch (Exception e)
            {
                return Json(new { success = false, error = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}