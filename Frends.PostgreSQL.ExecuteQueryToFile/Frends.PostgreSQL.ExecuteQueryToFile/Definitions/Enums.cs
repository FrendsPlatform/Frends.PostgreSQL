namespace Frends.PostgreSQL.ExecuteQueryToFile.Definitions;

/// <summary>
/// File format of the output.
/// </summary>
public enum ReturnFormat
{
    /// <summary>
    /// Comma separated values.
    /// </summary>
    Csv,

    /// <summary>
    /// JSON, either as a single array or as JSON Lines.
    /// </summary>
    Json,
}

/// <summary>
/// Action to take when the output file already exists.
/// </summary>
public enum ActionOnExistingFile
{
    /// <summary>
    /// Replace the existing file.
    /// </summary>
    Overwrite,

    /// <summary>
    /// Fail without running the query.
    /// </summary>
    Throw,
}

/// <summary>
/// Field delimiter of the CSV output.
/// </summary>
public enum CsvFieldDelimiter
{
    /// <summary>
    /// Comma (,).
    /// </summary>
    Comma,

    /// <summary>
    /// Semicolon (;).
    /// </summary>
    Semicolon,

    /// <summary>
    /// Pipe (|).
    /// </summary>
    Pipe,

    /// <summary>
    /// Tab character.
    /// </summary>
    Tab,

    /// <summary>
    /// Delimiter given in CustomFieldDelimiter.
    /// </summary>
    Custom,
}

/// <summary>
/// Line break of the CSV output.
/// </summary>
public enum CsvLineBreak
{
    /// <summary>
    /// Windows style line break (\r\n).
    /// </summary>
    CRLF,

    /// <summary>
    /// Unix style line break (\n).
    /// </summary>
    LF,

    /// <summary>
    /// Classic Mac style line break (\r).
    /// </summary>
    CR,
}

/// <summary>
/// Encoding of the CSV file.
/// </summary>
public enum FileEncoding
{
    /// <summary>
    /// UTF-8.
    /// </summary>
    UTF8,

    /// <summary>
    /// ASCII.
    /// </summary>
    ASCII,

    /// <summary>
    /// UTF-16 little endian.
    /// </summary>
    Unicode,

    /// <summary>
    /// Encoding given by name in EncodingInString.
    /// </summary>
    Other,
}

/// <summary>
/// Structure of the JSON output.
/// </summary>
public enum JsonOutputMode
{
    /// <summary>
    /// One indented JSON array containing all rows.
    /// </summary>
    Indented,

    /// <summary>
    /// One compact JSON object per line (JSON Lines).
    /// </summary>
    JsonLines,
}
