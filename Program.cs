using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using MYAcademy_IdentityEmailProject.Context;
using MYAcademy_IdentityEmailProject.Entities;
using MYAcademy_IdentityEmailProject.Services;

var builder = WebApplication.CreateBuilder(args);

// --- Veritabanı ---
builder.Services.AddDbContext<EmailContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- Identity ---
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedEmail = true;      // 6 haneli kod doğrulanmadan giriş yok
    options.User.RequireUniqueEmail = true;
    options.Password.RequiredLength = 8;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
})
.AddEntityFrameworkStores<EmailContext>()
.AddDefaultTokenProviders()
.AddErrorDescriber<TurkishIdentityErrorDescriber>();

// Şifre yenileme bağlantısı 30 dakika geçerli. Identity token'ı güvenlik damgasına bağlı olduğu için
// şifre değiştikten sonra aynı bağlantı tekrar kullanılamaz (tek kullanımlık).
builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
    options.TokenLifespan = TimeSpan.FromMinutes(30));

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Login/SignIn";
    options.LogoutPath = "/Login/Logout";
    options.AccessDeniedPath = "/ErrorPages/Page403";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
    options.Events.OnRedirectToLogin = context =>
    {
        // AJAX isteklerinde login sayfasına yönlendirmek yerine 401 döndür
        if (context.Request.Headers.XRequestedWith == "XMLHttpRequest")
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        else
            context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
});

// --- E-posta (SMTP) ---
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("Smtp"));
builder.Services.AddScoped<IEmailService, EmailService>();

// --- MVC ---
// Tüm sayfalar varsayılan olarak giriş ister; Login/Register/ErrorPages [AllowAnonymous] ile açılır.
// Tüm POST isteklerinde AntiForgery token doğrulanır.
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AuthorizeFilter(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build()));
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

var app = builder.Build();

// Kategori tablosu boşsa varsayılan kategorileri ekle
await DbSeeder.SeedAsync(app.Services);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/ErrorPages/Status/500");
    app.UseHsts();
}

// 401 / 403 / 404 vb. durum kodlarında ilgili hata sayfasını göster
app.UseStatusCodePagesWithReExecute("/ErrorPages/Status/{0}");

app.UseHttpsRedirection();
app.UseStaticFiles();   // çalışma anında yüklenen profil fotoğrafları (wwwroot/images/users) için
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
