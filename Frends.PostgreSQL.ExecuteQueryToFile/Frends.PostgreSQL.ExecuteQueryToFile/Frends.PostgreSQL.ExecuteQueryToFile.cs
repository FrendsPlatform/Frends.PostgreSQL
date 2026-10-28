using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Frends.PostgreSQL.ExecuteQueryToFile.Definitions;
using Frends.PostgreSQL.ExecuteQueryToFile.Helpers;
using Newtonsoft.Json.Linq;
using Npgsql;

namespace Frends.PostgreSQL.ExecuteQueryToFile;

/// <summary>
/// Task Class for PostgreSQL operations.
/// </summary>
public static class PostgreSQL
{
    /// <summary>
    /// Executes a PostgreSQL query and writes the results directly into a CSV or JSON file.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends-PostgreSQL-ExecuteQueryToFile)
    /// </summary>
    /// <frendsdocs>
    /// The result set is streamed from the database to the file row by row, so the amount of data
    /// is not limited by memory and nothing is returned to the Process except a summary.
    /// The data is first written to a temporary file next to the output file and moved into place
    /// when the query has completed, so a failed run never leaves a partial output file behind.
    /// Binary columns are written as Base64 text, and JSON/JSONB columns are written as JSON
    /// into JSON output and as text into CSV output.
    /// </frendsdocs>
    /// <param name="input">Essential parameters.</param>
    /// <param name="connection">Connection parameters.</param>
    /// <param name="options">Additional parameters.</param>
    /// <param name="cancellationToken">A cancellation token provided by Frends Platform.</param>
    /// <returns>object { bool Success, long RowsWritten, string FilePath, object Error { string Message, Exception AdditionalInfo } }</returns>
    public static async Task<Result> ExecuteQueryToFile(
        [PropertyTab] Input input,
        [PropertyTab] Connection connection,
        [PropertyTab] Options options,
        CancellationToken cancellationToken)
    {
        try
        {
            ValidationHandler.Run(input, connection);
            cancellationToken.ThrowIfCancellationRequested();

            var filePath = Path.GetFullPath(input.OutputFilePath);

            if (string.IsNullOrEmpty(Path.GetFileName(filePath)))
                throw new ArgumentException("Output file path must point to a file.");

            if (input.ActionOnExistingFile == ActionOnExistingFile.Throw && File.Exists(filePath))
                throw new IOException($"Output file already exists: {filePath}");

            var tempPath = Path.Combine(Path.GetDirectoryName(filePath)!, $".{Guid.NewGuid():N}.tmp");

            try
            {
                long rows;

                await using (var stream = new FileStream(
                                 tempPath,
                                 FileMode.CreateNew,
                                 FileAccess.Write,
                                 FileShare.None,
                                 options.FileBufferSize))
                {
                    rows = await QueryToStream(input, connection, options, stream, cancellationToken)
                        .ConfigureAwait(false);
                }

                File.Move(tempPath, filePath, input.ActionOnExistingFile == ActionOnExistingFile.Overwrite);

                return new Result
                {
                    Success = true,
                    RowsWritten = rows,
                    FilePath = filePath,
                    Error = null,
                };
            }
            finally
            {
                if (File.Exists(tempPath)) File.Delete(tempPath);
            }
        }
        catch (Exception ex)
        {
            return ex.Handle(options);
        }
    }

    private static async Task<long> QueryToStream(
        Input input,
        Connection connection,
        Options options,
        Stream stream,
        CancellationToken cancellationToken)
    {
        await using var npgsqlConnection = new NpgsqlConnection(connection.ConnectionString);
        await npgsqlConnection.OpenAsync(cancellationToken).ConfigureAwait(false);

        await using var command = new NpgsqlCommand(input.Query, npgsqlConnection);
        command.CommandTimeout = options.CommandTimeoutSeconds;

        if (input.Parameters != null)
        {
            foreach (var parameter in input.Parameters)
                command.Parameters.AddWithValue(parameter.Name, ToDbValue((object)parameter.Value));
        }

        await using var reader = await command
            .ExecuteReaderAsync(System.Data.CommandBehavior.SequentialAccess, cancellationToken)
            .ConfigureAwait(false);

        return options.ReturnFormat == ReturnFormat.Csv
            ? await CsvQueryWriter.WriteAsync(reader, stream, options.CsvOptions, cancellationToken).ConfigureAwait(false)
            : await JsonQueryWriter.WriteAsync(reader, stream, options.JsonOptions, cancellationToken).ConfigureAwait(false);
    }

    private static object ToDbValue(object value)
    {
        if (value is JValue jValue) value = jValue.Value;
        return value ?? DBNull.Value;
    }
}
