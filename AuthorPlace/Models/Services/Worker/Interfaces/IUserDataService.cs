namespace AuthorPlace.Models.Services.Worker.Interfaces;

public interface IUserDataService
{
    public void EnqueueUserDataDownload(string userId);
    public string GetUserDataZipFileLocation(string userId, Guid downloadFileId);
    public IEnumerable<string> EnumerateAllUserDataZipFileLocations();
}
