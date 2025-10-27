using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AugmentedScribe.Infrastructure.Persistence;

public sealed class ScribeDbContext(DbContextOptions<ScribeDbContext> options)
    : IdentityDbContext<IdentityUser>(options)
{
    // descomentar estas linhas nas Fatias 2 e 3,
    // quando criarmos as entidades no projeto Domain.
    // public DbSet<Campaign> Campaigns { get; set; }
    // public DbSet<Book> Books { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
    }
}