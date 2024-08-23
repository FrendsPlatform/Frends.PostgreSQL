using System.Globalization;
using Frends.PostgreSQL.ExecuteQuery.Definitions;
using Newtonsoft.Json.Linq;
using Npgsql;
using System.Data;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.Loader;

namespace Frends.PostgreSQL.ExecuteQuery;

/// <summary>
/// Task class.
/// </summary>
public static class PostgreSQL
{

    // For memory cleanup.
    static PostgreSQL()
    {
        var currentAssembly = Assembly.GetExecutingAssembly();
        var currentContext = AssemblyLoadContext.GetLoadContext(currentAssembly);
        if (currentContext != null)
            currentContext.Unloading += OnPluginUnloadingRequested;
    }

    /// <summary>
    /// Query data using PostgreSQL. [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends.PostgreSQL.ExecuteQuery)
    /// </summary>
    /// <param name="input">Query, parameters and connection string.</param>
    /// <param name="options">Set timeout and isolation level.</param>
    /// <param name="cancellationToken">Automatically generated and passed by Frends.</param>
    /// <returns>Result of the query. JToken QueryResult</returns>
    public static async Task<Result> ExecuteQuery([PropertyTab] Input input, [PropertyTab] Options options, CancellationToken cancellationToken)
    {
        Result result;
        using var conn = new NpgsqlConnection(input.ConnectionString);
        await conn.OpenAsync(cancellationToken);

        using var cmd = new NpgsqlCommand(input.Query, conn);
        cmd.CommandTimeout = options.CommandTimeoutSeconds;

        // Add parameters to command, if any were given.
        if (input.Parameters != null && input.Parameters.Length > 0)
        {
            foreach (var parameter in input.Parameters)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Convert parameter.Value to DBNull.Value if it is set to null.
                if (parameter.Value == null)
                    cmd.Parameters.AddWithValue(parameter.Name, DBNull.Value);
                else
                    cmd.Parameters.AddWithValue(parameter.Name, parameter.Value);
            }
        }

        // Execute command.

        if (input.Query.ToLower().Contains("select"))
        {
            var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            result = new Result(reader.ToJson(cancellationToken));
        }
        else
        {
            var transaction = conn.BeginTransaction(GetIsolationLevel(options.SqlTransactionIsolationLevel));
            cmd.Transaction = transaction;
            var rows = await cmd.ExecuteNonQueryAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            transaction.Dispose();
            result = new Result(JToken.FromObject(new { AffectedRows = rows }));
        }

        await conn.CloseAsync();

        return result;
    }

    #region HelperMethods

    // Extension method for NpgsqlDataReader to read the data and return it as JToken.
    private static JToken ToJson(this NpgsqlDataReader reader, CancellationToken cancellationToken)
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
                cancellationToken.ThrowIfCancellationRequested();
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
            TransactionIsolationLevel.None => IsolationLevel.Unspecified,
            TransactionIsolationLevel.RepeatableRead => IsolationLevel.RepeatableRead,
            TransactionIsolationLevel.ReadUncommited => IsolationLevel.ReadUncommitted,
            TransactionIsolationLevel.ReadCommited => IsolationLevel.ReadCommitted,
            TransactionIsolationLevel.Snapshot => IsolationLevel.Snapshot,
            TransactionIsolationLevel.Default => IsolationLevel.Serializable,
            TransactionIsolationLevel.Serializable => IsolationLevel.Serializable,
            _ => IsolationLevel.Serializable
        };
    }

    #endregion

    private static void OnPluginUnloadingRequested(AssemblyLoadContext obj)
    {
        obj.Unloading -= OnPluginUnloadingRequested;
    }
}
