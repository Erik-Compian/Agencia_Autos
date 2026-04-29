using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http;

namespace AgenciaMVC1.Filters
{
    // Heredamos de ActionFilterAttribute para convertir esta clase en una etiqueta decorativa
    public class ValidarSesionAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Verificamos si la variable de sesión "AdminId" NO existe
            if (context.HttpContext.Session.GetInt32("AdminId") == null)
            {
                // Si no hay sesión, lo redirigimos al Login del AccountController
                context.Result = new RedirectToActionResult("Login", "Account", null);
            }

            base.OnActionExecuting(context);
        }
    }
}