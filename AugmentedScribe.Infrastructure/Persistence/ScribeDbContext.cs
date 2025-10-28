using AugmentedScribe.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AugmentedScribe.Infrastructure.Persistence;

public sealed class ScribeDbContext(DbContextOptions<ScribeDbContext> options)
    : IdentityDbContext<IdentityUser>(options)
{
    public DbSet<Campaign> Campaigns { get; set; }

    // descomentar estas linhas nas Fatia 3,
    // quando criarmos as entidades no projeto Domain.
    // public DbSet<Book> Books { get; set; }

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
    }
}