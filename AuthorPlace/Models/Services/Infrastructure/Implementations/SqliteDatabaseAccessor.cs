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

    public async Task<int> CommandAsync(FormattableString formattableSQL)
    {
        try
        {
            using SqliteConnection connection = await GetConnection();
            using SqliteCommand command = GetCommand(formattableSQL, connection);
            int affectedRows = await command.ExecuteNonQueryAsync();
            return affectedRows;
        }
        catch (SqliteException exception) when (exception.SqliteErrorCode == 19)
        {
            throw new ConstraintViolationException(exception);
        }
    }

    public async Task<DataSet> QueryAsync(FormattableString formattableSQL)
    {
        try
        {
            using SqliteConnection connection = await GetConnection();
            using SqliteCommand command = GetCommand(formattableSQL, connection);
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
        catch (SqliteException exception) when (exception.SqliteErrorCode == 19)
        {
            throw new ConstraintViolationException(exception);
        }
    }

    public async Task<T> ScalarAsync<T>(FormattableString formattableSQL)
    {
        try
        {
            using SqliteConnection connection = await GetConnection();
            using SqliteCommand command = GetCommand(formattableSQL, connection);
            object? result = await command.ExecuteScalarAsync();
            return (T)Convert.ChangeType(result!, typeof(T));
        }
        catch (SqliteException exception) when (exception.SqliteErrorCode == 19)
        {
            throw new ConstraintViolationException(exception);
        }
    }

private async Task<SqliteConnection> GetConnection()
    {
        string? connectionString = connectionStringsOptions.CurrentValue.Default;
        SqliteConnection connection = new(connectionString);
        await connection.OpenAsync();
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
