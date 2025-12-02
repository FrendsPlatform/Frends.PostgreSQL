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
    /// Auto: Automatically detects if the query returns data (SELECT or RETURNING clause) and uses ExecuteReader, otherwise uses NonQuery.
    /// ExecuteReader: Use this to execute queries that return a result set (SELECT or INSERT/UPDATE/DELETE with RETURNING clause).
    /// NonQuery: Use this to execute commands that don't return a result set (INSERT, UPDATE, DELETE without RETURNING). Returns the number of affected rows.
    /// </summary>
    /// <example>ExecuteTypes.Auto</example>
    [DefaultValue(ExecuteTypes.Auto)]
    public ExecuteTypes ExecuteType { get; set; }

}
