using Microsoft.AspNetCore.Mvc;

namespace MYAcademy_IdentityEmailProject.Controllers
{
    // Views/MessageLayout/Index.cshtml artık tüm mesaj sayfalarının ortak Layout'u.
    // Doğrudan açılırsa Dashboard'a yönlendir.
    public class MessageLayoutController : Controller
    {
        public IActionResult Index() => RedirectToAction("Index", "Dashboard");
    }
}
