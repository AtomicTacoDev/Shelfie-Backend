using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Shelfie.Data.Models;
using Shelfie.Models;

namespace Shelfie.Data;

public class ApplicationDbContext : IdentityDbContext<User>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
        
    }
    
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
    
    public DbSet<Library> Libraries { get; set; } = null!;
    public DbSet<PlacedObject> PlacedObjects { get; set; } = null!;
    public DbSet<UserBook> UserBooks { get; set; } = null!;
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.HasDefaultSchema("identity");
        
        builder.Entity<RefreshToken>().ToTable("RefreshTokens", schema: "app");
        
        builder.Entity<Library>().ToTable("Libraries", schema: "app");
        builder.Entity<PlacedObject>().ToTable("PlacedObjects", schema: "app");
        builder.Entity<UserBook>().ToTable("UserBooks", schema: "app");
    }
}