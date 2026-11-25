namespace Frends.PostgreSQL.ExecuteQuery.Definitions;

/// <summary>
/// Class for query parameters.
/// </summary>
public class Parameter
{
    /// <summary>
    /// Field name.
    /// </summary>
    /// <example>customer_id</example>
    public string Name { get; set; }

    /// <summary>
    ///  Field value.
    /// </summary>
    /// <example>42</example>
    public dynamic Value { get; set; }
}
