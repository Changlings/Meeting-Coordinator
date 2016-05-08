$(document).ready(function () {

    function updateCalendar() {
        currentDate = $("#calendar").fullCalendar("getDate");
        $.getJSON("/Home/GetMeetingsForMonth/", { month: currentDate.toISOString() }, function (response) {
            console.log(response);
            $(".external-event").remove();
            for (var i = 0; i < response.meetings.length; i++) {
                var start = moment(+/\/Date\((\d*)\)\//.exec(response.meetings[i].start)[1]);
                var end = moment(+/\/Date\((\d*)\)\//.exec(response.meetings[i].end)[1]);
                var title = response.meetings[i].title;
                var attendees = response.meetings[i].attendees.join(",");
                var id = response.meetings[i].id;
                $("#external-events").append('<div class="external-event">' +
                  '<span class="meeting" data-title="' + title + '" data-attendees="' + attendees + '" data-id="' + id + '" data-start="' + start.toISOString() + '" data-end="' + end.toISOString() + '" data-is-personal-event="'+attendees.length > 0+'">' +
                  title +
                  '</span>' +
                  '<span class="pull-right">' +
                  '<i class="edit-meeting fa fa-pencil"></i>' +
                  '<i style="padding-left: 5px; padding-right: 5px;"></i>' +
                  '<i class="delete-meeting fa fa-times"></i>' +
                  '</span>' +
                  '</div>');
                $("#calendar").fullCalendar('renderEvent', {
                    id: id,
                    start: start,
                    end: end,
                    title: title
                });
            }
        });

        $("#calender-current-day").html($.fullCalendar.formatDate(currentDate, "dddd"));

        $("#calender-current-date").html($.fullCalendar.formatDate(currentDate, "MMM yyyy"));
    }
    /* 
     * initialize the calendar
     */
    $("#calendar").fullCalendar({
        header: {
            left: "prev,next today",
            center: "title",
            right: "month,agendaWeek,agendaDay"
        },
        editable: true
    });

    $('#external-events div.external-event').each(function () {
        // create an Event Object (http://arshaw.com/fullcalendar/docs/event_data/Event_Object/)
        // it doesn't need to have a start or end
        var data = $(this).find("span.meeting").get(0);
        var eventObject = {
            id: data.dataset["id"],
            title: $.trim($(this).text()),
            start: data.dataset["start"],
            end: data.dataset["end"]
        };

        $("#calendar").fullCalendar("renderEvent", eventObject);
    });
    /* Hide Default header : coz our bottons look awesome */
    $(".fc-header").hide();
    //Get the current date and display on the tile
    var currentDate = $('#calendar').fullCalendar('getDate');

    $("#calender-current-day").html($.fullCalendar.formatDate(currentDate, "dddd"));
    $("#calender-current-date").html($.fullCalendar.formatDate(currentDate, "MMM yyyy"));

    $("#calender-prev").click(function () {

        $("#calendar").fullCalendar("prev");
        updateCalendar();
    });
    $('#calender-next').click(function () {
        $('#calendar').fullCalendar('next');
        updateCalendar();
    });
    $('#change-view-month').click(function () {
        $('#calendar').fullCalendar('changeView', 'month');
    });
    $('#change-view-week').click(function () {
        $('#calendar').fullCalendar('changeView', 'agendaWeek');
    });
    $('#change-view-day').click(function () {
        $('#calendar').fullCalendar('changeView', 'agendaDay');
    });
    /**
     * Event listener for handling getting data about a meeting from the server and
     * displaying the Edit Meeting modal
     */
    $('.edit-meeting').click(function (e) {
        var meeting_thing = $(this).closest(".external-event").find(".meeting")[0];
        var meetingId = $(this).closest(".external-event").find(".meeting")[0].dataset["id"];
        // Personal events don't have rooms or attendees, so hide the relative inputs
        if (meeting_thing.dataset['is-personal-event'] == "true") {
            $('#meeting-edit').find('select[name=meeting-room]').closest("form-group").css("display", "none");
            $('#meeting-edit').find('select[name=attendees]').closest("form-group").css("display", "none");
        } else {
            $('#meeting-edit').find('select[name=meeting-room]').closest("form-group").css("display", "block");
            $('#meeting-edit').find('select[name=attendees]').closest("form-group").css("display", "block");
        }

        $.ajax({
            type: "GET",
            dataType: "json",
            data: {
                id: meetingId
            },
            url: "/Home/RetrieveMeeting",
            async: true,
            success: function (data) {
                var id = data['id'];
                var title = data['title'];
                var description = data['description'];
                var startTime = data['startTime'];
                var endTime = data['endTime'];
                var selectedAttendees = data['selectedAttendees'];
                var selectedRoom = data['selectedRoom'];
                var allRooms = data['allRooms'];
                var allAttendees = data['allAttendees'];

                //C# returns dates in seconds since Epoch format. strip uneccesary characters and convert to javascript date
                startTime = new Date(parseInt(startTime.substr(6)));
                endTime = new Date(parseInt(endTime.substr(6)));

                //datetime-local inputs want an ISO formatted string so here we go

                //Adjustzero offset times to a relevant time zone
                var timeZoneOffset = startTime.getTimezoneOffset() * 60 * 1000;

                // Subtract the time zone offset from the current UTC date, and pass
                //  that into the Date constructor to get a date whose UTC date/time is
                //  adjusted by timezoneOffset for display purposes.
                startTime = new Date(startTime.getTime() - timeZoneOffset);
                endTime = new Date(endTime.getTime() - timeZoneOffset);

                // Get date's ISO date string and remove the Z.
                var startTimeString = startTime.toISOString().replace('Z', '');
                var endTimeString = endTime.toISOString().replace('Z', '');

                if (id) {
                    var id_input = $('#meeting-edit').find('input[name=id]');
                    id_input.val(id);
                }
                if (title) {
                    var title_input = $('#meeting-edit').find('input[name=title]');
                    title_input.val(title);
                }
                if (description) {
                    var description_input = $('#meeting-edit').find('textarea[name=description]');
                    description_input.empty();
                    description_input.append(description);
                }
                if (startTime) {
                    var startTime_input = $('#meeting-edit').find('input[name=start-time]');
                    startTime_input.val(startTimeString);
                }
                if (endTime) {
                    var endTime_input = $('#meeting-edit').find('input[name=end-time]');
                    endTime_input.val(endTimeString);
                }
                if (allRooms) {
                    var meeting_room_select = $('#meeting-edit').find('select[name=meeting-room]');
                    meeting_room_select.empty();
                    for (var i = 0; i < allRooms.length; i++) {
                        if (allRooms[i].ID == selectedRoom.ID) {
                            meeting_room_select.append('<option value="' + allRooms[i].ID + '" selected>' + allRooms[i].RoomNo + '</option>');
                        } else {
                            meeting_room_select.append('<option value="' + allRooms[i].ID + '">' + allRooms[i].RoomNo + '</option>');
                        }
                    }
                }
                if (allAttendees) {
                    var attendee_select = $('#meeting-edit').find('select[name=attendees]');
                    attendee_select.empty();
                    for (var i = 0; i < allAttendees.length; i++) {
                        //containsAttendee returns -1 if value is not found in array, otherwise returns the index value
                        if (containsAttendee(selectedAttendees, allAttendees[i]) > -1) {
                            attendee_select.append('<option value="' + allAttendees[i].ID + '" selected>' + allAttendees[i].FirstName + ' ' + allAttendees[i].LastName + '</option>');
                        } else {
                            attendee_select.append('<option value="' + allAttendees[i].ID + '">' + allAttendees[i].FirstName + ' ' + allAttendees[i].LastName + '</option>');
                        }
                    }
                }
            }
        })

        $('#meeting-edit').modal("show");
    });

    $('#save-meeting-edit').click(function (e) {
        saveMeeting('#meeting-edit');
    });

    //search array for a certain attendee based on the IDs of the attendees
    function containsAttendee(array, attendee) {
        for (var i = 0, len = array.length; i < len; i++) {
            if (array[i].ID == attendee.ID) {
                return i;
            }
        }

        return -1;
    }

    /**
     * Event listener for deleting a meeting from an Edit Meeting modal
     */
    $('#delete-meeting').click(function (e) {
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
        click: function (e) {
            if (confirm("Are you sure you wish to remove this meeting? This cannot be undone!")) {
                var eventListItem = $(this).closest(".external-event");
                var meetingDetails = $(eventListItem).find("span.meeting").get(0).dataset;
                $.ajax({
                    url: "/Home/DeleteMeeting",
                    method: "GET",
                    data: {
                        id: meetingDetails["id"]
                    },
                    success: function (response) {
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
        click: function () {
            // Get the ID of this meeting from the data-id attribute
            var id = this.dataset["id"];
            // Fire off a JSON request to get the meeting according to the id
            $.getJSON("/Home/RetrieveMeeting", {
                id: id
            }, function (response) {
                // empty the modal of previous results
                $.each($(".meeting-detail"), function (index, element) {
                    $(element).empty();
                });
                // We need to filter out the attributes we need for Attendees
                var attendees = [];
                response.selectedAttendees.forEach(function (attendee) {
                    attendees.push(attendee.FirstName + " " + attendee.LastName);
                });
                // Update the details for the modal
                $("#detail-attendees").text(attendees.join(", "));
                $("#detail-title").text(response.title);
                $("#detail-description").text(response.description);
                $("#detail-room").text(response.selectedRoom.RoomNo);
                // Use moment.js to parse the times given back by the server into a nice message
                $("#detail-time").text(moment(parseInt(/-?\d+/.exec(response.startTime))).format("MMMM Do YYYY, h:mm:ss a") + " to " + moment(parseInt(/-?\d+/.exec(response.endTime))).format("MMMM Do YYYY, h:mm:ss a"));
                // Show the modal
                $('#meeting-details').modal("show");
            });
        }
    }, ".external-event > span.meeting");

    /*
     * Event handler for clicking the "Create Meeting" button
     */
    $('#schedule-meeting-sidebar').click(function (e) {
        var $meeting = $('#meeting-create');
        $meeting.find('input, textarea').each(function (index, element) {
            $(element).val("");
        });
        $meeting.find('option').each(function (index, element) {
            $(element).attr("selected", false);
        });
        $meeting.modal("show");
        //get list of attendees and list of rooms from controller
        $.ajax({
            type: "GET",
            dataType: "json",
            url: "/Home/GetSchedulingData",
            async: true,
            success: function (data, status) {
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
        change: function (event) {

            //set active modal so this function is looking at either the create modal or edit modal to check if meeting info is valid before user can save
            var activeModal;
            var currentMeetingID;
            if ($('#meeting-create').data('bs.modal')) {
                activeModal = '#meeting-create';
            } else if ($('#meeting-edit').data('bs.modal')) {
                activeModal = '#meeting-edit';
                currentMeetingID = $(activeModal).find('input[name=id]').val();
            }

            var start_time = $(activeModal).find('input[name=start-time]').val();
            var end_time = $(activeModal).find('input[name=end-time]').val();

            //if both start and end time have been filled out
            if (start_time !== "" && end_time !== "") {
                var roomID = $(activeModal).find('select[name=meeting-room]').val();

                //TODO: change this to attendee_ids
                var attendees = [];
                //get all selected attendees
                $(activeModal).find('select[name=attendees] :selected').each(function (i, selected) {
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
                            meeting_id: currentMeetingID,
                            end_time: end_time,
                            start_time: start_time,
                            room_id: roomID,
                            attendees: attendees
                        },
                        url: "/Home/CheckAvailability",
                        type: "POST",
                        async: true,
                        dataType: "json",
                        success: function (data, status) {
                            if (status) {
                                var error_alert = $(activeModal).find('div[name=error-messages]');
                                error_alert.empty();

                                var save_button;
                                if (activeModal === '#meeting-create') {
                                    save_button = $(activeModal).find('button[id=save-new-meeting]');
                                } else if (activeModal === '#meeting-edit') {
                                    save_button = $(activeModal).find('button[id=save-meeting-edit]');
                                }

                                if (!data.meetingAvailable) {
                                    save_button.prop('disabled', true);
                                    error_alert.show();

                                    error_alert.append('<ul style="list-style-type:disc">');
                                    for (var i = 0; i < data.errors.Data.length; i++) {
                                        //TODO: show actual error messages to the user
                                        console.log(data.errors.Data[i]);
                                        error_alert.append('<li>' + data.errors.Data[i] + '</li>');
                                    }
                                    error_alert.append('</ul>');
                                } else {
                                    save_button.prop('disabled', false);
                                    error_alert.hide();
                                }
                            }
                        }
                    });
                }
            }
        }
    });
    $('#save-new-meeting').click(function (e) {
        saveMeeting('#meeting-create');
    });

    /*
     * Event listener for toggling a personal event on event creation
     */
    $('input[name=is-personal-event]').change(function (e) {
        $("#meeting-create").find("#attendees-multiple-select").closest('.form-group').css('display', this.checked ? 'none' : 'block');
        $("#meeting-create").find("#meeting-room-select").closest('.form-group').css('display', this.checked ? 'none' : 'block');
    });

    function saveMeeting(modal) {
        var id;
        if (modal === '#meeting-edit') {
            id = $(modal).find('input[name=id]').val();
        }

        var title = $(modal).find('input[name=title]').val();
        var description = $(modal).find('textarea[name=description]').val();
        var room_id = $(modal).find('select[name=meeting-room]').val();

        var attendee_ids = [];
        //get all selected attendee ids
        $(modal).find('select[name=attendees] :selected').each(function (i, selected) {
            attendee_ids[i] = $(selected).val();
        });

        var start_time = $(modal).find('input[id=start-time-input]').val();
        var end_time = $(modal).find('input[id=end-time-input]').val();
        var data = {
            id: id,
            title: title,
            description: description,
            room_id: room_id,
            attendee_ids: attendee_ids,
            start_time: start_time,
            end_time: end_time
        };

        if (modal == "#meeting-create") {
            data['is_personal_event'] = $("#meeting-create").find("input[name=is-personal-event]")[0].checked;
        }
        $.ajax({
            data: data,
            type: "POST",
            dataType: "json",
            url: "/Home/SaveMeeting",
            async: true,
            success: function (response) {
                if (response.success) {
                    //don't want to reappend a meeting if we're just editing one
                    if (modal === '#meeting-create') {
                        alert("Meeting created!");
                        $('#external-events').append('<div class="external-event">' + '<span class="meeting" data-is-personal-event="'+response.meeting.is_personal_event+'" data-title="' + title + '" data-attendees="' + attendee_ids.join(",") + '" data-id="' + response.meeting.id + '">' + title + '</span>' + '<span class="pull-right">' + '<i class="edit-meeting fa fa-pencil"></i>' + '<i style="padding-left: 5px; padding-right: 5px;"></i>' + '<i class="delete-meeting fa fa-times"></i>' + '</span>');
                        $('#calendar').fullCalendar('renderEvent', response.meeting);
                    }

                    $(modal).modal('hide');
                } else {
                    //TODO: show an actual error message and handle it
                    console.log("Error! fix this");
                }
            }
        });
    }
});