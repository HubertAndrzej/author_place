using System.Data;

namespace AuthorPlace.Models.Services.Infrastructure.Interfaces;

public interface IDatabaseAccessor
{
    public Task<DataSet> QueryAsync(FormattableString query);
}
