using Microsoft.AspNetCore.Mvc;
using RAI_Lab2.Interfaces;
using RAI_Lab2.Models;
using RAI_Lab2.Filters;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace RAI_Lab2.Controllers
{
    [LoggedInFilter]
    public class BookingController : Controller
    {
        private readonly IReservationRepository _repository;

        public BookingController(IReservationRepository repository)
        {
            _repository = repository;
        }

        public IActionResult Calendar()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GetRooms()
        {
            var salki = _repository.GetAllRooms();
            return Json(salki);
        }

        [HttpGet]
        public IActionResult GetForDay(DateTime day)
        {
            var rezerwacje = _repository.GetReservationsForDay(day);
            return Json(rezerwacje);
        }

        [HttpPost]
        public IActionResult Create([FromBody] ReservationForm model)
        {
            if (model.startTime < DateTime.Now)
            {
                ModelState.AddModelError("", "Nie można dokonywać rezerwacji w czasie przeszłym.");
            }

            if (model.startTime >= model.endTime)
            {
                ModelState.AddModelError("", "Czas zakończenia musi być późniejszy niż rozpoczęcia.");
            }

            var duration = model.endTime - model.startTime;
            if (duration.TotalMinutes < 15)
            {
                ModelState.AddModelError("", "Rezerwacja musi trwać minimum 15 minut.");
            }
            if (duration.TotalMinutes > 180) 
            {
                ModelState.AddModelError("", "Rezerwacja może trwać maksymalnie 3 godziny."); 
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return Json(new { success = false, message = string.Join("\n", errors) }); 
            }

            var rezerwacja = new Reservation
            {
                roomId = model.roomId,
                startTime = model.startTime,
                endTime = model.endTime,
                login = HttpContext.Session.GetString("login")
            };

            var result = _repository.CreateReservation(rezerwacja);

            if (result.Success)
            {
                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false, message = result.ErrorMessage });
            }
        }


        public IActionResult MyBookings()
        {
            var login = HttpContext.Session.GetString("login");
            var reservations = _repository.GetUpcomingReservationsForUser(login);

            return View(reservations);
        }

        [HttpPost]
        public IActionResult Cancel(int id)
        {
            var login = HttpContext.Session.GetString("login");
            _repository.CancelReservation(id, login);

            return RedirectToAction("MyBookings"); 
        }

        public IActionResult ExportMyBookings()
        {
            var login = HttpContext.Session.GetString("login");
            var reservations = _repository.GetUpcomingReservationsForUser(login); 
            var sb = new StringBuilder();
            sb.AppendLine("BEGIN:VCALENDAR");
            sb.AppendLine("VERSION:2.0");
            sb.AppendLine("PRODID:-//railab2//PL");

            foreach (var r in reservations)
            {
                sb.AppendLine("BEGIN:VEVENT");
                sb.AppendLine($"DTSTAMP:{DateTime.UtcNow:yyyyMMddTHHmmssZ}");
                sb.AppendLine($"DTSTART:{r.startTime:yyyyMMddTHHmmss}"); 
                sb.AppendLine($"DTEND:{r.endTime:yyyyMMddTHHmmss}");
                sb.AppendLine($"SUMMARY:Rezerwacja sali: {r.room?.name ?? "Brak nazwy"}");
                sb.AppendLine($"LOCATION:{r.room?.name}");
                sb.AppendLine($"UID:{r.Id}@railab2.pl");
                sb.AppendLine("END:VEVENT");
            }
            sb.AppendLine("END:VCALENDAR");

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());

            return File(bytes, "text/calendar", "my_reservations.ics");
        }
    }
}
