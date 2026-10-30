using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Frends.PostgreSQL.ExecuteQueryToFile.Definitions;
using Newtonsoft.Json.Linq;
using Npgsql;
using NUnit.Framework;

namespace Frends.PostgreSQL.ExecuteQueryToFile.Tests;

[TestFixture]
internal class FunctionalTests : TestBase
{
    private const string BasicColumns = "id, name, amount, flag, created, updated";

    [Test]
    public async Task Csv_ShouldWriteRowsWithDefaultOptions()
    {
        var input = DefaultInput($"SELECT {BasicColumns} FROM {TableName} ORDER BY id");

        var result = await PostgreSQL.ExecuteQueryToFile(input, DefaultConnection(), DefaultOptions(), CancellationToken.None);

        const string expected =
            "id,name,amount,flag,created,updated\r\n" +
            "1,\"Erik\",12.50,true,2024-03-05,2024-03-05 10:11:12\r\n" +
            "2,\"Comma, \"\"Quote\"\" and\nnewline\",0.10,false,2023-12-31,2023-12-31 23:59:59\r\n" +
            "3,,,,,\r\n" +
            "4,\"\",1000.00,true,2020-01-01,2020-01-01 00:00:00\r\n";

        Assert.That(result.Success, Is.True);
        Assert.That(result.Error, Is.Null);
        Assert.That(result.RowsWritten, Is.EqualTo(4));
        Assert.That(result.FilePath, Is.EqualTo(OutputPath));
        Assert.That(await File.ReadAllTextAsync(OutputPath), Is.EqualTo(expected));
        Assert.That(FilesInOutputDirectory(), Is.EqualTo(new[] { "output.out" }));
    }

    [Test]
    public async Task Csv_ShouldApplyCustomFormatting()
    {
        var options = DefaultOptions();
        options.CsvOptions = new CsvOptions
        {
            FieldDelimiter = CsvFieldDelimiter.Custom,
            CustomFieldDelimiter = "##",
            LineBreak = CsvLineBreak.LF,
            IncludeHeadersInOutput = false,
            AddQuotesToStrings = false,
            AddQuotesToDates = true,
            DateFormat = "dd.MM.yyyy",
            DateTimeFormat = "HH:mm",
        };

        var input = DefaultInput($"SELECT id, name, created, updated FROM {TableName} WHERE id IN (1, 3) ORDER BY id");

        var result = await PostgreSQL.ExecuteQueryToFile(input, DefaultConnection(), options, CancellationToken.None);

        Assert.That(result.RowsWritten, Is.EqualTo(2));
        Assert.That(await File.ReadAllTextAsync(OutputPath), Is.EqualTo("1##Erik##\"05.03.2024\"##\"10:11\"\n3######\n"));
    }

    [Test]
    public async Task Csv_ShouldQuoteValuesContainingCustomDelimiter()
    {
        var options = DefaultOptions();
        options.CsvOptions = new CsvOptions { FieldDelimiter = CsvFieldDelimiter.Pipe, AddQuotesToStrings = false };
        var input = DefaultInput("SELECT 'a|b' AS x, 'c,d' AS y");

        await PostgreSQL.ExecuteQueryToFile(input, DefaultConnection(), options, CancellationToken.None);

        Assert.That(await File.ReadAllTextAsync(OutputPath), Is.EqualTo("x|y\r\n\"a|b\"|c,d\r\n"));
    }

    [Test]
    public async Task Csv_ShouldWriteArraysBinaryAndJson()
    {
        var input = DefaultInput($"SELECT tags, data, doc FROM {TableName} WHERE id = 1");
        var options = DefaultOptions();
        options.CsvOptions = new CsvOptions { FieldDelimiter = CsvFieldDelimiter.Semicolon };

        await PostgreSQL.ExecuteQueryToFile(input, DefaultConnection(), options, CancellationToken.None);

        var lines = await File.ReadAllLinesAsync(OutputPath);
        Assert.That(lines[0], Is.EqualTo("tags;data;doc"));
        Assert.That(lines[1], Is.EqualTo("{1,2,3};AQI=;\"{\"\"a\"\": [1, 2]}\""));
    }

    [Test]
    public async Task Csv_ShouldWriteBomWhenEnabled()
    {
        var options = DefaultOptions();
        options.CsvOptions = new CsvOptions { EnableBom = true };

        await PostgreSQL.ExecuteQueryToFile(DefaultInput("SELECT 'ä' AS x"), DefaultConnection(), options, CancellationToken.None);

        var bytes = await File.ReadAllBytesAsync(OutputPath);
        Assert.That(bytes.Take(3), Is.EqualTo(new byte[] { 0xEF, 0xBB, 0xBF }));
    }

