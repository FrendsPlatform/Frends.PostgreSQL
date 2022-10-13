namespace Frends.PostgreSQL.ExecuteQuery.Definitions;

/// <summary>
/// Class for query parameters.
/// </summary>
public class Parameter
{

    /// <summary>
    /// Field name.
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    ///  Field value.
    /// </summary>
    public dynamic Value { get; set; }

}
