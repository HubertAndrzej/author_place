namespace AuthorPlace.Models.ViewModels.Interfaces;

public interface IPaginationInfo
{
    public int CurrentPage { get; }
    public int TotalResults { get; }
    public int ResultsPerPage { get; }
    public string Search { get; }
    public string OrderBy { get; }
    public bool Ascending { get; }
}
