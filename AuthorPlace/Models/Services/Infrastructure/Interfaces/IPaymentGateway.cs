using AuthorPlace.Models.InputModels.Albums;

namespace AuthorPlace.Models.Services.Infrastructure.Interfaces;

public interface IPaymentGateway
{
    public Task<string> GetPaymentUrlAsync(AlbumPayInputModel inputModel);
    public Task<AlbumSubscribeInputModel> CapturePaymentAsync(string token);
}
