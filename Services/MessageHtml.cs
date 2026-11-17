using System.Net;
using System.Text.RegularExpressions;
using Ganss.Xss;

namespace MYAcademy_IdentityEmailProject.Services
{
    /// <summary>
    /// Mesaj içeriği zengin metin editöründen HTML olarak gelir.
    /// Başka kullanıcının tarayıcısında script çalıştırılamaması (XSS) için
    /// kaydetmeden önce ve ekrana basmadan önce temizlenir.
    /// </summary>
    public static class MessageHtml
    {
        private static readonly HtmlSanitizer Sanitizer = CreateSanitizer();

        private static HtmlSanitizer CreateSanitizer()
        {
            var s = new HtmlSanitizer();
            s.AllowedSchemes.Add("mailto");
            s.AllowedAttributes.Add("class");
            return s;
        }

        public static string Sanitize(string? html) =>
            string.IsNullOrWhiteSpace(html) ? string.Empty : Sanitizer.Sanitize(html).Trim();

        public static string ToPlainText(string? html)
        {
            if (string.IsNullOrWhiteSpace(html)) return string.Empty;
            var text = Regex.Replace(html, "<(br|/p|/div|/h[1-6]|/li|/blockquote)[^>]*>", " ", RegexOptions.IgnoreCase);
            text = WebUtility.HtmlDecode(Regex.Replace(text, "<[^>]+>", " "));
            return Regex.Replace(text, @"\s+", " ").Trim();
        }

        public static string Snippet(string? html, int max = 140)
        {
            var text = ToPlainText(html);
            return text.Length > max ? text[..max].TrimEnd() + "…" : text;
        }
    }
}
