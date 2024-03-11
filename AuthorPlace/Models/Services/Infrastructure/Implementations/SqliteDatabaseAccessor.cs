using AuthorPlace.Models.Exceptions.Infrastructure;
using AuthorPlace.Models.Options;
using AuthorPlace.Models.Services.Infrastructure.Interfaces;
using AuthorPlace.Models.ValueObjects;
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

    public async Task<int> CommandAsync(FormattableString formattableSQL, CancellationToken token)
    {
        try
        {
            using SqliteConnection connection = await GetConnection(token);
            using SqliteCommand command = GetCommand(formattableSQL, connection);
            int affectedRows = await command.ExecuteNonQueryAsync(token);
            return affectedRows;
        }
        catch (SqliteException exception) when (exception.SqliteErrorCode == 19)
        {
            throw new ConstraintViolationException(exception);
        }
    }

    public async Task<DataSet> QueryAsync(FormattableString formattableSQL, CancellationToken token)
    {
        try
        {
            using SqliteConnection connection = await GetConnection(token);
            using SqliteCommand command = GetCommand(formattableSQL, connection);
            using SqliteDataReader reader = await command.ExecuteReaderAsync(token);
            DataSet dataSet = new();
            do
            {
                DataTable dataTable = new();
                dataSet.Tables.Add(dataTable);
                dataTable.Load(reader);
            } while (!reader.IsClosed);
            return dataSet;
        }
        catch (SqliteException exception) when (exception.SqliteErrorCode == 19)
        {
            throw new ConstraintViolationException(exception);
        }
    }

    public async Task<T> ScalarAsync<T>(FormattableString formattableSQL, CancellationToken token)
    {
        try
        {
            using SqliteConnection connection = await GetConnection(token);
            using SqliteCommand command = GetCommand(formattableSQL, connection);
            object? result = await command.ExecuteScalarAsync(token);
            return (T)Convert.ChangeType(result!, typeof(T));
        }
        catch (SqliteException exception) when (exception.SqliteErrorCode == 19)
        {
            throw new ConstraintViolationException(exception);
        }
    }

private async Task<SqliteConnection> GetConnection(CancellationToken token)
    {
        string? connectionString = connectionStringsOptions.CurrentValue.Default;
        SqliteConnection connection = new(connectionString);
        await connection.OpenAsync(token);
        return connection;
    }

    private static SqliteCommand GetCommand(FormattableString formattableSQL, SqliteConnection connection)
    {
        object?[] queryArguments = formattableSQL.GetArguments();
        List<SqliteParameter> sqliteParameters = new();
        for (int i = 0; i < queryArguments.Length; i++)
        {
            if (queryArguments[i] is Sql)
            {
                continue;
            }
            SqliteParameter parameter = new(i.ToString(), queryArguments[i] ?? DBNull.Value);
            sqliteParameters.Add(parameter);
            queryArguments[i] = "@" + i;
        }
        string query = formattableSQL.ToString();
        SqliteCommand command = new(query, connection);
        command.Parameters.AddRange(sqliteParameters);
        return command;
    }
}
