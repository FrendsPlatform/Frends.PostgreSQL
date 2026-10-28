using System;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Frends.PostgreSQL.ExecuteQueryToFile.Definitions;

namespace Frends.PostgreSQL.ExecuteQueryToFile.Helpers;

internal static class JsonQueryWriter
{
    private const int FlushThresholdBytes = 16 * 1024;

    internal static async Task<long> WriteAsync(
        DbDataReader reader,
        Stream stream,
        JsonOptions options,
        CancellationToken cancellationToken)
    {
        var jsonLines = options.JsonOutputMode == JsonOutputMode.JsonLines;

        var writerOptions = new JsonWriterOptions
        {
            Indented = !jsonLines,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };

        await using var writer = new Utf8JsonWriter(stream, writerOptions);
        var columns = ColumnSchema.FromReader(reader);
        long rows = 0;

        if (!jsonLines) writer.WriteStartArray();

        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            cancellationToken.ThrowIfCancellationRequested();

            writer.WriteStartObject();
            foreach (var column in columns)
            {
                writer.WritePropertyName(column.Name);
                WriteValue(writer, reader.GetValue(column.Index), column, options);
            }

            writer.WriteEndObject();
            rows++;

            if (jsonLines)
            {
                await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
                stream.WriteByte((byte)'\n');
                writer.Reset();
            }
            else if (writer.BytesPending > FlushThresholdBytes)
            {
                await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        if (!jsonLines) writer.WriteEndArray();

        await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
        return rows;
    }

    private static void WriteValue(Utf8JsonWriter writer, object value, ColumnSchema column, JsonOptions options)
    {
        switch (value)
        {
            case null:
            case DBNull:
                writer.WriteNullValue();
                break;
            case string text when column.IsJson:
                writer.WriteRawValue(text, true);
                break;
            case string text:
                writer.WriteStringValue(text);
                break;
            case bool boolean:
                writer.WriteBooleanValue(boolean);
                break;
            case short number:
                writer.WriteNumberValue(number);
                break;
            case int number:
                writer.WriteNumberValue(number);
                break;
            case long number:
                writer.WriteNumberValue(number);
                break;
            case decimal number:
                writer.WriteNumberValue(number);
                break;
            case float number when float.IsFinite(number):
                writer.WriteNumberValue(number);
                break;
            case double number when double.IsFinite(number):
                writer.WriteNumberValue(number);
                break;
            case float or double:
                writer.WriteStringValue(((IFormattable)value).ToString(null, CultureInfo.InvariantCulture));
                break;
            case DateTime dateTime:
                var format = column.IsDate ? options.DateFormat : options.DateTimeFormat;
                writer.WriteStringValue(dateTime.ToString(format, CultureInfo.InvariantCulture));
                break;
            case DateTimeOffset dateTimeOffset:
                writer.WriteStringValue(ColumnSchema.FormatDateTimeOffset(dateTimeOffset, column.IsTimeWithTimeZone));
                break;
            case TimeSpan timeSpan:
                writer.WriteStringValue(timeSpan.ToString("c", CultureInfo.InvariantCulture));
                break;
            case Guid guid:
                writer.WriteStringValue(guid);
                break;
            case byte[] bytes:
                writer.WriteBase64StringValue(bytes);
                break;
            case Array array:
                writer.WriteStartArray();
                foreach (var item in array) WriteValue(writer, item, column, options);
                writer.WriteEndArray();
                break;
            case IFormattable formattable:
                writer.WriteStringValue(formattable.ToString(null, CultureInfo.InvariantCulture));
                break;
            default:
                writer.WriteStringValue(value.ToString());
                break;
        }
    }
}
