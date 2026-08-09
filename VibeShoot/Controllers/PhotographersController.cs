using Microsoft.AspNetCore.Mvc;

namespace VibeShoot.Controllers
{
    public class PhotographersController : Controller
    {
        public IActionResult Details()
        {
            return View();
        }
    }
}
