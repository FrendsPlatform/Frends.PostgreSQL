using System;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Frends.PostgreSQL.ExecuteQueryToFile.Definitions;

namespace Frends.PostgreSQL.ExecuteQueryToFile.Helpers;

internal static class CsvQueryWriter
{
    private static readonly char[] AlwaysQuoted = ['"', '\r', '\n'];

    internal static async Task<long> WriteAsync(
        DbDataReader reader,
        Stream stream,
        CsvOptions options,
        CancellationToken cancellationToken)
    {
        var delimiter = options.GetFieldDelimiterAsString();
        if (delimiter.IndexOfAny(AlwaysQuoted) >= 0)
            throw new ArgumentException("Field delimiter cannot contain quotes or line breaks.");

        await using var writer = new StreamWriter(stream, GetEncoding(options), 32 * 1024);
        writer.NewLine = options.GetLineBreakAsString();

        var columns = ColumnSchema.FromReader(reader);

        if (options.IncludeHeadersInOutput && columns.Length > 0)
        {
            for (var i = 0; i < columns.Length; i++)
            {
                if (i > 0) writer.Write(delimiter);
                WriteField(writer, columns[i].Name, false, delimiter);
            }

            writer.WriteLine();
        }

        long rows = 0;

        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            cancellationToken.ThrowIfCancellationRequested();

            for (var i = 0; i < columns.Length; i++)
            {
                if (i > 0) writer.Write(delimiter);
                WriteValue(writer, reader, columns[i], options, delimiter);
            }

            writer.WriteLine();
            rows++;
        }

        await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
        return rows;
    }

    private static void WriteValue(
        TextWriter writer,
        DbDataReader reader,
        ColumnSchema column,
        CsvOptions options,
        string delimiter)
    {
        var value = reader.GetValue(column.Index);

        switch (value)
        {
            case DBNull:
                return;
            case string text:
                WriteField(writer, text, options.AddQuotesToStrings, delimiter);
                break;
            case DateTime dateTime:
                var format = column.IsDate ? options.DateFormat : options.DateTimeFormat;
                WriteField(writer, dateTime.ToString(format, CultureInfo.InvariantCulture), options.AddQuotesToDates, delimiter);
                break;
            case DateTimeOffset dateTimeOffset:
                WriteField(writer, ColumnSchema.FormatDateTimeOffset(dateTimeOffset, column.IsTimeWithTimeZone), options.AddQuotesToDates, delimiter);
                break;
            default:
                WriteField(writer, ToText(value, options), false, delimiter);
                break;
        }
    }

    private static void WriteField(TextWriter writer, string text, bool forceQuotes, string delimiter)
    {
        var needsQuotes = forceQuotes
                          || text.IndexOfAny(AlwaysQuoted) >= 0
                          || text.Contains(delimiter, StringComparison.Ordinal);

        if (!needsQuotes)
        {
            writer.Write(text);
            return;
        }

        writer.Write('"');
        writer.Write(text.Replace("\"", "\"\"", StringComparison.Ordinal));
        writer.Write('"');
    }

    private static string ToText(object value, CsvOptions options)
    {
        return value switch
        {
            null => string.Empty,
            string text => text,
            bool boolean => boolean ? "true" : "false",
            byte[] bytes => Convert.ToBase64String(bytes),
            DateTime dateTime => dateTime.ToString(options.DateTimeFormat, CultureInfo.InvariantCulture),
            DateTimeOffset dateTimeOffset => dateTimeOffset.ToString("O", CultureInfo.InvariantCulture),
            Array array => "{" + string.Join(",", array.Cast<object>().Select(item => ToText(item, options))) + "}",
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString(),
        };
    }

    private static Encoding GetEncoding(CsvOptions options)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        return options.FileEncoding switch
        {
            FileEncoding.UTF8 => new UTF8Encoding(options.EnableBom),
            FileEncoding.ASCII => Encoding.ASCII,
            FileEncoding.Unicode => Encoding.Unicode,
            FileEncoding.Other => Encoding.GetEncoding(options.EncodingInString),
            _ => throw new ArgumentOutOfRangeException(nameof(options), options.FileEncoding, "Unknown file encoding."),
        };
    }
}
