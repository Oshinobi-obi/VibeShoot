
using Microsoft.AspNetCore.Mvc;

namespace VibeShoot.Controllers
{
    public class GalleryController : Controller
    {
        public IActionResult Gallery()
        {
            return View();
        }
    }
}
