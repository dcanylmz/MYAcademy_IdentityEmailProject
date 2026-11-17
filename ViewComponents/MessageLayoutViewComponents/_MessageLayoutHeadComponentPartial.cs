using Microsoft.AspNetCore.Mvc;

namespace MYAcademy_IdentityEmailProject.ViewComponents.MessageLayoutViewComponents
{
    public class _MessageLayoutHeadComponentPartial:ViewComponent
    {
        public IViewComponentResult Invoke() { 
            return View();
        }
    }
}
