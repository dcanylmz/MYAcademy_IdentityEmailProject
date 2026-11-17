using System.Net;

namespace MYAcademy_IdentityEmailProject.Services
{
    /// <summary>Gönderilen e-postaların ortak HTML şablonu.</summary>
    public static class EmailTemplates
    {
        private static string Wrap(string title, string inner) => $@"
<div style=""background:#F7F6FC;padding:32px 12px;font-family:Segoe UI,Arial,sans-serif;color:#303348"">
  <div style=""max-width:520px;margin:0 auto;background:#fff;border-radius:20px;padding:32px;box-shadow:0 10px 30px rgba(91,80,170,.08)"">
    <div style=""font-size:20px;font-weight:800;margin-bottom:24px"">Mail<span style=""color:#8B7FD6"">ora</span></div>
    <h2 style=""font-size:18px;margin:0 0 12px"">{WebUtility.HtmlEncode(title)}</h2>
    {inner}
    <p style=""color:#85899B;font-size:12px;margin-top:28px"">Bu e-posta Mailora tarafından otomatik olarak gönderilmiştir.</p>
  </div>
</div>";

        public static string VerificationCode(string name, int code) => Wrap("E-posta doğrulama kodunuz", $@"
    <p style=""line-height:1.6"">Merhaba {WebUtility.HtmlEncode(name)}, Mailora hesabınızı etkinleştirmek için aşağıdaki 6 haneli kodu kullanın.</p>
    <div style=""font-size:32px;font-weight:800;letter-spacing:10px;text-align:center;background:#EFEDFB;color:#6F62C6;border-radius:14px;padding:18px;margin:20px 0"">{code}</div>
    <p style=""color:#85899B;font-size:13px"">Bu işlemi siz yapmadıysanız e-postayı yok sayabilirsiniz.</p>");

        public static string ResetPassword(string name, string link) => Wrap("Şifre yenileme talebi", $@"
    <p style=""line-height:1.6"">Merhaba {WebUtility.HtmlEncode(name)}, şifrenizi yenilemek için aşağıdaki butona tıklayın. Bağlantı <b>30 dakika</b> geçerlidir ve yalnızca bir kez kullanılabilir.</p>
    <p style=""text-align:center;margin:26px 0""><a href=""{WebUtility.HtmlEncode(link)}"" style=""background:#8B7FD6;color:#fff;text-decoration:none;padding:13px 26px;border-radius:12px;font-weight:700;display:inline-block"">Şifremi Yenile</a></p>
    <p style=""color:#85899B;font-size:13px"">Bu talebi siz yapmadıysanız hiçbir şey yapmanıza gerek yok; şifreniz değişmeyecek.</p>");

        // Uygulama dışındaki bir adrese gönderilen mesaj (içerik daha önce sanitize edildi)
        public static string ExternalMessage(string senderName, string senderEmail, string safeHtmlBody) => $@"
<div style=""font-family:Segoe UI,Arial,sans-serif;color:#303348;line-height:1.6"">
  {safeHtmlBody}
  <hr style=""border:0;border-top:1px solid #ECEBF5;margin:24px 0 12px"">
  <p style=""color:#85899B;font-size:12px"">{WebUtility.HtmlEncode(senderName)} &lt;{WebUtility.HtmlEncode(senderEmail)}&gt; tarafından Mailora ile gönderildi.</p>
</div>";
    }
}
