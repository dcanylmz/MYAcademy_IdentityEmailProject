<div align="center">

# ✉️ Mailora

**ASP.NET Core Identity ile geliştirilmiş, kullanıcıya özel e-posta yönetim sistemi**

Kayıt ol, email adresini doğrula, mesajlaş, taslak kaydet, yıldızla, sil, geri al.<br>
Hepsi kendi gelen kutunda ve yalnızca sana ait verilerle.

![.NET](https://img.shields.io/badge/.NET-10-512BD4?style=flat-square&logo=dotnet&logoColor=white)
![ASP.NET Core MVC](https://img.shields.io/badge/ASP.NET_Core-MVC-5C2D91?style=flat-square&logo=dotnet&logoColor=white)
![EF Core](https://img.shields.io/badge/EF_Core-10-6DB33F?style=flat-square)
![SQL Server](https://img.shields.io/badge/SQL_Server-CC2927?style=flat-square&logo=microsoftsqlserver&logoColor=white)
![Bootstrap](https://img.shields.io/badge/Bootstrap-5-7952B3?style=flat-square&logo=bootstrap&logoColor=white)
![Chart.js](https://img.shields.io/badge/Chart.js-4-FF6384?style=flat-square&logo=chartdotjs&logoColor=white)

[Özellikler](#-özellikler) •
[Hızlı Başlangıç](#-hızlı-başlangıç) •
[Güvenlik](#-güvenlik) •
[Proje Yapısı](#-proje-yapısı)

</div>

---

## 📖 Proje Hakkında

Mailora, **MY Academy 12. Dönem Bootcamp** kapsamında geliştirilmiş bir e-posta yönetim uygulamasıdır.
Kullanıcılar uygulama içinde birbirlerine mesaj gönderebilir. Uygulamaya kayıtlı olmayan
adreslere ise mesajlar **gerçek e-posta (SMTP)** olarak gider.

Tüm veriler SQL Server'da tutulur ve her kullanıcı **yalnızca kendi mesajlarını** görebilir.

---

## ✨ Özellikler

<table>
<tr>
<td width="50%" valign="top">

### 🔐 Hesap
- Kayıt ol, giriş yap, çıkış yap
- Email'e gelen **6 haneli kod** ile hesap doğrulama
- Kullanıcı adı **veya** email ile giriş
- **Şifremi unuttum:** 30 dk geçerli, tek kullanımlık link
- Profilden şifre değiştirme

</td>
<td width="50%" valign="top">

### 📬 Mesajlar
- Gelen Kutusu, Gönderilenler, Yıldızlılar
- **Taslaklar:** kaydet, düzenle, gönder
- **Çöp Kutusu:** taşı, geri al, kalıcı sil
- Yanıtla, ilet, hızlı yanıt
- Okundu / okunmadı işaretleme

</td>
</tr>
<tr>
<td width="50%" valign="top">

### 🔎 Arama ve Düzen
- Anlık arama (kişi, konu, içerik)
- Kategori, okunmamış ve yıldızlı filtreleri
- Tarihe göre sıralama, sayfalama
- Zengin metin editörü (kalın, renk, liste, link)

</td>
<td width="50%" valign="top">

### 📊 Dashboard ve Profil
- Gelen, gönderilen, okunmamış, yıldızlı ve taslak sayıları
- 7 / 30 / 90 günlük mesaj trafiği grafiği
- Kategori dağılımı, en sık yazışılan kişiler
- Profil fotoğrafı, ad-soyad, hakkımda

</td>
</tr>
</table>

---

## 🚀 Hızlı Başlangıç

### Gereksinimler

| Araç | Sürüm |
|---|---|
| [.NET SDK](https://dotnet.microsoft.com/download) | 10.0 |
| SQL Server | LocalDB, Express veya tam sürüm |
| Visual Studio 2022+ | *(önerilir)* |

### 1️⃣ Projeyi indir

```bash
git clone https://github.com/dcanylmz/MYAcademy_IdentityEmailProject.git
cd MYAcademy_IdentityEmailProject
```

### 2️⃣ Veritabanını hazırla

Gerekirse `appsettings.json` içindeki bağlantı cümlesini kendi SQL Server'ına göre düzenle:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Initial Catalog=IdentityEmailDb;Integrated Security=true;TrustServerCertificate=true"
}
```

Ardından tabloları oluştur:

```powershell
# Visual Studio → Package Manager Console
Update-Database

# veya terminal
dotnet ef database update
```

> 💡 Kategoriler (İş, Kişisel, Fatura, Sosyal, Duyuru) uygulama ilk açıldığında otomatik eklenir.

### 3️⃣ E-posta (SMTP) ayarlarını gir

Doğrulama kodları ve dışarıya giden mesajlar için bir Gmail hesabı gerekir.

1. Google hesabında **2 Adımlı Doğrulama**'yı aç
2. [Uygulama Şifreleri](https://myaccount.google.com/apppasswords) sayfasından yeni bir şifre oluştur
3. Şifreyi **User Secrets**'a kaydet (kodun içine veya repoya **yazma**):

```powershell
dotnet user-secrets set "Smtp:UserName"    "adresin@gmail.com"
dotnet user-secrets set "Smtp:Password"    "uygulama-sifren"
dotnet user-secrets set "Smtp:FromAddress" "adresin@gmail.com"
```

<details>
<summary>Visual Studio ile yapmak istersen</summary>

<br>

Solution Explorer → projeye sağ tıkla → **Manage User Secrets** → şunu yapıştır:

```json
{
  "Smtp": {
    "UserName": "adresin@gmail.com",
    "Password": "uygulama-sifren",
    "FromAddress": "adresin@gmail.com"
  }
}
```

</details>

### 4️⃣ Çalıştır

```bash
dotnet run
```

Tarayıcıda açılan adreste **Kayıt Ol** → mailine gelen kodu gir → **Giriş Yap**. Hepsi bu kadar 🎉

---

## 🛡️ Güvenlik

| Önlem | Ne işe yarar? |
|---|---|
| 🔒 **Varsayılan olarak kilitli** | Tüm sayfalar giriş ister; yalnızca Login, Register ve hata sayfaları açıktır |
| 👤 **Sahiplik kontrolü** | URL'deki mesaj id'sini değiştirerek başkasının mesajı açılamaz (403) |
| ✉️ **Email doğrulama** | Kod doğrulanmadan giriş yapılamaz |
| 🧱 **Hesap kilitleme** | 5 hatalı denemede hesap 5 dakika kilitlenir |
| 🧼 **XSS koruması** | Mesaj içeriği `HtmlSanitizer` ile temizlenir, zararlı script çalışmaz |
| 🛡️ **CSRF koruması** | Tüm POST isteklerinde AntiForgery token doğrulanır |
| 🔑 **Sırlar koddan uzak** | SMTP şifresi User Secrets'ta tutulur, repoya girmez |
| 🖼️ **Güvenli dosya yükleme** | Uzantı ve boyut kontrolü yapılır, dosyalar rastgele isimle kaydedilir |

---

## 🧠 Nasıl Çalışıyor?

### Mesaj kime gider?

```
Yeni Mesaj → Alıcı Mailora'ya kayıtlı mı?
               ├── Evet  → Alıcının Gelen Kutusuna düşer
               └── Hayır → SMTP ile gerçek email adresine gönderilir
```

### Bir mesaj, iki bakış açısı

Gönderen ve alıcı veritabanında **aynı mesaj satırını** paylaşır. Bu yüzden yıldız ve silme
durumları her iki taraf için ayrı tutulur:

| Gönderen tarafı | Alıcı tarafı |
|---|---|
| `IsStarredBySender` | `IsStarredByReceiver` |
| `IsDeletedBySender` | `IsDeletedByReceiver` |
| `IsPermanentlyDeletedBySender` | `IsPermanentlyDeletedByReceiver` |

Böylece alıcı bir mesajı sildiğinde mesaj gönderenin "Gönderilenler" kutusunda kalmaya devam eder.
Satır, iki taraf da kalıcı olarak sildiğinde veritabanından tamamen kaldırılır.

---

## 📁 Proje Yapısı

```
MYAcademy_IdentityEmailProject
│
├── Controllers/        Dashboard, Message, Profile, Login, Register, ErrorPages
├── Entities/           AppUser, Message, Category (veritabanı tabloları)
├── Models/             ViewModel'ler (form ve ekran verileri)
├── Context/            EmailContext (IdentityDbContext)
├── Services/           EmailService, MessageQueries, MessageHtml, EmailTemplates
├── Helpers/            View'larda kullanılan biçimlendirme yardımcıları
├── ViewComponents/     Sidebar, Navbar ve tema parçaları
├── Views/              Razor sayfaları
├── Migrations/         EF Core migration dosyaları
└── wwwroot/            Statik dosyalar ve yüklenen profil fotoğrafları
```

---

## 🧰 Kullanılan Teknolojiler

- **Backend:** ASP.NET Core MVC (.NET 10), ASP.NET Core Identity
- **Veritabanı:** Entity Framework Core 10, SQL Server
- **E-posta:** MailKit / MimeKit
- **Güvenlik:** HtmlSanitizer
- **Frontend:** Bootstrap 5, Bootstrap Icons, Chart.js, Vanilla JavaScript

---

## 🗺️ Yol Haritası

- [x] Üyelik, email doğrulama, şifre sıfırlama
- [x] Gelen / Gönderilen / Yıldızlı / Taslak / Çöp kutuları
- [x] Yanıtla, ilet, SMTP ile dışarıya gönderim
- [x] Profil ve Dashboard
- [ ] Dosya eki yükleme ve indirme
- [ ] Çoklu alıcı, CC / BCC
- [ ] Mesaj arşivleme
- [ ] Anlık yeni mesaj bildirimleri

---

<div align="center">

**MY Academy · 12. Dönem Bootcamp** kapsamında geliştirilmiştir.

⭐ Projeyi beğendiysen yıldız bırakmayı unutma!

</div>
