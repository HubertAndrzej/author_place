﻿namespace AuthorPlace.Models.Options;

public class AlbumsOptions
{
    public int PerPage { get; set; }
    public int InHome { get; set; }
    public AlbumsOrderOptions? Order { get; set; }
}