using System.Data;

namespace AuthorPlace.Models.Services.Infrastructure.Interfaces;

public interface IDatabaseAccessor
{
    public IAsyncEnumerable<IDataRecord> QueryAsync(FormattableString query);
}
