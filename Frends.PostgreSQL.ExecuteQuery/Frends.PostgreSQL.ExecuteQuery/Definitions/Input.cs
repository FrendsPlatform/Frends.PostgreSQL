using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.PostgreSQL.ExecuteQuery.Definitions;

/// <summary>
/// Input-class for ExecuteQuery-task.
/// </summary>
public class Input
{

    /// <summary>
    /// Query.
    /// Note: Normal query requires double quotes around Column and 2 single quotes around Value.
    /// </summary>
    /// <example>SELECT * FROM Users</example>
    [DefaultValue("SELECT * FROM Users")]
    [DisplayFormat(DataFormatString = "Text")]
    public string Query { get; set; }

    /// <summary>
    /// Query parameters.
    /// </summary>
    /// <example>[{ Name = "Name", Value = "Erik" }, { Name = "Email", Value = "erik.example@foobar.com" }]</example>
    [DefaultValue(null)]
    public Parameter[] Parameters { get; set; }

    /// <summary>
    /// Connection string.
    /// </summary>
    /// <example>Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;</example>
    [PasswordPropertyText]
    public string ConnectionString { get; set; }

	/// <summary>
	/// Specifies how a command string is interpreted.
	/// Auto: ExecuteReader for SELECT-query and NonQuery for UPDATE, INSERT, or DELETE statements.
	/// ExecuteReader: Use this operation to execute any arbitrary SQL statements in SQL Server if you want the result set to be returned.
	/// NonQuery: Use this operation to execute any arbitrary SQL statements in SQL Server if you do not want any result set to be returned. You can use this operation to create database objects or change data in a database by executing UPDATE, INSERT, or DELETE statements. The return value of this operation is of Int32 data type, and For the UPDATE, INSERT, and DELETE statements, the return value is the number of rows affected by the SQL statement. For all other types of statements, the return value is -1.
	/// Scalar: Use this operation to execute any arbitrary SQL statements in SQL Server to return a single value. This operation returns the value only in the first column of the first row in the result set returned by the SQL statement.
	/// </summary>
	/// <example>ExecuteType.ExecuteReader</example>
	[DefaultValue(ExecuteTypes.ExecuteReader)]
	public ExecuteTypes ExecuteType { get; set; }
}
