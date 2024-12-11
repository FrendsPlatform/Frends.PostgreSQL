namespace Frends.PostgreSQL.ExecuteQuery.Definitions;
/// <summary>
/// Execute types.
/// </summary>
public enum ExecuteTypes
{
	/// <summary>
	/// ExecuteReader for SELECT-query and NonQuery for UPDATE, INSERT, or DELETE statements.
	/// </summary>
	Auto,

	/// <summary>
	/// Executes a Transact-SQL statement against the connection and returns the number of rows affected.
	/// </summary>
	NonQuery,

	/// <summary>
	/// Executes the query, and returns the first column of the first row in the result set returned by the query. Additional columns or rows are ignored.
	/// </summary>
	Scalar,

	/// <summary>
	/// Executes the query, and returns an object that can iterate over the entire result set.
	/// </summary>
	ExecuteReader
}
