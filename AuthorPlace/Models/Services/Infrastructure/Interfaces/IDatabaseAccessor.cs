using System.Data;

namespace AuthorPlace.Models.Services.Infrastructure.Interfaces;

public interface IDatabaseAccessor
{
    public Task<DataSet> ExecuteAsync(FormattableString query);
    public IAsyncEnumerable<IDataRecord> QueryAsync(FormattableString query);
}
