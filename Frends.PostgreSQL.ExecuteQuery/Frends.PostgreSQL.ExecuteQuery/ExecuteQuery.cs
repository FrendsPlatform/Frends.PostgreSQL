using System.Globalization;
using Frends.PostgreSQL.ExecuteQuery.Definitions;
using Newtonsoft.Json.Linq;
using Npgsql;
using System.Data;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.Loader;
using System.Data.Common;

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

		try
		{
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

			if (options.SqlTransactionIsolationLevel is TransactionIsolationLevel.None)
				result = await ExecuteHandler(input, options, cmd, cancellationToken);
			else
			{
				using var transaction = conn.BeginTransaction(GetIsolationLevel(options.SqlTransactionIsolationLevel));
				cmd.Transaction = transaction;
				result = await ExecuteHandler(input, options, cmd, cancellationToken);
			}

			return result;

		}
		catch (Exception ex)
		{
			var eMsg = $"ExecuteQuery exception: {ex}.";

			if (options.ThrowErrorOnFailure)
				throw new Exception(eMsg);

			return new Result(false, 0, eMsg, null);
		}
    }

	private static async Task<Result> ExecuteHandler(Input input, Options options, NpgsqlCommand cmd, CancellationToken cancellationToken)
	{
		Result result;
		object dataObject;
		NpgsqlDataReader dataReader = null;
		using var table = new DataTable();

		try
		{
			switch (input.ExecuteType)
			{
				case ExecuteTypes.Auto:
					if (input.Query.ToLower().StartsWith("select"))
					{
						dataReader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
						table.Load(dataReader);
						result = new Result(true, dataReader.RecordsAffected, null, JToken.FromObject(table));
						await dataReader.CloseAsync();
						break;
					}
					dataObject = await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
					result = new Result(true, (int)dataObject, null, JToken.FromObject(new { AffectedRows = dataObject }));
					break;
				case ExecuteTypes.NonQuery:
					dataObject = await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
					result = new Result(true, (int)dataObject, null, JToken.FromObject(new { AffectedRows = dataObject }));
					break;
				case ExecuteTypes.Scalar:
					dataObject = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
					result = new Result(true, 1, null, JToken.FromObject(new { Value = dataObject }));
					break;
				case ExecuteTypes.ExecuteReader:
					dataReader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
					table.Load(dataReader);
					result = new Result(true, dataReader.RecordsAffected, null, JToken.FromObject(table));
					await dataReader.CloseAsync();
					break;
				default:
					throw new NotSupportedException();
			}

			if (cmd.Transaction != null)
				await cmd.Transaction.CommitAsync(cancellationToken);

			return result;
		}
		catch (Exception ex)
		{
			if (dataReader != null && !dataReader.IsClosed)
				await dataReader.CloseAsync();

			if (cmd.Transaction is null)
			{
				if (options.ThrowErrorOnFailure)
					throw new Exception("ExecuteHandler exception: 'Options.SqlTransactionIsolationLevel = None', so there was no transaction rollback.", ex);
				else
					return new Result(false, 0, $"ExecuteHandler exception: 'Options.SqlTransactionIsolationLevel = None', so there was no transaction rollback. {ex}", null);
			}
			else
			{
				try
				{
					await cmd.Transaction.RollbackAsync(cancellationToken);
				}
				catch (Exception rollbackEx)
				{
					if (options.ThrowErrorOnFailure)
						throw new Exception("ExecuteHandler exception: An exception occurred on transaction rollback.", rollbackEx);
					else
						return new Result(false, 0, $"ExecuteHandler exception: An exception occurred on transaction rollback. Rollback exception: {rollbackEx}. ||  Exception leading to rollback: {ex}", null);
				}

				if (options.ThrowErrorOnFailure)
					throw new Exception("ExecuteHandler exception: (If required) transaction rollback completed without exception.", ex);
				else
					return new Result(false, 0, $"ExecuteHandler exception: (If required) transaction rollback completed without exception. {ex}.", null);
			}
		}
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
