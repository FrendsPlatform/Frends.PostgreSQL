using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.PostgreSQL.ExecuteQueryToFile.Definitions;

/// <summary>
/// Essential parameters.
/// </summary>
public class Input
{
    /// <summary>
    /// Query to execute. Use parameter placeholders (e.g. @Name) together with the Parameters
    /// property instead of concatenating values into the query.
    /// </summary>
    /// <example>SELECT * FROM users WHERE name = @Name</example>
    [DisplayFormat(DataFormatString = "Sql")]
    [DefaultValue("SELECT * FROM users")]
    [Required]
    public string Query { get; set; } = string.Empty;

    /// <summary>
    /// Query parameters.
    /// </summary>
    /// <example>[ { Name = "Name", Value = "Erik" } ]</example>
    public Parameter[] Parameters { get; set; }

    /// <summary>
    /// Full path of the file the query result is written to.
    /// </summary>
    /// <example>C:\output\users.csv</example>
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("")]
    [Required]
    public string OutputFilePath { get; set; } = string.Empty;

    /// <summary>
    /// What to do if the output file already exists.
    /// </summary>
    /// <example>ActionOnExistingFile.Overwrite</example>
    [DefaultValue(ActionOnExistingFile.Overwrite)]
    public ActionOnExistingFile ActionOnExistingFile { get; set; } = ActionOnExistingFile.Overwrite;
}
