using System.ComponentModel.DataAnnotations;

namespace AuthorPlace.Models.Enums;

public enum Status
{
    [Display(Name = "Draft")]
    Drafted,

    [Display(Name = "Public")]
    Published,

    [Display(Name = "Hidden")]
    Erased
}
