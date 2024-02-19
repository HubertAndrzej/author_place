using AuthorPlace.Models.InputModels;
using AuthorPlace.Models.ViewModels.Interfaces;

namespace AuthorPlace.Models.ViewModels;

public class AlbumListViewModel : IPaginationInfo
{
    public ListViewModel<AlbumViewModel>? Albums { get; set; }
    public AlbumListInputModel? Input { get; set; }
    int IPaginationInfo.CurrentPage => Input!.Page;
    int IPaginationInfo.TotalResults => Albums!.TotalCount;
    int IPaginationInfo.ResultsPerPage => Input!.Limit;
    string IPaginationInfo.Search => Input!.Search!;
    string IPaginationInfo.OrderBy => Input!.OrderBy;
    bool IPaginationInfo.Ascending => Input!.Ascending;
}
