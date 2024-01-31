using AuthorPlace.Models.Services.Infrastructure.Interfaces;
using Microsoft.Data.Sqlite;
using System.Data;

namespace AuthorPlace.Models.Services.Infrastructure.Implementations;

public class SqliteDatabaseAccessor : IDatabaseAccessor
{
    public DataSet Query(FormattableString formattableQuery)
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
            connection.Open();
            using (SqliteCommand command = new SqliteCommand(query, connection))
            {
                command.Parameters.AddRange(sqliteParameters);
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
