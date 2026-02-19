using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RAI_Lab2.Filters
{
    public class AdminFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var userLogin = context.HttpContext.Session.GetString("login");
            var isAdmin = context.HttpContext.Session.GetString("isAdmin");

            if (string.IsNullOrEmpty(userLogin) || isAdmin != "True")
            {
                context.Result = new RedirectToActionResult("Index", "Home", null);
            }

            base.OnActionExecuting(context);
        }
    }
}
