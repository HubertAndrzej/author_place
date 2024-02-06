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
        modelBuilder.Entity<Album>();

        modelBuilder.Entity<Song>();

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}