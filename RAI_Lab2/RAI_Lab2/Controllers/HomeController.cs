using Microsoft.AspNetCore.Mvc;
using RAI_Lab2.Interfaces;
using RAI_Lab2.Models;
using System.Diagnostics;

namespace RAI_Lab2.Controllers
{
    public class HomeController : Controller
    {
        private readonly IReservationRepository _repository;

        public HomeController(IReservationRepository repository)
        {
            _repository = repository;
        }

        public IActionResult Index()
        {
            var users = _repository.GetAllUsers();
            return View(users);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
