using AuthorPlace.Controllers;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace AuthorPlace.Models.InputModels;

public class AlbumCreateInputModel
{
    [Required(ErrorMessage = "The title is mandatory and cannot be made only of empty spaces")]
    [Remote(action: nameof(AlbumsController.IsAlbumUnique), controller: "Albums", ErrorMessage = "This title is already used by this author")]
    public string? Title { get; set; }
}
