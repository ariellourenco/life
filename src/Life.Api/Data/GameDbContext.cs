using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Life.Api.Data;

public sealed class GameDbContext(DbContextOptions<GameDbContext> options) : IdentityDbContext<Gamer, IdentityRole<int>, int>(options)
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = null,
        Converters = { new BoolArray2DConverter() }
    };

    public DbSet<Game> Games => Set<Game>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Gamer>().ToTable("Users");
        builder.Entity<Game>(entity =>
        {
            entity.ToTable("Games");
            entity.HasKey(game => game.Id);
            entity.Property(game => game.Id).IsRequired();
            entity.Property(game => game.RawBoard)
                .HasColumnName("Board")
                .HasColumnType("TEXT")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, _jsonOptions),
                    v => JsonSerializer.Deserialize<bool[,]>(v, _jsonOptions)!);

            entity.HasOne<Gamer>()
                .WithMany()
                .HasForeignKey(game => game.PlayerId)
                .HasPrincipalKey(game => game.Id);
        });

        ConfigureIdentityTables(builder);
    }

    private static void ConfigureIdentityTables(ModelBuilder builder)
    {
        builder.Entity<IdentityUserClaim<int>>().ToTable("Claims");
        builder.Entity<IdentityUserLogin<int>>().ToTable("Logins");
        builder.Entity<IdentityUserToken<int>>().ToTable("Tokens");
        builder.Entity<IdentityRole<int>>().ToTable("Roles");
        builder.Entity<IdentityRoleClaim<int>>().ToTable("RoleClaims");
        builder.Entity<IdentityUserRole<int>>().ToTable("UserRoles");
    }
}
