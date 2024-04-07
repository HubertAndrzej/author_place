using AuthorPlace.Models.Entities;
using AuthorPlace.Models.Exceptions.Application;
using AuthorPlace.Models.InputModels.Songs;
using AuthorPlace.Models.Services.Application.Implementations.Songs;
using AuthorPlace.Models.Services.Application.Interfaces.Songs;
using AuthorPlace.Models.Services.Infrastructure.Implementations;
using AuthorPlace.Models.ViewModels.Songs;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using Moq;

namespace AuthorPlaceTests.Models.Services.Application.Implementations.Songs;

[TestFixture]
public class EFCoreSongServiceTests
{
    private readonly AuthorPlaceDbContext _ctx;
    private readonly ISongService _songService;

    public EFCoreSongServiceTests()
    {
        DbContextOptionsBuilder<AuthorPlaceDbContext> builder = new();
        DbContextOptions<AuthorPlaceDbContext> options = builder.UseInMemoryDatabase("AuthorPlaceTestDatabase").Options;
        _ctx = new AuthorPlaceDbContext(options);
        Mock<ILoggerFactory> loggerFactoryMock = new();
        Mock<ILogger<EFCoreSongService>> loggerMock = new();
        loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
        _songService = new EFCoreSongService(_ctx, loggerFactoryMock.Object);
    }

    [SetUp]
    public void SetUp()
    {
        _ctx.Albums!.AddRange(MockAlbums());
        _ctx.Songs!.AddRange(MockSongs());
        _ctx.SaveChanges();
    }

    [TearDown]
    public void TearDown()
    {
        List<Album> albums = _ctx.Albums!.ToList();
        List<Song> songs = _ctx.Songs!.ToList();
        _ctx.Albums!.RemoveRange(albums);
        _ctx.Songs!.RemoveRange(songs);
        _ctx.SaveChanges();
        foreach (EntityEntry entry in _ctx.ChangeTracker.Entries())
        {
            entry.State = EntityState.Detached;
        }
        _ctx.Database.EnsureDeleted();
        _ctx.Database.EnsureCreated();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _ctx.Dispose();
    }

    private static List<Album> MockAlbums()
    {
        return new List<Album>()
        {
            new("Album 1", "Hub Sobo", "A1B2C3D4-E5F6-A7B8-C9D0-E0123456789F", "hub.sobo@example.com")
        };
    }

    private static List<Song> MockSongs()
    {
        return new List<Song>()
        {
            new() { Id = 1, AlbumId = 1, Title = "Song 1", Description = "Description 1", Duration = TimeSpan.FromSeconds(30), RowVersion = "Row Version" },
            new() { Id = 2, AlbumId = 1, Title = "Song 2", Description = "Description 2", Duration = TimeSpan.FromSeconds(30), RowVersion = "Row Version" }
        };
    }

    [Test]
    [TestCase(1)]
    [TestCase(2)]
    public void GetSong_WhenSongExists_ReturnSongDetailViewModel(int id)
    {
        // Arrange
        Song song = MockSongs().ElementAt(id - 1);
        SongDetailViewModel expected = new()
        {
            AlbumId = song.AlbumId,
            Description = song.Description,
            Duration = song.Duration,
            Id = song.Id,
            Title = song.Title
        };

        // Act
        SongDetailViewModel actual = _songService.GetSong(id);

        // Assert
        expected.Should().BeEquivalentTo(actual);
    }

    [Test]
    public void GetSong_WhenSongNotExists_ThrowSongNotFoundException()
    {
        // Arrange
        int id = 3;
        string message = "Song 3 not found";

        // Act
        Action act = () => _songService.GetSong(id);

        // Assert
        act.Should().Throw<SongNotFoundException>(message);
    }

    [Test]
    public void CreateSong_WhenSongCreatedSuccessfully_ReturnSongDetailViewModel()
    {
        // Arrange
        SongCreateInputModel inputModel = new()
        {
            AlbumId = 1,
            Title = "Song 3"
        };
        SongDetailViewModel expected = new()
        {
            AlbumId = 1,
            Description = null,
            Duration = new TimeSpan(),
            Id = 3,
            Title = "Song 3"
        };
        int beforeCount = _ctx.Songs!.Count();
        beforeCount.Should().Be(2);

        // Act
        SongDetailViewModel actual = _songService.CreateSong(inputModel);

        // Assert
        expected.Should().BeEquivalentTo(actual);
        int afterCount = _ctx.Songs!.Count();
        afterCount.Should().Be(3);
    }

