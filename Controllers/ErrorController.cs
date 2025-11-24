using Microsoft.AspNetCore.Mvc;

namespace WebBanMayTinh.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error/{statusCode}")]
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {
            switch (statusCode)
            {
                case 404:
                    return View("NotFound"); // Views/Error/NotFound.cshtml
                case 403:
                    return View("403");
                case 500:
                    return View("ServerError");
            }

            return View("Generic");
        }
    }
}
