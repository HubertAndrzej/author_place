using AuthorPlace.Models.Entities;
using AuthorPlace.Models.Exceptions.Application;
using AuthorPlace.Models.Extensions;
using AuthorPlace.Models.InputModels.Songs;
using AuthorPlace.Models.Services.Application.Implementations.Songs;
using AuthorPlace.Models.Services.Application.Interfaces.Songs;
using AuthorPlace.Models.Services.Infrastructure.Implementations;
using AuthorPlace.Models.ViewModels.Songs;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace AuthorPlaceTests.Models.Services.Application.Implementations.Songs;

[TestFixture]
public class EFCoreSongServiceTests
{
    private Mock<AuthorPlaceDbContext> ctx;
    private ISongService songService;

    public static List<Album> MockAlbums()
    {
        return new List<Album>()
        {
            new("Album 1", "Hub Sobo", "A1B2C3D4-E5F6-A7B8-C9D0-E0123456789F", "hub.sobo@example.com")
        };
    }

    public static List<Song> MockSongs()
    {
        return new List<Song>()
        {
            new() { Id = 1, AlbumId = 1, Title = "Song 1", Description = "Description 1", Duration = TimeSpan.FromSeconds(30), RowVersion = null, Album = new("Album 1", "Hub Sobo", "A1B2C3D4-E5F6-A7B8-C9D0-E0123456789F", "hub.sobo@example.com") },
            new() { Id = 2, AlbumId = 1, Title = "Song 2", Description = "Description 2", Duration = TimeSpan.FromSeconds(30), RowVersion = null, Album = new("Album 1", "Hub Sobo", "A1B2C3D4-E5F6-A7B8-C9D0-E0123456789F", "hub.sobo@example.com") }
        };
    }

    public static Mock<AuthorPlaceDbContext> SetAlbumsDbSet(Mock<AuthorPlaceDbContext> ctx, List<Album> albums)
    {
        Mock<DbSet<Album>> dbAlbumsSet = new();
        IQueryable<Album> dataQueryable = albums.AsQueryable();
        dbAlbumsSet.As<IQueryable<Album>>().Setup(x => x.Provider).Returns(dataQueryable.Provider);
        dbAlbumsSet.As<IQueryable<Album>>().Setup(x => x.Expression).Returns(dataQueryable.Expression);
        dbAlbumsSet.As<IQueryable<Album>>().Setup(x => x.ElementType).Returns(dataQueryable.ElementType);
        dbAlbumsSet.As<IQueryable<Album>>().Setup(x => x.GetEnumerator()).Returns(dataQueryable.GetEnumerator());
        ctx.Setup(x => x.Albums).Returns(dbAlbumsSet.Object);
        return ctx;
    }

    public static Mock<AuthorPlaceDbContext> SetSongsDbSet(Mock<AuthorPlaceDbContext> ctx, List<Song> songs)
    {
        Mock<DbSet<Song>> dbSongSet = new();
        IQueryable<Song> dataQueryable = songs.AsQueryable();
        dbSongSet.As<IQueryable<Song>>().Setup(x => x.Provider).Returns(dataQueryable.Provider);
        dbSongSet.As<IQueryable<Song>>().Setup(x => x.Expression).Returns(dataQueryable.Expression);
        dbSongSet.As<IQueryable<Song>>().Setup(x => x.ElementType).Returns(dataQueryable.ElementType);
        dbSongSet.As<IQueryable<Song>>().Setup(x => x.GetEnumerator()).Returns(dataQueryable.GetEnumerator());
        ctx.Setup(x => x.Songs).Returns(dbSongSet.Object);
        return ctx;
    }
    public static Mock<AuthorPlaceDbContext> GetAuthorPlaceDbContext()
    {
        DbContextOptions<AuthorPlaceDbContext> options = new DbContextOptionsBuilder<AuthorPlaceDbContext>().UseInMemoryDatabase(databaseName: "MockAuthorPlaceDB").Options;
        Mock<AuthorPlaceDbContext> ctx = new(options);
        ctx = SetAlbumsDbSet(ctx, MockAlbums());
        ctx = SetSongsDbSet(ctx, MockSongs());
        return ctx;
    }

    public ISongService GetSongService(Mock<AuthorPlaceDbContext> ctx)
    {
        Mock<ILoggerFactory> loggerFactoryMock = new();
        Mock<ILogger<EFCoreSongService>> loggerMock = new();
        loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
        return new EFCoreSongService(ctx.Object, loggerFactoryMock.Object);
    }

    [SetUp]
    public void SetUp()
    {
        ctx = GetAuthorPlaceDbContext();
        songService = GetSongService(ctx);
    }

    [Test]
    [TestCase(1)]
    [TestCase(2)]
    public void GetSong_WhenSongExists_ReturnSongDetailViewModel(int id)
    {
        // Arrange
        SongDetailViewModel expected = new()
        {
            AlbumId = 1,
            Description = $"Description {id}",
            Duration = TimeSpan.FromSeconds(30),
            Id = id,
            Title = $"Song {id}"
        };

        // Act
        SongDetailViewModel actual = songService.GetSong(id);

        // Assert
        expected.Should().BeEquivalentTo(actual);
    }

    [Test]
    public void GetSong_WhenSongNotExists_ThrowSongNotFoundException()
    {
        // Arrange
        string message = "Song 3 not found";

        // Act
        Action act = () => songService.GetSong(3);

        // Assert
        act.Should().Throw<SongNotFoundException>(message);
    }
}
