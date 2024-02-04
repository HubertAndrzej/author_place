using AuthorPlace.Models.Services.Infrastructure.Interfaces;
using Microsoft.Data.Sqlite;
using System.Data;

namespace AuthorPlace.Models.Services.Infrastructure.Implementations;

public class SqliteDatabaseAccessor : IDatabaseAccessor
{
    public async Task<DataSet> QueryAsync(FormattableString formattableQuery)
    {
        object?[] queryArguments = formattableQuery.GetArguments();
        List<SqliteParameter> sqliteParameters = new List<SqliteParameter>();
        for (int i = 0; i < queryArguments.Length; i++)
        {
            SqliteParameter parameter = new SqliteParameter(i.ToString(), queryArguments[i]);
            sqliteParameters.Add(parameter);
            queryArguments[i] = "@" + i;
        }
        string query = formattableQuery.ToString();
        using (SqliteConnection connection = new SqliteConnection("Data Source = Data/AuthorPlace.db"))
        {
            await connection.OpenAsync();
            using (SqliteCommand command = new SqliteCommand(query, connection))
            {
                command.Parameters.AddRange(sqliteParameters);
                using (SqliteDataReader reader = await command.ExecuteReaderAsync())
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