    [Test]
    public async Task Csv_ShouldUseOtherEncoding()
    {
        var options = DefaultOptions();
        options.CsvOptions = new CsvOptions
        {
            FileEncoding = FileEncoding.Other,
            EncodingInString = "iso-8859-1",
            IncludeHeadersInOutput = false,
            AddQuotesToStrings = false,
        };

        await PostgreSQL.ExecuteQueryToFile(DefaultInput("SELECT 'ä' AS x"), DefaultConnection(), options, CancellationToken.None);

        Assert.That(await File.ReadAllBytesAsync(OutputPath), Is.EqualTo(new byte[] { 0xE4, 0x0D, 0x0A }));
    }

    [Test]
    public async Task Csv_ShouldWriteUnicodeAndAscii()
    {
        var options = DefaultOptions();
        options.CsvOptions = new CsvOptions { FileEncoding = FileEncoding.Unicode, IncludeHeadersInOutput = false };
        await PostgreSQL.ExecuteQueryToFile(DefaultInput("SELECT 'a' AS x"), DefaultConnection(), options, CancellationToken.None);
        Assert.That(await File.ReadAllTextAsync(OutputPath, Encoding.Unicode), Is.EqualTo("\"a\"\r\n"));

        options.CsvOptions.FileEncoding = FileEncoding.ASCII;
        await PostgreSQL.ExecuteQueryToFile(DefaultInput("SELECT 'a' AS x"), DefaultConnection(), options, CancellationToken.None);
        Assert.That(await File.ReadAllTextAsync(OutputPath, Encoding.ASCII), Is.EqualTo("\"a\"\r\n"));
    }

    [Test]
    public async Task Csv_ShouldWriteTimestampWithTimeZoneAndTime()
    {
        var input = DefaultInput("SELECT timestamptz '2024-01-02 03:04:05+00' AS a, time '10:11:12' AS b, interval '1 day' AS c");

        await PostgreSQL.ExecuteQueryToFile(input, DefaultConnection(), DefaultOptions(), CancellationToken.None);

        Assert.That((await File.ReadAllLinesAsync(OutputPath))[1], Is.EqualTo("2024-01-02 03:04:05,10:11:12,1.00:00:00"));
    }

    [Test]
    public async Task Json_ShouldWriteIndentedArray()
    {
        var options = DefaultOptions();
        options.ReturnFormat = ReturnFormat.Json;

        var result = await PostgreSQL.ExecuteQueryToFile(DefaultInput(), DefaultConnection(), options, CancellationToken.None);

        Assert.That(result.RowsWritten, Is.EqualTo(4));

        using var document = JsonDocument.Parse(await File.ReadAllTextAsync(OutputPath));
        var rows = document.RootElement.EnumerateArray().ToArray();
        Assert.That(rows, Has.Length.EqualTo(4));

        var first = rows[0];
        Assert.That(first.GetProperty("id").GetInt32(), Is.EqualTo(1));
        Assert.That(first.GetProperty("name").GetString(), Is.EqualTo("Erik"));
        Assert.That(first.GetProperty("amount").GetDecimal(), Is.EqualTo(12.5m));
        Assert.That(first.GetProperty("flag").GetBoolean(), Is.True);
        Assert.That(first.GetProperty("created").GetString(), Is.EqualTo("2024-03-05"));
        Assert.That(first.GetProperty("updated").GetString(), Is.EqualTo("2024-03-05 10:11:12"));
        Assert.That(first.GetProperty("doc").GetProperty("a")[1].GetInt32(), Is.EqualTo(2));
        Assert.That(first.GetProperty("tags").EnumerateArray().Select(t => t.GetInt32()), Is.EqualTo(new[] { 1, 2, 3 }));
        Assert.That(first.GetProperty("data").GetString(), Is.EqualTo("AQI="));

        Assert.That(rows[1].GetProperty("name").GetString(), Is.EqualTo("Comma, \"Quote\" and\nnewline"));
        Assert.That(rows[1].GetProperty("doc").ValueKind, Is.EqualTo(JsonValueKind.Null));

        foreach (var property in rows[2].EnumerateObject().Where(p => p.Name != "id"))
            Assert.That(property.Value.ValueKind, Is.EqualTo(JsonValueKind.Null), property.Name);

        Assert.That(rows[3].GetProperty("doc").ValueKind, Is.EqualTo(JsonValueKind.Array));
        Assert.That(rows[3].GetProperty("tags").GetArrayLength(), Is.EqualTo(0));
        Assert.That(rows[3].GetProperty("data").GetString(), Is.Empty);
    }

