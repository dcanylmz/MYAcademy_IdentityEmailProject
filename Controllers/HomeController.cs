using Microsoft.AspNetCore.Mvc;

namespace MYAcademy_IdentityEmailProject.Controllers
{
    // Varsayılan rota artık Dashboard. Eski /Home adresleri oraya yönlendirilir.
    public class HomeController : Controller
    {
        public IActionResult Index() => RedirectToAction("Index", "Dashboard");
    }
}
