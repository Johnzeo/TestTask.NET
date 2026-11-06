
using Microsoft.EntityFrameworkCore;
using TestTask.Core.Entities;

namespace TestTask.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Video> Videos => Set<Video>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Video>(entity =>
        {
            entity.ToTable("videos");

            entity.HasKey(v => v.Id);

            entity.Property(v => v.CompanyName)
                .IsRequired()
                .HasMaxLength(200);

            entity.HasIndex(v => v.CompanyName)
                .IsUnique();

            entity.Property(v => v.FilePath)
                .IsRequired();

            entity.Property(v => v.VideoUrl)
                .IsRequired();

            entity.Property(v => v.CreatedAt)
                .IsRequired();

            entity.Property(v => v.UpdatedAt)
                .IsRequired();
        });
    }
}
