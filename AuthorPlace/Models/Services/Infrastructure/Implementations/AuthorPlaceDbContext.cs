using AuthorPlace.Models.Entities;
using AuthorPlace.Models.Enums;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AuthorPlace.Models.Services.Infrastructure.Implementations;

public partial class AuthorPlaceDbContext : IdentityDbContext<ApplicationUser>
{
    public AuthorPlaceDbContext(DbContextOptions<AuthorPlaceDbContext> options) : base(options)
    {
    }

    public virtual DbSet<Album>? Albums { get; set; }

    public virtual DbSet<Song>? Songs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Album>(entity =>
        {
            entity.ToTable("Albums");
            entity.HasKey(album => album.Id);
            entity.HasIndex(entity => new { entity.Title, entity.Author }).IsUnique();
            entity.Property(album => album.RowVersion).IsRowVersion();
            entity.Property(album => album.Status).HasConversion<string>();
            entity.HasQueryFilter(album => album.Status != Status.Erased);
            entity.OwnsOne(album => album.CurrentPrice, builder =>
            {
                builder.Property(money => money.Amount).HasConversion<double>().HasColumnName("CurrentPrice_Amount");
                builder.Property(money => money.Currency).HasConversion<string>().HasColumnName("CurrentPrice_Currency");
            });
            entity.OwnsOne(album => album.FullPrice, builder =>
            {
                builder.Property(money => money.Amount).HasConversion<double>().HasColumnName("FullPrice_Amount");
                builder.Property(money => money.Currency).HasConversion<string>().HasColumnName("FullPrice_Currency");
            });
            entity.HasOne(album => album.User).WithMany(user => user.Albums).HasForeignKey(album => album.AuthorId);
            entity.HasMany(album => album.Songs).WithOne(song => song.Album).HasForeignKey(song => song.AlbumId);
        });

        modelBuilder.Entity<Song>(entity =>
        {
            entity.ToTable("Songs");
            entity.HasKey(song => song.Id);
            entity.Property(song => song.RowVersion).IsRowVersion();
            entity.HasQueryFilter(song => song.Album!.Status != Status.Erased);
            entity.HasOne(song => song.Album).WithMany(album => album.Songs).HasForeignKey(song => song.AlbumId);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}