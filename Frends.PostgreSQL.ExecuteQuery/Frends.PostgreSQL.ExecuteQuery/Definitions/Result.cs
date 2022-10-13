namespace Frends.PostgreSQL.ExecuteQuery.Definitions;

/// <summary>
/// Result-class for ExecuteQuery-task.
/// </summary>
public class Result
{
    /// <summary>
    /// Result of the query.
    /// </summary>
    /// <example>[{ "id": 123, "Name": "Matti" }, { "id": 124, "Name": "Teppo" }]</example>
    public dynamic QueryResult { get; private set; }

    /// <summary>
    /// Constructor.
    /// </summary>
    internal Result(dynamic result)
    {
        QueryResult = result;
    }

}
