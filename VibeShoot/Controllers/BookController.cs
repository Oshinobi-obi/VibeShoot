using Microsoft.AspNetCore.Mvc;

namespace VibeShoot.Controllers
{
    public class BookController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
