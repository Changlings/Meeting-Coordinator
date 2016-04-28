﻿using MeetingCoordinator.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using System.Web.Script.Serialization;
using Microsoft.Ajax.Utilities;

namespace MeetingCoordinator.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db = new ApplicationDbContext();

        //        [Authorize]
        public ActionResult Index(int year = -1, int month = -1)
        {    
//            System.Diagnostics.Debugger.Launch();
//            int attendeeId = int.Parse(User.Identity.GetUserId());
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
        public ActionResult EditMeeting(int id)
        {
            var meetingResult = _db.Meetings.First(meeting => meeting.ID == id);
            if (meetingResult == null)
            {
                return Json(new {success = false, error = "No meeting with that ID found"});
            }
            return Json(meetingResult);
        }

        [HttpPost]
        public ActionResult EditMeetingSubmit()
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
            var startDateTimeString = Request.Form["start_time"];
            var endDateTimeString = Request.Form["end_time"];

            var startDateTime = DateTime.Parse(startDateTimeString);
            var endDateTime = DateTime.Parse(endDateTimeString);

            var availableRooms = new List<Room>();
            availableRooms.AddRange(_db.Rooms);
            foreach (var m in _db.Meetings.Where(m => startDateTime <= m.EndTime && m.StartTime <= endDateTime).Where(m => availableRooms.Contains(m.HostingRoom)))
            {
                availableRooms.Remove(m.HostingRoom);
            }

            var serializer = new JavaScriptSerializer();
            var list = serializer.Serialize(availableRooms);
            return Json(new {status = true, data = new { rooms = list } });
        }

    }
}