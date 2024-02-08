using AuthorPlace.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthorPlace.Models.Services.Infrastructure.Implementations;

public partial class AuthorPlaceDbContext : DbContext
{
    public AuthorPlaceDbContext(DbContextOptions<AuthorPlaceDbContext> options) : base(options)
    {

    }

    public virtual DbSet<Album>? Albums { get; set; }

    public virtual DbSet<Song>? Songs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Album>(entity =>
        {
            entity.ToTable("Albums");
            entity.HasKey(album => album.Id);
            entity.OwnsOne(album => album.CurrentPrice, builder =>
            {
                builder.Property(money => money.Amount).HasColumnName("CurrentPrice_Amount");
                builder.Property(money => money.Currency).HasConversion<string>().HasColumnName("CurrentPrice_Currency");
            });
            entity.OwnsOne(album => album.FullPrice, builder =>
            {
                builder.Property(money => money.Amount).HasColumnName("FullPrice_Amount");
                builder.Property(money => money.Currency).HasConversion<string>().HasColumnName("FullPrice_Currency");
            });
            entity.HasMany(album => album.Songs).WithOne(song => song.Album).HasForeignKey(song => song.AlbumId);
        });

        modelBuilder.Entity<Song>(entity =>
        {
            entity.ToTable("Songs");
            entity.HasKey(song => song.Id);
            entity.HasOne(song => song.Album).WithMany(album => album.Songs).HasForeignKey(song => song.AlbumId);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}