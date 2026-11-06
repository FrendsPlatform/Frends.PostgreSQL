using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Frends.PostgreSQL.ExecuteQueryToFile.Attributes;

namespace Frends.PostgreSQL.ExecuteQueryToFile.Definitions;

/// <summary>
/// Options for the CSV output.
/// </summary>
public class CsvOptions
{
    /// <summary>
    /// What to use as the field separator.
    /// </summary>
    /// <example>CsvFieldDelimiter.Semicolon</example>
    [DefaultValue(CsvFieldDelimiter.Comma)]
    public CsvFieldDelimiter FieldDelimiter { get; set; } = CsvFieldDelimiter.Comma;

    /// <summary>
    /// Custom field delimiter.
    /// </summary>
    /// <example>#</example>
    [UIHint(nameof(FieldDelimiter), "", CsvFieldDelimiter.Custom)]
    [DisplayFormat(DataFormatString = "Text")]
    [RequiredIf(nameof(FieldDelimiter), CsvFieldDelimiter.Custom)]
    public string CustomFieldDelimiter { get; set; }

    /// <summary>
    /// What to use as the line break.
    /// </summary>
    /// <example>CsvLineBreak.CRLF</example>
    [DefaultValue(CsvLineBreak.CRLF)]
    public CsvLineBreak LineBreak { get; set; } = CsvLineBreak.CRLF;

    /// <summary>
    /// Encoding of the output file.
    /// </summary>
    /// <example>FileEncoding.UTF8</example>
    [DefaultValue(FileEncoding.UTF8)]
    public FileEncoding FileEncoding { get; set; } = FileEncoding.UTF8;

    /// <summary>
    /// Whether to write a byte order mark at the start of the file.
    /// </summary>
    /// <example>false</example>
    [UIHint(nameof(FileEncoding), "", FileEncoding.UTF8)]
    [DefaultValue(false)]
    public bool EnableBom { get; set; }

    /// <summary>
    /// Name of the encoding to use. A partial list of encodings: https://en.wikipedia.org/wiki/Windows_code_page#List
    /// </summary>
    /// <example>windows-1252</example>
    [UIHint(nameof(FileEncoding), "", FileEncoding.Other)]
    [DisplayFormat(DataFormatString = "Text")]
    [RequiredIf(nameof(FileEncoding), FileEncoding.Other)]
    public string EncodingInString { get; set; }

    /// <summary>
    /// Whether to write the column names as the first row.
    /// </summary>
    /// <example>true</example>
    [DefaultValue(true)]
    public bool IncludeHeadersInOutput { get; set; } = true;

    /// <summary>
    /// Whether to always wrap text values in quotes. Values containing the delimiter, quotes or line breaks
    /// are always quoted, and quotes inside values are escaped by doubling them.
    /// </summary>
    /// <example>true</example>
    [DefaultValue(true)]
    public bool AddQuotesToStrings { get; set; } = true;

    /// <summary>
    /// Whether to always wrap date and timestamp values in quotes.
    /// </summary>
    /// <example>false</example>
    [DefaultValue(false)]
    public bool AddQuotesToDates { get; set; }

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

    internal string GetFieldDelimiterAsString() => FieldDelimiter switch
    {
        CsvFieldDelimiter.Comma => ",",
        CsvFieldDelimiter.Semicolon => ";",
        CsvFieldDelimiter.Pipe => "|",
        CsvFieldDelimiter.Tab => "\t",
        CsvFieldDelimiter.Custom => CustomFieldDelimiter,
        _ => throw new ArgumentOutOfRangeException(nameof(FieldDelimiter), FieldDelimiter, "Unknown field delimiter."),
    };

    internal string GetLineBreakAsString() => LineBreak switch
    {
        CsvLineBreak.CRLF => "\r\n",
        CsvLineBreak.LF => "\n",
        CsvLineBreak.CR => "\r",
        _ => throw new ArgumentOutOfRangeException(nameof(LineBreak), LineBreak, "Unknown line break."),
    };
}
