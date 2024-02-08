using AuthorPlace.Models.Services.Infrastructure.Interfaces;
using Microsoft.Data.Sqlite;
using System.Data;

namespace AuthorPlace.Models.Services.Infrastructure.Implementations;

public class SqliteDatabaseAccessor : IDatabaseAccessor
{
    public async Task<DataSet> QueryAsync(FormattableString formattableQuery)
    {
        object?[] queryArguments = formattableQuery.GetArguments();
        List<SqliteParameter> sqliteParameters = new();
        for (int i = 0; i < queryArguments.Length; i++)
        {
            SqliteParameter parameter = new(i.ToString(), queryArguments[i]);
            sqliteParameters.Add(parameter);
            queryArguments[i] = "@" + i;
        }
        string query = formattableQuery.ToString();
        using SqliteConnection connection = new("Data Source = Data/AuthorPlace.db");
        await connection.OpenAsync();
        using SqliteCommand command = new(query, connection);
        command.Parameters.AddRange(sqliteParameters);
        using SqliteDataReader reader = await command.ExecuteReaderAsync();
        DataSet dataSet = new();
        do
        {
            DataTable dataTable = new();
            dataSet.Tables.Add(dataTable);
            dataTable.Load(reader);
        } while (!reader.IsClosed);
        return dataSet;
    }
}
