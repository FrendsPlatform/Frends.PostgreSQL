namespace Frends.PostgreSQL.ExecuteQuery.Definitions;

/// <summary>
/// Result-class for ExecuteQuery-task.
/// </summary>
public class Result
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Result"/> class.
    /// </summary>
    /// <param name="result">The query result, or null when the query fails.</param>
    internal Result(dynamic result = null)
    {
        QueryResult = result;
        Success = true;
    }

    /// <summary>
    /// Indicates whether the query completed successfully.
    /// </summary>
    /// <example>true</example>
    public bool Success { get; set; }

    /// <summary>
    /// Result of the query.
    /// </summary>
    /// <example>[{ "id": 123, "Name": "Matti" }, { "id": 124, "Name": "Teppo" }]</example>
    public dynamic QueryResult { get; private set; }

    /// <summary>
    /// Error that occurred during query execution.
    /// </summary>
    /// <example>object { string Message, Exception AdditionalInfo }</example>
    public Error Error { get; set; }
}
