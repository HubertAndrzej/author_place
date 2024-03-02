using AuthorPlace.Models.ValueObjects;

namespace AuthorPlace.Models.Services.Application.Interfaces.Errors;

public interface IErrorViewSelectorService
{
    ErrorViewData GetErrorViewData(HttpContext context);
}