    [Test]
    public async Task Json_ShouldWriteJsonLines()
    {
        var options = DefaultOptions();
        options.ReturnFormat = ReturnFormat.Json;
        options.JsonOptions = new JsonOptions { JsonOutputMode = JsonOutputMode.JsonLines };

        var result = await PostgreSQL.ExecuteQueryToFile(DefaultInput(), DefaultConnection(), options, CancellationToken.None);

        var lines = await File.ReadAllLinesAsync(OutputPath);
        Assert.That(result.RowsWritten, Is.EqualTo(4));
        Assert.That(lines, Has.Length.EqualTo(4));
        Assert.That(lines.Select(line => JsonDocument.Parse(line).RootElement.GetProperty("id").GetInt32()), Is.EqualTo(new[] { 1, 2, 3, 4 }));
    }

    [Test]
    public async Task Json_ShouldWriteSpecialTypes()
    {
        var options = DefaultOptions();
        options.ReturnFormat = ReturnFormat.Json;
        options.JsonOptions = new JsonOptions { JsonOutputMode = JsonOutputMode.JsonLines, DateTimeFormat = "yyyy-MM-dd'T'HH:mm:ss" };

        var input = DefaultInput(
            "SELECT 'NaN'::float8 AS nan, 1.5::float4 AS f, 2.5::float8 AS d, 7::int8 AS l, 3::int2 AS s, " +
            "timestamptz '2024-01-02 03:04:05+00' AS tz, timetz '10:11:12+02' AS ttz, time '10:11:12' AS t, " +
            "'11111111-1111-1111-1111-111111111111'::uuid AS u, inet '10.0.0.1' AS ip, 'ä€' AS txt");

        await PostgreSQL.ExecuteQueryToFile(input, DefaultConnection(), options, CancellationToken.None);

        var content = await File.ReadAllTextAsync(OutputPath);
        Assert.That(content, Does.Contain("ä€"));

        using var document = JsonDocument.Parse(content);
        var row = document.RootElement;
        Assert.That(row.GetProperty("nan").GetString(), Is.EqualTo("NaN"));
        Assert.That(row.GetProperty("f").GetSingle(), Is.EqualTo(1.5f));
        Assert.That(row.GetProperty("d").GetDouble(), Is.EqualTo(2.5));
        Assert.That(row.GetProperty("l").GetInt64(), Is.EqualTo(7));
        Assert.That(row.GetProperty("s").GetInt16(), Is.EqualTo(3));
        Assert.That(row.GetProperty("tz").GetString(), Is.EqualTo("2024-01-02T03:04:05"));
        Assert.That(row.GetProperty("ttz").GetString(), Is.EqualTo("10:11:12+02:00"));
        Assert.That(row.GetProperty("t").GetString(), Is.EqualTo("10:11:12"));
        Assert.That(row.GetProperty("u").GetString(), Is.EqualTo("11111111-1111-1111-1111-111111111111"));
        Assert.That(row.GetProperty("ip").GetString(), Is.EqualTo("10.0.0.1"));
    }

    [Test]
    public async Task ShouldWriteHeaderOnlyOrEmptyArrayWhenNoRows()
    {
        var input = DefaultInput($"SELECT id, name FROM {TableName} WHERE id < 0");

        var csvResult = await PostgreSQL.ExecuteQueryToFile(input, DefaultConnection(), DefaultOptions(), CancellationToken.None);
        Assert.That(csvResult.RowsWritten, Is.Zero);
        Assert.That(await File.ReadAllTextAsync(OutputPath), Is.EqualTo("id,name\r\n"));

        var options = DefaultOptions();
        options.ReturnFormat = ReturnFormat.Json;
        var jsonResult = await PostgreSQL.ExecuteQueryToFile(input, DefaultConnection(), options, CancellationToken.None);
        Assert.That(jsonResult.RowsWritten, Is.Zero);
        Assert.That(JsonDocument.Parse(await File.ReadAllTextAsync(OutputPath)).RootElement.GetArrayLength(), Is.Zero);
    }

    [Test]
    public async Task ShouldUseQueryParameters()
    {
        var input = DefaultInput($"SELECT id FROM {TableName} WHERE id = @Id OR name = @Name OR CAST(@Nothing AS text) IS NULL AND id = 4");
        input.Parameters =
        [
            new Parameter { Name = "Id", Value = 1 },
            new Parameter { Name = "@Name", Value = new JValue("Erik") },
            new Parameter { Name = "Nothing", Value = null },
        ];

        var result = await PostgreSQL.ExecuteQueryToFile(input, DefaultConnection(), DefaultOptions(), CancellationToken.None);

        Assert.That(result.RowsWritten, Is.EqualTo(2));
        Assert.That(await File.ReadAllLinesAsync(OutputPath), Is.EqualTo(new[] { "id", "1", "4" }));
    }

