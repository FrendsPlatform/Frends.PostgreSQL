using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.PostgreSQL.ExecuteQueryToFile.Definitions;

/// <summary>
/// Query parameter.
/// </summary>
public class Parameter
{
    /// <summary>
    /// Parameter name as used in the query, with or without the leading @.
    /// </summary>
    /// <example>Name</example>
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("")]
    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Parameter value. Null is sent to the database as NULL.
    /// </summary>
    /// <example>Erik</example>
    public dynamic Value { get; set; }
}
