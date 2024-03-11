using AuthorPlace.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace AuthorPlace.Models.InputModels.Users;

public class UserRoleInputModel
{
    [Display(Name = "Email")]
    public string? Email { get; set; }

    [Display(Name = "Role")]
    public Role Role { get; set; }
}
