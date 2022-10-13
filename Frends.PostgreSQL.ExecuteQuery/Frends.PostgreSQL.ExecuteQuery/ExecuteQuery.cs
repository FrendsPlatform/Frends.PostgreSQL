using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Frends.PostgreSQL.ExecuteQuery.Definitions;
using Newtonsoft.Json.Linq;
using Npgsql;
using System.Data;
using System.ComponentModel;

#pragma warning disable 1591

namespace Frends.PostgreSQL.ExecuteQuery;

public static class PostgreSQL
{
    /// <summary>
    /// Query data using PostgreSQL. [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends.PostgreSQL.ExecuteQuery)
    /// </summary>
    /// <param name="input">Query, parameters and connection string.</param>
    /// <param name="options">Set timeout and isolation level.</param>
    /// <param name="cancellationToken">Automatically generated and passed by Frends.</param>
    /// <returns>Result of the query. JToken QueryResult</returns>
    public static async Task<Result> ExecuteQuery([PropertyTab] Input input, [PropertyTab] Options options, CancellationToken cancellationToken)
    {
        using (var conn = new NpgsqlConnection(input.ConnectionString))
        {
            await conn.OpenAsync(cancellationToken);
            var transaction = conn.BeginTransaction(GetIsolationLevel(options.SqlTransactionIsolationLevel));
            using (var cmd = new NpgsqlCommand(input.Query, conn))
            {
                cmd.CommandTimeout = options.CommandTimeoutSeconds;
                cmd.Transaction = transaction;

                // Add parameters to command, if any were given.
                if (input.Parameters != null && input.Parameters.Length > 0)
                {
                    foreach (var parameter in input.Parameters)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        // Convert parameter.Value to DBNull.Value if it is set to null.
                        if (parameter.Value == null)
                        {
                            cmd.Parameters.AddWithValue(parameter.Name, DBNull.Value);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue(parameter.Name, parameter.Value);
                        }
                    }
                }

                // Execute command.

                if (input.Query.ToLower().Contains("select"))
                {
                    var reader = await cmd.ExecuteReaderAsync(cancellationToken);
                    var result = reader.ToJson();
                    conn.Close();
                    return new Result(result);
                }
                else
                {
                    var rows = await cmd.ExecuteNonQueryAsync(cancellationToken);
                    transaction.Commit();
                    conn.Close();
                    return new Result(JToken.FromObject(new { AffectedRows = rows }));
                }
            }
        }
    }

    #region HelperMethods

    // Extension method for NpgsqlDataReader to read the data and return it as JToken.
    private static JToken ToJson(this NpgsqlDataReader reader)
    {
        // Create JSON result.
        using (var writer = new JTokenWriter())
        {
            writer.Formatting = Newtonsoft.Json.Formatting.Indented;
            writer.Culture = CultureInfo.InvariantCulture;

            // Start array.
            writer.WriteStartArray();

            while (reader.Read())
            {
                // Start row object.
                writer.WriteStartObject();
                for (var i = 0; i < reader.FieldCount; i++)
                {
                    // Add row element name.
                    writer.WritePropertyName(reader.GetName(i));

                    // Add row element value.
                    writer.WriteValue(reader.GetValue(i) ?? string.Empty);
                }

                // End row object.
                writer.WriteEndObject();
            }
            // End array.
            writer.WriteEndArray();

            return writer.Token;
        }
    }

    // Determine transaction isolation level from Options-class.
    private static IsolationLevel GetIsolationLevel(TransactionIsolationLevel level)
    {
        return level switch
        {
            TransactionIsolationLevel.None           => IsolationLevel.Unspecified,
            TransactionIsolationLevel.RepeatableRead => IsolationLevel.RepeatableRead,
            TransactionIsolationLevel.ReadUncommited => IsolationLevel.ReadUncommitted,
            TransactionIsolationLevel.ReadCommited   => IsolationLevel.ReadCommitted,
            TransactionIsolationLevel.Snapshot       => IsolationLevel.Snapshot,
            TransactionIsolationLevel.Default        => IsolationLevel.Serializable,
            TransactionIsolationLevel.Serializable   => IsolationLevel.Serializable,
                                                   _ => IsolationLevel.Serializable,
        };
    }

    #endregion
}
