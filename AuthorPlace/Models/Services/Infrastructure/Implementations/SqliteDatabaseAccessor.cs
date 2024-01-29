using AuthorPlace.Models.Services.Infrastructure.Interfaces;
using System.Data;

namespace AuthorPlace.Models.Services.Infrastructure.Implementations;

public class SqliteDatabaseAccessor : IDatabaseAccessor
{
    public DataSet Query(string query)
    {
        throw new NotImplementedException();
    }
}
