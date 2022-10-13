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

}
