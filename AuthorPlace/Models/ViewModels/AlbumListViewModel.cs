using AuthorPlace.Models.InputModels;

namespace AuthorPlace.Models.ViewModels;

public class AlbumListViewModel
{
    public List<AlbumViewModel>? Albums { get; set; }
    public AlbumListInputModel? Input { get; set; }
}
