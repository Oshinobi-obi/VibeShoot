using Microsoft.AspNetCore.Mvc;

namespace VibeShoot.Controllers
{
    public class PhotographersController : Controller
    {
        [HttpGet("photographers")]
        public IActionResult Details()
        {
            return View("~/Views/Photographers/Details.cshtml");
        }
    }
}