namespace Frends.PostgreSQL.ExecuteQueryToFile.Definitions;

/// <summary>
/// Result of the task.
/// </summary>
public class Result
{
    /// <summary>
    /// Indicates if the task completed successfully.
    /// </summary>
    /// <example>true</example>
    public bool Success { get; set; }

    /// <summary>
    /// Number of rows written to the file.
    /// </summary>
    /// <example>2</example>
    public long RowsWritten { get; set; }

    /// <summary>
    /// Full path of the written file.
    /// </summary>
    /// <example>C:\output\users.csv</example>
    public string FilePath { get; set; }

    /// <summary>
    /// Error that occurred during task execution.
    /// </summary>
    /// <example>object { string Message, Exception AdditionalInfo }</example>
    public Error Error { get; set; }
}
