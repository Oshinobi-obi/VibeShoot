using Microsoft.AspNetCore.Mvc;

namespace VibeShoot.Controllers
{
    public class PhotographersController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
