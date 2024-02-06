using AuthorPlace.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthorPlace.Models.Services.Infrastructure.Implementations;

public partial class AuthorPlaceDbContext : DbContext
{
    public AuthorPlaceDbContext()
    {

    }

    public AuthorPlaceDbContext(DbContextOptions<AuthorPlaceDbContext> options) : base(options)
    {

    }

    public virtual DbSet<Album>? Albums { get; set; }

    public virtual DbSet<Song>? Songs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseSqlite("Data Source = Data/AuthorPlace.db");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Album>(entity =>
        {
            entity.Property(e => e.Author).HasColumnType("TEXT (100)");
            entity.Property(e => e.CurrentPriceAmount)
                .HasDefaultValueSql("0")
                .HasColumnType("NUMERIC")
                .HasColumnName("CurrentPrice_Amount");
            entity.Property(e => e.CurrentPriceCurrency)
                .HasDefaultValueSql("'EUR'")
                .HasColumnType("TEXT (3)")
                .HasColumnName("CurrentPrice_Currency");
            entity.Property(e => e.Description).HasColumnType("TEXT (10000)");
            entity.Property(e => e.Email).HasColumnType("TEXT (100)");
            entity.Property(e => e.FullPriceAmount)
                .HasDefaultValueSql("0")
                .HasColumnType("NUMERIC")
                .HasColumnName("FullPrice_Amount");
            entity.Property(e => e.FullPriceCurrency)
                .HasDefaultValueSql("'EUR'")
                .HasColumnType("TEXT (3)")
                .HasColumnName("FullPrice_Currency");
            entity.Property(e => e.ImagePath).HasColumnType("TEXT (100)");
            entity.Property(e => e.Title).HasColumnType("TEXT (100)");
        });

        modelBuilder.Entity<Song>(entity =>
        {
            entity.Property(e => e.Description).HasColumnType("TEXT (10000)");
            entity.Property(e => e.Duration)
                .HasDefaultValueSql("'00:00:00'")
                .HasColumnType("TEXT (8)");
            entity.Property(e => e.Title).HasColumnType("TEXT (100)");

            entity.HasOne(d => d.Album).WithMany(p => p.Songs).HasForeignKey(d => d.AlbumId);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}