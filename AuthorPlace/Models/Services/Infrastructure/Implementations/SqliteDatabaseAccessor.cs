using AuthorPlace.Models.Options;
using AuthorPlace.Models.Services.Infrastructure.Interfaces;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using System.Data;

namespace AuthorPlace.Models.Services.Infrastructure.Implementations;

public class SqliteDatabaseAccessor : IDatabaseAccessor
{
    private readonly IOptionsMonitor<ConnectionStringsOptions> connectionStringsOptions;

    public SqliteDatabaseAccessor(IOptionsMonitor<ConnectionStringsOptions> connectionStringsOptions)
    {
        this.connectionStringsOptions = connectionStringsOptions;
    }

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
        string? connectionString = connectionStringsOptions.CurrentValue.Default;
        using SqliteConnection connection = new(connectionString);
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
