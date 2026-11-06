using System;
using System.Data.Common;
using System.Globalization;

namespace Frends.PostgreSQL.ExecuteQueryToFile.Helpers;

internal sealed class ColumnSchema
{
    private ColumnSchema(int index, string name, Type fieldType, string dataTypeName)
    {
        Index = index;
        Name = name;
        FieldType = fieldType;
        IsDate = fieldType == typeof(DateTime) && dataTypeName == "date";
        IsTimeWithTimeZone = dataTypeName == "time with time zone";
        IsJson = dataTypeName is "json" or "jsonb";
    }

    internal int Index { get; }

    internal string Name { get; }

    internal Type FieldType { get; }

    internal bool IsDate { get; }

    internal bool IsTimeWithTimeZone { get; }

    internal bool IsJson { get; }

    internal static string FormatDateTimeOffset(DateTimeOffset value, bool timeOnly) =>
        value.ToString(timeOnly ? "HH:mm:ss.FFFFFFFzzz" : "O", CultureInfo.InvariantCulture);

    internal static ColumnSchema[] FromReader(DbDataReader reader)
    {
        var columns = new ColumnSchema[reader.FieldCount];
        for (var i = 0; i < columns.Length; i++)
            columns[i] = new ColumnSchema(i, reader.GetName(i), reader.GetFieldType(i), reader.GetDataTypeName(i));

        return columns;
    }
}
