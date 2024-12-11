document.addEventListener('DOMContentLoaded', function () {
    var antiforgeryToken = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
    var currentQuery = "";
    var justSearched = false;

    var calendarEl = document.getElementById('calendar');
    var calendar = new FullCalendar.Calendar(calendarEl, {
        initialView: 'dayGridMonth',
        headerToolbar: {
            left: 'prev,next today',
            center: 'title',
            right: 'dayGridMonth,timeGridWeek,timeGridDay,listWeek'
        },
        editable: true,
        selectable: true,

        events: function (fetchInfo, successCallback, failureCallback) {
            var url = '/Calendar/GetEvents?query=' + encodeURIComponent(currentQuery);
            fetch(url)
                .then(response => response.json())
                .then(data => {
                    successCallback(data);

                    if (justSearched && data.length > 0) {
                        var earliestDate = null;
                        data.forEach(function (ev) {
                            var evDate = new Date(ev.start + "T00:00:00");
                            if (earliestDate == null || evDate < earliestDate) {
                                earliestDate = evDate;
                            }
                        });

                        if (earliestDate) {
                            setTimeout(function() {
                                calendar.gotoDate(earliestDate);
                            }, 100);
                        }
                    }

                    justSearched = false;
                })
                .catch(error => {
                    console.error('Error fetching events:', error);
                    failureCallback(error);
                });
        },

        eventDrop: function(info) {
            var newDueDate = info.event.start.toISOString().split('T')[0];
            fetch('/Calendar/UpdateTaskDate', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': antiforgeryToken
                },
                body: JSON.stringify({
                    id: info.event.id,
                    newDueDate: newDueDate
                })
            })
                .then(response => {
                    if (!response.ok) {
                        alert("An error occurred while updating the task date.");
                    }
                });
        },

        eventClick: function(info) {
            alert('Task: ' + info.event.title + '\n' + info.event.extendedProps.description);
        }
    });

    calendar.render();

    var searchInput = document.getElementById('calendarSearchInput');
    var debounceTimer;
    searchInput.addEventListener('keyup', function () {
        clearTimeout(debounceTimer);
        debounceTimer = setTimeout(function() {
            currentQuery = searchInput.value;
            justSearched = true;
            calendar.refetchEvents();
        }, 300);
    });
});
