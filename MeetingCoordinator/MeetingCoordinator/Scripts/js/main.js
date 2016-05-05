$(document).ready(function() {
  /* 
   * initialize the calendar
   */
  $('#calendar').fullCalendar({
    header: {
      left: 'prev,next today',
      center: 'title',
      right: 'month,agendaWeek,agendaDay'
    },
    editable: true
  });
  $('#external-events div.external-event').each(function () {
    // create an Event Object (http://arshaw.com/fullcalendar/docs/event_data/Event_Object/)
    // it doesn't need to have a start or end
    debugger;
    var data = $(this).find('span.meeting').get(0);
    var eventObject = {
      id: data.dataset["id"],
      title: $.trim($(this).text()),
      start: data.dataset["start"],
      end: data.dataset["end"]
    };
    $('#calendar').fullCalendar('renderEvent', eventObject);
  });
  /* Hide Default header : coz our bottons look awesome */
  $('.fc-header').hide();
  //Get the current date and display on the tile
  var currentDate = $('#calendar').fullCalendar('getDate');
  $('#calender-current-day').html($.fullCalendar.formatDate(currentDate, "dddd"));
  $('#calender-current-date').html($.fullCalendar.formatDate(currentDate, "MMM yyyy"));
  $('#calender-prev').click(function() {
    $('#calendar').fullCalendar('prev');
    currentDate = $('#calendar').fullCalendar('getDate');
    $('#calender-current-day').html($.fullCalendar.formatDate(currentDate, "dddd"));
    $('#calender-current-date').html($.fullCalendar.formatDate(currentDate, "MMM yyyy"));
  });
  $('#calender-next').click(function() {
    $('#calendar').fullCalendar('next');
    currentDate = $('#calendar').fullCalendar('getDate');
    $('#calender-current-day').html($.fullCalendar.formatDate(currentDate, "dddd"));
    $('#calender-current-date').html($.fullCalendar.formatDate(currentDate, "MMM yyyy"));
  });
  $('#change-view-month').click(function() {
    $('#calendar').fullCalendar('changeView', 'month');
  });
  $('#change-view-week').click(function() {
    $('#calendar').fullCalendar('changeView', 'agendaWeek');
  });
  $('#change-view-day').click(function() {
    $('#calendar').fullCalendar('changeView', 'agendaDay');
  });
  /**
   * Event listener for handling getting data about a meeting from the server and
   * displaying the Edit Meeting modal
   */
  $('.edit-meeting').click(function(e) {
    // TODO: SERVER LOGIC (EDIT MEETING USE CASE)
    $('.modal-body').find('input[name=title]').val(this.dataset.title);
    $(this).closest('.external-event').find('.meeting')[0].dataset.attendees.split(',').forEach(function(val, index) {
      $('select[name=attendees]').find('option[value=' + val + ']').attr('selected', true);
    });
    $('#delete-meeting')[0].dataset.meetingId = this.dataset.id;
    $('#meeting-edit').modal("show");
  });
  /**
   * Event listener for deleting a meeting from an Edit Meeting modal
   */
  $('#delete-meeting').click(function(e) {
    if (confirm("Are you sure you wish to remove this meeting? This cannot be undone!")) {
      // TODO: SERVER LOGIC (EDIT-MEETING USE CASE)
      // Remove the list item on the main page
      $('.meeting[data-id=' + this.dataset.meetingId + ']').closest('.external-event').remove();
      // Hide the modal
      $('#meeting-edit').modal("hide");
    }
  });

  /**
   * Event listener for deleting a meeting from the list view
   */
  $(document).on({
    click: function(e) {
      if (confirm("Are you sure you wish to remove this meeting? This cannot be undone!")) {
        var eventListItem = $(this).closest(".external-event");
        var meetingDetails = $(eventListItem).find("span.meeting").get(0).dataset;
        $.ajax({
          url: "/Home/DeleteMeeting",
          method: "GET",
          data: {
            id: meetingDetails["id"]
          },
          success: function(response) {
            if (response.success) {
              $("#calendar").fullCalendar("removeEvents", meetingDetails["id"]);
              $(eventListItem).remove();
            } else {
              console.error("ERROR DELETING MEETING", meetingDetails["id"], response.error);
              alert("There was an error deleting your meeting. Please try again later");
            }
          }
        });
      }
    }
  }, '.delete-meeting');
  /**
   * Event listener for clicking on a meeting link in the list view
   */
  $(document).on({
    click: function() {
      // Get the ID of this meeting from the data-id attribute
      var id = this.dataset["id"];
      // Fire off a JSON request to get the meeting according to the id
      $.getJSON("/Home/Meeting", {
        id: id
      }, function(response) {
        // empty the modal of previous results
        $.each($(".meeting-detail"), function(index, element) {
          $(element).empty();
        });
        // We need to filter out the attributes we need for Attendees
        var attendees = [];
        response.attendees.forEach(function(attendee) {
          attendees.push(attendee.firstName + " " + attendee.lastName);
        });
        // Update the details for the modal
        $("#detail-attendees").text(attendees.join(", "));
        $("#detail-title").text(response.title);
        $("#detail-description").text(response.description);
        // Use moment.js to parse the times given back by the server into a nice message
        $("#detail-time").text(moment(parseInt(/-?\d+/.exec(response.startTime))).format("MMMM Do YYYY, h:mm:ss a") + " to " + moment(parseInt(/-?\d+/.exec(response.endTime))).format("MMMM Do YYYY, h:mm:ss a"));
        // Show the modal
        $('#meeting-details').modal("show");
      });
    }
  }, ".external-event > span.meeting, .external-event > span.edit-meeting");

  /*
   * Event handler for clicking the "Create Meeting" button
   */
  $('#schedule-meeting-sidebar').click(function(e) {
    var $meeting = $('#meeting-create');
    $meeting.find('input, textarea').each(function(index, element) {
      $(element).val("");
    });
    $meeting.find('option').each(function(index, element) {
      $(element).attr("selected", false);
    });
    $meeting.modal("show");
    //get list of attendees and list of rooms from controller
    $.ajax({
      type: "GET",
      dataType: "json",
      url: "/Home/GetSchedulingData",
      async: true,
      success: function(data, status) {
        var rooms = data['rooms'];
        var attendees = data['attendees'];
        if (rooms) {
          var meeting_room_select = $('#meeting-create').find('select[name=meeting-room]');
          meeting_room_select.empty();
          for (var i = 0; i < rooms.length; i++) {
            meeting_room_select.append('<option value=' + rooms[i].ID + '>' + rooms[i].RoomNo + '</option>');
          }
        }
        if (attendees) {
          var attendee_select = $('#meeting-create').find('select[name=attendees]');
          attendee_select.empty();
          for (var i = 0; i < attendees.length; i++) {
            attendee_select.append('<option value=' + attendees[i].ID + '>' + attendees[i].FirstName + ' ' + attendees[i].LastName + '</option>');
          }
        }
      }
    })
  });
  //after all info is filled in, check if the meeting is valid
  $('#start-time-input, #end-time-input').on({
    change: function(event) {
      var start_time = $('#start-time-input').val();
      var end_time = $('#end-time-input').val();
      //if both start and end time have been filled out
      if (start_time !== "" && end_time !== "") {
        var roomID = $('#meeting-create').find('select[name=meeting-room]').val();
        //TODO: change this to attendee_ids
        var attendees = [];
        //get all selected attendees
        $('#attendees-multiple-select :selected').each(function(i, selected) {
          attendees[i] = $(selected).val();
        });
        //if a room has been chosen and at least one attendee has been chosen 
        if (roomID != "" && attendees.length > 0) {
          //reformat start and end time
          start_time = start_time.replace("T", " ");
          end_time = end_time.replace("T", " ");
          //send filled in data back to controller
          $.ajax({
            data: {
              end_time: end_time,
              start_time: start_time,
              room_id: roomID,
              attendees: attendees
            },
            url: "/Home/CheckAvailability",
            type: "POST",
            async: true,
            dataType: "json",
            success: function(data, status) {
              if (status) {
                if (!data.meetingAvailable) {
                  for (var i = 0; i < data.errors.Data.length; i++) {
                    //TODO: show actual error messages to the user
                    console.log(data.errors.Data[i]);
                  }
                }
              }
            }
          });
        }
      }
    }
  });
  $('#save-new-meeting').click(function(e) {
    var title = $('#meeting-create').find('input[name=title]').val();
    var description = $('#meeting-create').find('textarea[name=description]').val();
    var room_id = $('#meeting-create').find('select[name=meeting-room]').val();
    var attendee_ids = [];
    //get all selected attendee ids
    $('#attendees-multiple-select :selected').each(function(i, selected) {
      attendee_ids[i] = $(selected).val();
    });
    var start_time = $('#start-time-input').val();
    var end_time = $('#end-time-input').val();
    $.ajax({
      data: {
        title: title,
        description: description,
        room_id: room_id,
        attendee_ids: attendee_ids,
        start_time: start_time,
        end_time: end_time
      },
      type: "POST",
      dataType: "json",
      url: "/Home/SaveMeeting",
      async: true,
      success: function(response) {
        if (response.success) {
          alert("Meeting created!");
          $('#external-events').append('<div class="external-event">' + '<span class="meeting" data-title="' + title + '" data-attendees="' + attendee_ids.join(",") + '" data-id="' + response.meeting.id + '">' + title + '</span>' + '<span class="pull-right">' + '<i class="edit-meeting fa fa-pencil"></i>' + '<i style="padding-left: 5px; padding-right: 5px;"></i>' + '<i class="delete-meeting fa fa-times"></i>' + '</span>');
          $('#calendar').fullCalendar('renderEvent', response.meeting);
          $('#meeting-create').modal('hide');
        } else {
          //TODO: show an actual error message and handle it
          console.log("Error! fix this");
        }
      }
    });
  });

  $('#save-meeting-edit').click(function(e) {
    $.ajax({
      data: $('#meeting-edit').find('form').serialize(),
      url: "/Home/Edit"
    });
  });
});