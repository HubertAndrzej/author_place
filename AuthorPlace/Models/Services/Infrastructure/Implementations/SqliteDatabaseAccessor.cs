using AuthorPlace.Models.Services.Infrastructure.Interfaces;
using Microsoft.Data.Sqlite;
using System.Data;

namespace AuthorPlace.Models.Services.Infrastructure.Implementations;

public class SqliteDatabaseAccessor : IDatabaseAccessor
{
    public DataSet Query(string query)
    {
        using (SqliteConnection connection = new SqliteConnection("Data Source = Data/AuthorPlace.db"))
        {
            connection.Open();
            using (SqliteCommand command = new SqliteCommand(query, connection))
            {
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    DataSet dataSet = new DataSet();
                    do
                    {
                        DataTable dataTable = new DataTable();
                        dataSet.Tables.Add(dataTable);
                        dataTable.Load(reader);
                    } while (!reader.IsClosed);
                    return dataSet;
                }
            }
        }
    }
}
