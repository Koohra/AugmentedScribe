using AugmentedScribe.Domain.Entities;
using AugmentedScribe.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AugmentedScribe.Infrastructure.Persistence;

public sealed class ScribeDbContext(DbContextOptions<ScribeDbContext> options)
    : IdentityDbContext<IdentityUser>(options)
{
    public DbSet<Campaign> Campaigns { get; set; }
    public DbSet<Book> Books { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Campaign>(entity =>
        {
            entity.ToTable("Campaigns");

            entity.HasKey(e => e.Id);

            entity.Property(c => c.Name)
                .HasMaxLength(100)
                .IsRequired();
            entity.Property(c => c.System)
                .HasMaxLength(50)
                .IsRequired();
            entity.Property(c => c.Description)
                .HasMaxLength(500);

            entity.HasOne<IdentityUser>()
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .IsRequired();
        });

        builder.Entity<Book>(entity =>
        {
            entity.ToTable("Books");

            entity.HasKey(e => e.Id);

            entity.Property(b => b.FileName)
                .HasMaxLength(255)
                .IsRequired();
            entity.Property(b => b.StorageUrl)
                .HasMaxLength(1024)
                .IsRequired();
            entity.Property(b => b.Status)
                .HasMaxLength(50)
                .IsRequired()
                .HasConversion(
                    v => v.ToString(),
                    v => Enum.Parse<BookStatus>(v));

            entity.HasOne(b => b.Campaign)
                .WithMany(c => c.Books)
                .HasForeignKey(b => b.CampaignId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}