    [Test]
    [TestCase(1)]
    [TestCase(2)]
    public void GetSongForEditing_WhenSongExists_ReturnSongUpdateInputModel(int id)
    {
        // Arrange
        Song song = MockSongs().ElementAt(id - 1);
        SongUpdateInputModel expected = new()
        {
            AlbumId = song.AlbumId,
            Description = song.Description,
            Duration = song.Duration,
            Id = song.Id,
            Title = song.Title,
            RowVersion = song.RowVersion
        };

        // Act
        SongUpdateInputModel actual = _songService.GetSongForEditing(id);

        // Assert
        expected.Should().BeEquivalentTo(actual);
    }

    [Test]
    public void GetSongForEditing_WhenSongNotExists_ThrowSongNotFoundException()
    {
        // Arrange
        int id = 3;
        string message = "Song 3 not found";

        // Act
        Action act = () => _songService.GetSongForEditing(id);

        // Assert
        act.Should().Throw<SongNotFoundException>(message);
    }

    [Test]
    public void UpdateSong_WhenSongUpdatedSuccessfully_ReturnSongDetailViewModel()
    {
        // Arrange
        SongUpdateInputModel inputModel = new()
        {
            Id = 1,
            AlbumId = 1,
            Title = "New Title",
            Description = "New Description",
            Duration = TimeSpan.FromSeconds(30),
            RowVersion = "Row Version"
        };
        SongDetailViewModel expected = new()
        {
            Id = 1,
            AlbumId = 1,
            Title = "New Title",
            Description = "New Description",
            Duration = TimeSpan.FromSeconds(30)
        };

        // Act
        SongDetailViewModel actual = _songService.UpdateSong(inputModel);

        // Assert
        expected.Should().BeEquivalentTo(actual);
    }

    [Test]
    public void UpdateSong_WhenSongNotExists_ThrowSongNotFoundException()
    {
        // Arrange
        SongUpdateInputModel inputModel = new()
        {
            Id = 3,
            AlbumId = 1,
            Title = "New Title",
            Description = "New Description",
            Duration = TimeSpan.FromSeconds(30),
            RowVersion = "Row Version"
        };
        string message = "Song 3 not found";

        // Act
        Action act = () => _songService.UpdateSong(inputModel);

        // Assert
        act.Should().Throw<SongNotFoundException>(message);
    }

    [Test]
    public void UpdateSong_WhenDbUpdateConcurrencyException_ThrowOptimisticConcurrencyException()
    {
        // Arrange
        SongUpdateInputModel inputModel = new()
        {
            Id = 1,
            AlbumId = 1,
            Title = "New Title",
            Description = "New Description",
            Duration = TimeSpan.FromSeconds(30),
            RowVersion = "Different Row Version"
        };
        string message = "Song 3 not found";

        // Act
        Action act = () => _songService.UpdateSong(inputModel);

        // Assert
        act.Should().Throw<OptimisticConcurrencyException>(message);
        foreach (EntityEntry entry in _ctx.ChangeTracker.Entries())
        {
            entry.State = EntityState.Detached;
        }
        _ctx.SaveChanges();
    }

    [Test]
    public void RemoveSong_WhenSongRemovedSuccessfully_NoReturn()
    {
        // Arrange
        SongDeleteInputModel inputModel = new()
        {
            Id = 1,
            AlbumId = 1
        };
        int beforeCount = _ctx.Songs!.Count();
        beforeCount.Should().Be(2);

        // Act
        _songService.RemoveSong(inputModel);

        // Assert
        int afterCount = _ctx.Songs!.Count();
        afterCount.Should().Be(1);
    }

    [Test]
    public void RemoveSong_WhenSongNotExists_ThrowSongNotFoundException()
    {
        // Arrange
        SongDeleteInputModel inputModel = new()
        {
            Id = 3,
            AlbumId = 1
        };
        string message = "Song 3 not found";

        // Act
        Action act = () => _songService.RemoveSong(inputModel);

        // Assert
        act.Should().Throw<SongNotFoundException>(message);
    }
}