using System.Data;

namespace AuthorPlace.Models.Services.Infrastructure.Interfaces;

public interface IDatabaseAccessor
{
    public Task<int> CommandAsync(FormattableString formattableSQL);
    public Task<DataSet> QueryAsync(FormattableString formattableSQL);
    public Task<T> ScalarAsync<T>(FormattableString formattableSQL);
    public IAsyncEnumerable<IDataRecord> ExecuteAsync(FormattableString formattableSQL);
}
