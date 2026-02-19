using Microsoft.AspNetCore.Mvc;
using RAI_Lab2.Interfaces;

namespace RAI_Lab2.Controllers
{
    public class AccountController : Controller
    {
        private readonly IReservationRepository _repository;

        public AccountController(IReservationRepository repository)
        {
            _repository = repository;
        }

        [Route("Account/Login/{login}")]
        public IActionResult Login(string login)
        {
            var user = _repository.GetUserByLogin(login);
            if (user != null)
            {
                HttpContext.Session.SetString("login", user.login);
                HttpContext.Session.SetString("isAdmin", user.isAdmin.ToString());
            }

            return RedirectToAction("Calendar", "Booking");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
