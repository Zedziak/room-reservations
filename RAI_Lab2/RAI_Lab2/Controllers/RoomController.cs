using Microsoft.AspNetCore.Mvc;
using RAI_Lab2.Interfaces;
using RAI_Lab2.Models;
using RAI_Lab2.Filters;

namespace RAI_Lab2.Controllers
{
    [AdminFilter]
    public class RoomController : Controller
    {
        private readonly IReservationRepository _repository;

        public RoomController(IReservationRepository repository)
        {
            _repository = repository;
        }

        public IActionResult Init()
        {
            _repository.InitData();
            return Content("Dane zostały zainicjalizowane.");
        }

        [HttpGet]
        public IActionResult Manage()
        {
            var model = new ManageRooms
            {
                existingRooms = _repository.GetAllRooms() 
            };
            return View(model); 
        }

        [HttpPost] 
        [ValidateAntiForgeryToken]
        public IActionResult Manage(ManageRooms model)
        {
            if (!ModelState.IsValid)
            {
                model.existingRooms = _repository.GetAllRooms();
                return View(model);
            }

            var nowaSalka = new Room
            {
                name = model.newRoomName,
                size = model.newRoomSize
            };
            _repository.AddRoom(nowaSalka);

            return RedirectToAction("Manage");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {

            _repository.DeleteRoom(id);
            return RedirectToAction("Manage");
        }
    }
}
