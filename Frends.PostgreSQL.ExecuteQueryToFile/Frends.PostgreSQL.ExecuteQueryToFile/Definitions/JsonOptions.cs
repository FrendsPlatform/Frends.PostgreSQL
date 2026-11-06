using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.PostgreSQL.ExecuteQueryToFile.Definitions;

/// <summary>
/// Options for the JSON output.
/// </summary>
public class JsonOptions
{
    /// <summary>
    /// Structure of the JSON file: one indented array, or one compact object per line.
    /// </summary>
    /// <example>JsonOutputMode.Indented</example>
    [DefaultValue(JsonOutputMode.Indented)]
    public JsonOutputMode JsonOutputMode { get; set; } = JsonOutputMode.Indented;

    /// <summary>
    /// Format of date columns, using .NET format tokens and invariant culture.
    /// </summary>
    /// <example>yyyy-MM-dd</example>
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("yyyy-MM-dd")]
    public string DateFormat { get; set; } = "yyyy-MM-dd";

    /// <summary>
    /// Format of timestamp columns, using .NET format tokens and invariant culture.
    /// Timestamps with time zone are returned by the database in UTC.
    /// </summary>
    /// <example>yyyy-MM-dd HH:mm:ss</example>
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("yyyy-MM-dd HH:mm:ss")]
    public string DateTimeFormat { get; set; } = "yyyy-MM-dd HH:mm:ss";
}
