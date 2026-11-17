using System.Globalization;
using System.Net;
using Microsoft.AspNetCore.Html;

namespace MYAcademy_IdentityEmailProject.Helpers
{
    /// <summary>View'larda ortak kullanılan küçük biçimlendirme yardımcıları.</summary>
    public static class UiHelpers
    {
        public static readonly CultureInfo Tr = new("tr-TR");
        private static readonly string[] AvatarColors = { "av-lav", "av-blue", "av-mint", "av-rose", "av-peach", "av-sun" };

        public static string Initials(string? name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "?";
            var source = name.Contains('@') ? name.Split('@')[0].Replace('.', ' ').Replace('_', ' ') : name;
            var parts = source.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return "?";
            return string.Concat(parts.Take(2).Select(p => p[0])).ToUpper(Tr);
        }

        // Aynı kişi her zaman aynı renkte görünür
        public static string AvatarColor(string? key)
        {
            var h = 0;
            foreach (var c in (key ?? "").ToLowerInvariant()) h = (h * 31 + c) & 0x7fffffff;
            return AvatarColors[h % AvatarColors.Length];
        }

        // Bugün → 10:42 · Dün → Dün · Bu yıl → 4 Kas · Eski → 12.03.2025
        public static string ShortTime(DateTime d)
        {
            var today = DateTime.Today;
            if (d.Date == today) return d.ToString("HH:mm", Tr);
            if (d.Date == today.AddDays(-1)) return "Dün";
            if (d.Year == today.Year) return d.ToString("d MMM", Tr);
            return d.ToString("dd.MM.yyyy", Tr);
        }

        public static string LongDate(DateTime d) => d.ToString("d MMMM yyyy, dddd HH:mm", Tr);

        public static string FullName(string? name, string? surname, string? fallback)
        {
            var full = $"{name} {surname}".Trim();
            return string.IsNullOrEmpty(full) ? (fallback ?? "") : full;
        }

        /// <summary>Profil fotoğrafı varsa resim, yoksa baş harfler gösteren avatar.</summary>
        public static IHtmlContent Avatar(string? name, string? imageUrl, string? colorKey, string size = "", string extraClass = "")
        {
            var cls = $"avatar {size} {extraClass}".Trim();
            if (!string.IsNullOrWhiteSpace(imageUrl))
                return new HtmlString($"<span class=\"{cls}\"><img src=\"{WebUtility.HtmlEncode(imageUrl)}\" alt=\"{WebUtility.HtmlEncode(name)}\"></span>");
            return new HtmlString($"<span class=\"{cls} {AvatarColor(colorKey ?? name)}\">{WebUtility.HtmlEncode(Initials(name))}</span>");
        }

        // Kategori rengi → yumuşak arka planlı etiket stili
        public static string TagStyle(string? color)
        {
            var c = string.IsNullOrWhiteSpace(color) ? "#85899B" : color;
            return $"background:{c}1F;color:{c}";
        }
    }
}