    [Test]
    public async Task ShouldNotInterpretParameterValueAsSql()
    {
        var input = DefaultInput($"SELECT id FROM {TableName} WHERE name = @Name");
        input.Parameters = [new Parameter { Name = "Name", Value = "x'; DROP TABLE " + TableName + "; --" }];

        var result = await PostgreSQL.ExecuteQueryToFile(input, DefaultConnection(), DefaultOptions(), CancellationToken.None);

        Assert.That(result.RowsWritten, Is.Zero);
        Assert.That(
            (await PostgreSQL.ExecuteQueryToFile(DefaultInput(), DefaultConnection(), DefaultOptions(), CancellationToken.None)).RowsWritten,
            Is.EqualTo(4));
    }

    [Test]
    public async Task ShouldOverwriteExistingFile()
    {
        await File.WriteAllTextAsync(OutputPath, "old");

        var result = await PostgreSQL.ExecuteQueryToFile(DefaultInput("SELECT 1 AS x"), DefaultConnection(), DefaultOptions(), CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(await File.ReadAllTextAsync(OutputPath), Is.EqualTo("x\r\n1\r\n"));
        Assert.That(FilesInOutputDirectory(), Is.EqualTo(new[] { "output.out" }));
    }

    [Test]
    public async Task ShouldFailWithoutTouchingExistingFileWhenActionIsThrow()
    {
        await File.WriteAllTextAsync(OutputPath, "old");
        var input = DefaultInput("SELECT 1 AS x");
        input.ActionOnExistingFile = ActionOnExistingFile.Throw;

        var ex = await Assert.ThrowsAsync<IOException>(() =>
            PostgreSQL.ExecuteQueryToFile(input, DefaultConnection(), DefaultOptions(), CancellationToken.None));

        Assert.That(ex.Message, Does.Contain(OutputPath));
        Assert.That(await File.ReadAllTextAsync(OutputPath), Is.EqualTo("old"));
        Assert.That(FilesInOutputDirectory(), Is.EqualTo(new[] { "output.out" }));
    }

    [Test]
    public async Task ShouldKeepExistingFileAndLeaveNoTempFilesWhenQueryFails()
    {
        await File.WriteAllTextAsync(OutputPath, "old");

        await Assert.ThrowsAsync<PostgresException>(() =>
            PostgreSQL.ExecuteQueryToFile(DefaultInput("SELECT * FROM missing_table"), DefaultConnection(), DefaultOptions(), CancellationToken.None));

        Assert.That(await File.ReadAllTextAsync(OutputPath), Is.EqualTo("old"));
        Assert.That(FilesInOutputDirectory(), Is.EqualTo(new[] { "output.out" }));
    }

    [Test]
    public async Task ShouldFailWhenQueryFailsMidStream()
    {
        var input = DefaultInput("SELECT 1 / (3 - g) AS x FROM generate_series(1, 5) g");

        await Assert.ThrowsAsync<PostgresException>(() =>
            PostgreSQL.ExecuteQueryToFile(input, DefaultConnection(), DefaultOptions(), CancellationToken.None));

        Assert.That(FilesInOutputDirectory(), Is.Empty);
    }

    [Test]
    public async Task ShouldStreamLargeResult()
    {
        const int rowCount = 200_000;
        var input = DefaultInput($"SELECT g AS id, md5(g::text) AS hash FROM generate_series(1, {rowCount}) g");

        var csvResult = await PostgreSQL.ExecuteQueryToFile(input, DefaultConnection(), DefaultOptions(), CancellationToken.None);
        Assert.That(csvResult.RowsWritten, Is.EqualTo(rowCount));
        Assert.That(File.ReadLines(OutputPath).Count(), Is.EqualTo(rowCount + 1));

        var options = DefaultOptions();
        options.ReturnFormat = ReturnFormat.Json;
        options.JsonOptions = new JsonOptions { JsonOutputMode = JsonOutputMode.JsonLines };
        var jsonResult = await PostgreSQL.ExecuteQueryToFile(input, DefaultConnection(), options, CancellationToken.None);
        Assert.That(jsonResult.RowsWritten, Is.EqualTo(rowCount));
        Assert.That(File.ReadLines(OutputPath).Count(), Is.EqualTo(rowCount));
    }

    [Test]
    public async Task ShouldWriteEmptyFileWhenStatementReturnsNoColumns()
    {
        var result = await PostgreSQL.ExecuteQueryToFile(
            DefaultInput("CREATE TEMP TABLE etf_temp (id int)"),
            DefaultConnection(),
            DefaultOptions(),
            CancellationToken.None);

        Assert.That(result.RowsWritten, Is.Zero);
        Assert.That(new FileInfo(OutputPath).Length, Is.Zero);
    }

    [Test]
    public async Task ShouldTimeOutLongRunningQuery()
    {
        var options = DefaultOptions();
        options.CommandTimeoutSeconds = 1;

        var result = await Assert.CatchAsync<Exception>(() =>
            PostgreSQL.ExecuteQueryToFile(DefaultInput("SELECT pg_sleep(10)"), DefaultConnection(), options, CancellationToken.None));

        Assert.That(result, Is.Not.Null);
        Assert.That(FilesInOutputDirectory(), Is.Empty);
    }

    [Test]
    public async Task ShouldCreateNoFileWhenCancelledBeforeStart()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.CatchAsync<OperationCanceledException>(() =>
            PostgreSQL.ExecuteQueryToFile(DefaultInput(), DefaultConnection(), DefaultOptions(), cts.Token));

        Assert.That(FilesInOutputDirectory(), Is.Empty);
    }

