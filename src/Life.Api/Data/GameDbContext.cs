using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace Life.Api.Data;

public class GameDbContext : DbContext
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = null,
        Converters = { new BoolArray2DConverter() }
    };

    public GameDbContext(DbContextOptions<GameDbContext> options)
        : base(options) { }

    public DbSet<Game> Games => Set<Game>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.Entity<Game>(entity => {
            entity.HasKey(game => game.Id);
            entity.Property(game => game.Id).IsRequired();
            entity.Property(game => game.RawBoard)
                .HasColumnName("Board")
                .HasColumnType("TEXT")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, JsonOptions),
                    v => JsonSerializer.Deserialize<bool[,]>(v, JsonOptions)!);
        });
}
