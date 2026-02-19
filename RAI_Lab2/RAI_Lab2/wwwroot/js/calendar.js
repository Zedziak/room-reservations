
document.addEventListener('DOMContentLoaded', function () {
    const datePicker = document.getElementById('datepicker');
    const calendarContainer = document.getElementById('calendar-container');
    const bookingModal = new bootstrap.Modal(document.getElementById('bookingModal'));
    const saveBookingBtn = document.getElementById('saveBookingBtn');
    const reservationForm = document.getElementById('reservationForm');

    const modalRoomId = document.getElementById('roomId');
    const modalRoomName = document.getElementById('roomName');
    const modalStartTime = document.getElementById('startTime');
    const modalEndTime = document.getElementById('endTime');
    const modalError = document.getElementById('modalError');

    const today = new Date();
    datePicker.value = today.toISOString().split('T')[0];

    var currentRooms = [];
    var currentReservations = [];
    function toLocalISOString(date) {
        const offset = date.getTimezoneOffset();
        const adjustedDate = new Date(date.getTime() - (offset * 60000));
        return adjustedDate.toISOString().slice(0, 16);
    }

    async function loadCalendarData() {
        const selectedDate = datePicker.value;
        if (!selectedDate) return;

        calendarContainer.innerHTML = '<p>Ładowanie...</p>';

        try {
            const roomsResponse = await fetch('/Booking/GetRooms');
            currentRooms = await roomsResponse.json();

            const reservationsResponse = await fetch(`/Booking/GetForDay?day=${selectedDate}`);
            currentReservations = await reservationsResponse.json();

            renderCalendar(selectedDate);
        } catch (error) {
            calendarContainer.innerHTML = '<p class="text-danger">Błąd ładowania kalendarza.</p>';
            console.error(error);
        }
    }

    function renderCalendar(selectedDateStr) {
        const table = document.createElement('table');
        table.className = 'table table-bordered text-center calendar-table';

        const thead = table.createTHead();
        const headerRow = thead.insertRow();
        headerRow.insertCell().textContent = 'Godzina';
        currentRooms.forEach(room => {
            headerRow.insertCell().textContent = `${room.name} (Rozmiar: ${room.size})`;
        });

        const tbody = table.createTBody();
        const selectedDate = new Date(selectedDateStr);

        for (let hour = 8; hour < 18; hour++) {
            for (let minute = 0; minute < 60; minute += 30) {
                const row = tbody.insertRow();
                const cellTime = new Date(selectedDate.getFullYear(), selectedDate.getMonth(), selectedDate.getDate(), hour, minute);
                
                row.insertCell().textContent = `${hour.toString().padStart(2, '0')}:${minute.toString().padStart(2, '0')}`;

                currentRooms.forEach(room => {
                    const cell = row.insertCell();
                    cell.dataset.roomId = room.id;
                    cell.dataset.roomName = room.name;
                    cell.dataset.startTime = cellTime.toISOString();

                    const conflictingReservation = currentReservations.find(r =>
                        r.roomId === room.id &&
                        new Date(r.startTime) <= cellTime &&
                        new Date(r.endTime) > cellTime
                     
                    );

                    if (conflictingReservation) {
                        cell.classList.add('table-danger', 'zajete');
                        cell.textContent = `Zajęte (${conflictingReservation.login})`;
                    } else {
                        cell.classList.add('table-success', 'wolne');
                        cell.textContent = 'Wolne';
                    }
                });
            }
        }
        calendarContainer.innerHTML = '';
        calendarContainer.appendChild(table);
    }


    datePicker.addEventListener('change', loadCalendarData);

    calendarContainer.addEventListener('click', function (e) {
        const cell = e.target;
        if (cell.classList.contains('wolne')) {
            const roomId = cell.dataset.roomId;
            const roomName = cell.dataset.roomName;
            const startTime = new Date(cell.dataset.startTime);
            const endTime = new Date(startTime.getTime() + 60 * 60000);

            modalRoomId.value = roomId;
            modalRoomName.textContent = roomName;
            modalStartTime.value = toLocalISOString(startTime);
            modalEndTime.value = toLocalISOString(endTime);
            modalError.textContent = '';

            bookingModal.show();
        }
    });

    saveBookingBtn.addEventListener('click', async function () {
        modalError.textContent = '';

        const reservation = {
            roomId: parseInt(modalRoomId.value),
            startTime: modalStartTime.value,
            endTime: modalEndTime.value
        };

        if (!reservation.startTime || !reservation.endTime) {
            modalError.textContent = "Wszystkie pola są wymagane.";
            return;
        }
        if (new Date(reservation.startTime) >= new Date(reservation.endTime)) {
            modalError.textContent = "Czas zakończenia musi być późniejszy niż rozpoczęcia.";
            return;
        }

        try {
            const response = await fetch('/Booking/Create', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(reservation)
            });

            const result = await response.json();

            if (result.success) {
                bookingModal.hide();
                loadCalendarData();
            } else {
                modalError.textContent = result.message;
            }
        } catch (error) {
            modalError.textContent = "Wystąpił błąd sieci. Spróbuj ponownie.";
            console.error(error);
        }
    });

    loadCalendarData();
});