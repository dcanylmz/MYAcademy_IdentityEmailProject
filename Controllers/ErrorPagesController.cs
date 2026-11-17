using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MYAcademy_IdentityEmailProject.Models;

namespace MYAcademy_IdentityEmailProject.Controllers
{
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    public class ErrorPagesController : Controller
    {
        public IActionResult Page401() => ErrorView(401);

        public IActionResult Page403() => ErrorView(403);

        public IActionResult Page404() => ErrorView(404);

        // UseStatusCodePagesWithReExecute("/ErrorPages/Status/{0}") buraya yönlendirir
        public IActionResult Status(int id) => ErrorView(id);

        private IActionResult ErrorView(int code)
        {
            var model = code switch
            {
                401 => new ErrorPageViewModel(401, "Oturum açmanız gerekiyor",
                    "Bu sayfayı görüntülemek için giriş yapmalısınız. Oturumunuzun süresi dolmuş olabilir.", "bi-person-lock"),
                403 => new ErrorPageViewModel(403, "Bu içeriğe erişim yetkiniz yok",
                    "Görüntülemek istediğiniz mesaj veya sayfa başka bir kullanıcıya ait.", "bi-shield-lock"),
                404 => new ErrorPageViewModel(404, "Sayfa bulunamadı",
                    "Aradığınız sayfa veya mesaj taşınmış, silinmiş ya da hiç var olmamış olabilir.", "bi-signpost-split"),
                _ => new ErrorPageViewModel(code, "Bir şeyler ters gitti",
                    "İsteğiniz işlenirken beklenmeyen bir hata oluştu. Lütfen daha sonra tekrar deneyin.", "bi-bug")
            };

            Response.StatusCode = code;
            return View("ErrorPage", model);
        }
    }
}