    [Test]
    public async Task ShouldCancelRunningQueryAndCleanUp()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(500));

        await Assert.CatchAsync<OperationCanceledException>(() =>
            PostgreSQL.ExecuteQueryToFile(DefaultInput("SELECT pg_sleep(30)"), DefaultConnection(), DefaultOptions(), cts.Token));

        Assert.That(FilesInOutputDirectory(), Is.Empty);
    }

    [Test]
    public async Task ShouldFailValidationBeforeAnySideEffects()
    {
        var input = DefaultInput();
        input.Query = string.Empty;

        await Assert.ThrowsAsync<ValidationException>(() =>
            PostgreSQL.ExecuteQueryToFile(input, DefaultConnection(), DefaultOptions(), CancellationToken.None));

        Assert.That(FilesInOutputDirectory(), Is.Empty);
    }

    [Test]
    public async Task ShouldRequireCustomDelimiterWhenSelected()
    {
        var options = DefaultOptions();
        options.CsvOptions = new CsvOptions { FieldDelimiter = CsvFieldDelimiter.Custom };

        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            PostgreSQL.ExecuteQueryToFile(DefaultInput(), DefaultConnection(), options, CancellationToken.None));

        Assert.That(ex.Message, Does.Contain("CustomFieldDelimiter"));
    }

    [Test]
    public async Task ShouldRejectDelimiterContainingQuote()
    {
        var options = DefaultOptions();
        options.CsvOptions = new CsvOptions { FieldDelimiter = CsvFieldDelimiter.Custom, CustomFieldDelimiter = "\"" };

        await Assert.ThrowsAsync<ArgumentException>(() =>
            PostgreSQL.ExecuteQueryToFile(DefaultInput(), DefaultConnection(), options, CancellationToken.None));
    }

    [Test]
    public async Task ShouldRequireEncodingNameWhenOtherSelected()
    {
        var options = DefaultOptions();
        options.CsvOptions = new CsvOptions { FileEncoding = FileEncoding.Other };

        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            PostgreSQL.ExecuteQueryToFile(DefaultInput(), DefaultConnection(), options, CancellationToken.None));

        Assert.That(ex.Message, Does.Contain("EncodingInString"));
    }

    [Test]
    public async Task ShouldFailWhenOutputDirectoryDoesNotExist()
    {
        var input = DefaultInput();
        input.OutputFilePath = Path.Combine(OutputDirectory, "missing", "output.csv");

        await Assert.ThrowsAsync<DirectoryNotFoundException>(() =>
            PostgreSQL.ExecuteQueryToFile(input, DefaultConnection(), DefaultOptions(), CancellationToken.None));
    }

    [Test]
    public async Task ShouldFailWhenOutputPathIsDirectory()
    {
        var input = DefaultInput();
        input.OutputFilePath = OutputDirectory + Path.DirectorySeparatorChar;

        await Assert.ThrowsAsync<ArgumentException>(() =>
            PostgreSQL.ExecuteQueryToFile(input, DefaultConnection(), DefaultOptions(), CancellationToken.None));
    }

    [Test]
    public async Task ShouldFailValidationForNullParameterItems()
    {
        var input = DefaultInput();
        input.Parameters = [null];

        await Assert.ThrowsAsync<ValidationException>(() =>
            PostgreSQL.ExecuteQueryToFile(input, DefaultConnection(), DefaultOptions(), CancellationToken.None));
    }
}
