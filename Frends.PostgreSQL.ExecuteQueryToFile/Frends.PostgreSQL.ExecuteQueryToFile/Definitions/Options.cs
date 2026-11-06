using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.PostgreSQL.ExecuteQueryToFile.Definitions;

/// <summary>
/// Additional parameters.
/// </summary>
public class Options
{
    /// <summary>
    /// Format of the output file.
    /// </summary>
    /// <example>ReturnFormat.Csv</example>
    [DefaultValue(ReturnFormat.Csv)]
    public ReturnFormat ReturnFormat { get; set; } = ReturnFormat.Csv;

    /// <summary>
    /// Command timeout in seconds. Use 0 for no timeout.
    /// </summary>
    /// <example>30</example>
    [DefaultValue(30)]
    public int CommandTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Size of the file write buffer in bytes. Larger values reduce I/O calls on big exports,
    /// smaller values reduce memory usage.
    /// </summary>
    /// <example>65536</example>
    [DefaultValue(65536)]
    public int FileBufferSize { get; set; } = 65536;

    /// <summary>
    /// CSV specific options.
    /// </summary>
    /// <example>new CsvOptions { FieldDelimiter = CsvFieldDelimiter.Semicolon }</example>
    [UIHint(nameof(ReturnFormat), "", ReturnFormat.Csv)]
    public CsvOptions CsvOptions { get; set; } = new();

    /// <summary>
    /// JSON specific options.
    /// </summary>
    /// <example>new JsonOptions { JsonOutputMode = JsonOutputMode.JsonLines }</example>
    [UIHint(nameof(ReturnFormat), "", ReturnFormat.Json)]
    public JsonOptions JsonOptions { get; set; } = new();

    /// <summary>
    /// Whether to throw an error on failure.
    /// </summary>
    /// <example>true</example>
    [DefaultValue(true)]
    public bool ThrowErrorOnFailure { get; set; } = true;

    /// <summary>
    /// Overrides the error message on failure.
    /// </summary>
    /// <example>Custom error message</example>
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("")]
    public string ErrorMessageOnFailure { get; set; } = string.Empty;
}
