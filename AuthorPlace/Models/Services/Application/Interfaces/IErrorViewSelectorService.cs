using AuthorPlace.Models.ValueObjects;

namespace AuthorPlace.Models.Services.Application.Interfaces;

public interface IErrorViewSelectorService
{
    ErrorViewData GetErrorViewData(HttpContext context);
}
