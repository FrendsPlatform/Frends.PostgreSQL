namespace Frends.PostgreSQL.ExecuteQuery.Definitions;

/// <summary>
/// Specifies how a command string is interpreted.
/// </summary>
public enum ExecuteTypes
{
    /// <summary>
    /// Auto-detect based on query structure.
    /// Uses ExecuteReader for queries that return data (SELECT or queries with RETURNING clause).
    /// Uses NonQuery for INSERT, UPDATE, DELETE statements without RETURNING clause.
    /// </summary>
    Auto,

    /// <summary>
    /// Execute the query without expecting any result set.
    /// Use for INSERT, UPDATE, DELETE statements without RETURNING clause.
    /// Returns the number of rows affected.
    /// </summary>
    NonQuery,

    /// <summary>
    /// Execute the query and return the result set.
    /// Use for SELECT queries or INSERT/UPDATE/DELETE with RETURNING clause.
    /// Returns the data rows.
    /// </summary>
    ExecuteReader
}
