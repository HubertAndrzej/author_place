﻿using Microsoft.AspNetCore.Identity;

namespace AuthorPlace.Models.Entities;

public class ApplicationUser : IdentityUser
{
    public string? FullName { get; set; }
    public virtual ICollection<Album> Albums { get; set; } = new List<Album>();
}
