using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MYAcademy_IdentityEmailProject.Entities;

namespace MYAcademy_IdentityEmailProject.Context
{
    public class EmailContext : IdentityDbContext<AppUser>
    {
        // Bağlantı cümlesi appsettings.json → ConnectionStrings:DefaultConnection
        public EmailContext(DbContextOptions<EmailContext> options) : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);   // Identity tabloları için gerekli

            builder.Entity<Message>(e =>
            {
                e.Property(x => x.SenderEmail).HasMaxLength(256);
                e.Property(x => x.ReceiverEmail).HasMaxLength(256);
                e.Property(x => x.MessageSubject).HasMaxLength(200);
                e.HasIndex(x => x.SenderEmail);
                e.HasIndex(x => x.ReceiverEmail);

                // Kategori silinirse mesajlar silinmesin, kategorisiz kalsın
                e.HasOne(x => x.Category)
                 .WithMany(c => c.Messages)
                 .HasForeignKey(x => x.CategoryID)
                 .OnDelete(DeleteBehavior.SetNull);
            });

            builder.Entity<Category>(e =>
            {
                e.Property(x => x.CategoryName).HasMaxLength(50);
                e.Property(x => x.CategoryColor).HasMaxLength(20).HasDefaultValue("#8B7FD6");
            });
        }
    }
}
