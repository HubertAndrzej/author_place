using System.Data;

namespace AuthorPlace.Models.Services.Infrastructure.Interfaces;

public interface IDatabaseAccessor
{
    public Task<int> CommandAsync(FormattableString formattableSQL, CancellationToken token = default);
    public Task<DataSet> QueryAsync(FormattableString formattableSQL, CancellationToken token = default);
    public Task<T> ScalarAsync<T>(FormattableString formattableSQL, CancellationToken token = default);
}
