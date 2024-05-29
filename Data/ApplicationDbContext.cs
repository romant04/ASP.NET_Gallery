using Gallery.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Gallery.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<StoredFile> Files { get; set; }
        public DbSet<Thumbnail> Thumbnails { get; set; }
        public DbSet<Album> Albums { get; set; }
        public DbSet<GalleryOrder> GalleryOrders { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Thumbnail>().HasKey(t => new { t.FileId, t.Type });
            modelBuilder.Entity<StoredFile>()
                .HasMany(f => f.Position)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}