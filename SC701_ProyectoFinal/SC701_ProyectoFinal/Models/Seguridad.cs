using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SC701_ProyectoFinal.Models
{
    public class Seguridad : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.HttpContext.Session.GetInt32("Id_Usuario") == null)
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
            }
            else
            {
                base.OnActionExecuting(context);
            }
        }
    }
}
