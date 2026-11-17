using Microsoft.EntityFrameworkCore;
using MYAcademy_IdentityEmailProject.Context;
using MYAcademy_IdentityEmailProject.Entities;

namespace MYAcademy_IdentityEmailProject.Services
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<EmailContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<EmailContext>>();

            try
            {
                if (await context.Categories.AnyAsync()) return;

                context.Categories.AddRange(
                    new Category { CategoryName = "İş", CategoryColor = "#8B7FD6" },
                    new Category { CategoryName = "Kişisel", CategoryColor = "#5CC09A" },
                    new Category { CategoryName = "Fatura", CategoryColor = "#EBA27A" },
                    new Category { CategoryName = "Sosyal", CategoryColor = "#6FA8E8" },
                    new Category { CategoryName = "Duyuru", CategoryColor = "#E07597" });
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Migration henüz uygulanmadıysa uygulama yine de açılsın
                logger.LogWarning(ex, "Kategori seed işlemi atlandı. 'Update-Database' çalıştırıldı mı?");
            }
        }
    }
}
