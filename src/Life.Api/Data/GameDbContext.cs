using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace Life.Api.Data;

public class GameDbContext(DbContextOptions<GameDbContext> options) : DbContext(options)
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = null,
        Converters = { new BoolArray2DConverter() }
    };

    public DbSet<Game> Games => Set<Game>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.Entity<Game>(entity =>
        {
            entity.HasKey(game => game.Id);
            entity.Property(game => game.Id).IsRequired();
            entity.Property(game => game.RawBoard)
                .HasColumnName("Board")
                .HasColumnType("TEXT")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, _jsonOptions),
                    v => JsonSerializer.Deserialize<bool[,]>(v, _jsonOptions)!);
        });
}
