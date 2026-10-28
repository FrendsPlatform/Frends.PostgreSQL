using System;
using System.IO;
using System.Linq;
using Frends.PostgreSQL.ExecuteQueryToFile.Definitions;
using Npgsql;
using NUnit.Framework;

namespace Frends.PostgreSQL.ExecuteQueryToFile.Tests;

internal abstract class TestBase
{
    protected const string TableName = "etf_test_table";

    protected static string ConnectionString => PostgresFixture.ConnectionString;

    protected string OutputDirectory { get; private set; }

    protected string OutputPath => Path.Combine(OutputDirectory, "output.out");

    [OneTimeSetUp]
    public void CreateTestTable()
    {
        Execute("""
                DROP TABLE IF EXISTS etf_test_table;
                CREATE TABLE etf_test_table (
                    id int PRIMARY KEY,
                    name text,
                    amount numeric(10, 2),
                    flag boolean,
                    created date,
                    updated timestamp,
                    doc jsonb,
                    tags int[],
                    data bytea);
                INSERT INTO etf_test_table VALUES
                    (1, 'Erik', 12.50, true, '2024-03-05', '2024-03-05 10:11:12', '{"a": [1, 2]}', '{1,2,3}', '\x0102'),
                    (2, 'Comma, "Quote" and' || chr(10) || 'newline', 0.10, false, '2023-12-31', '2023-12-31 23:59:59', null, null, null),
                    (3, null, null, null, null, null, null, null, null),
                    (4, '', 1000.00, true, '2020-01-01', '2020-01-01 00:00:00', '[]', '{}', '\x');
                """);
    }

    [OneTimeTearDown]
    public void DropTestTable() => Execute($"DROP TABLE IF EXISTS {TableName}");

    [SetUp]
    public void CreateOutputDirectory()
    {
        OutputDirectory = Path.Combine(Path.GetTempPath(), "etf_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(OutputDirectory);
    }

    [TearDown]
    public void DeleteOutputDirectory()
    {
        if (Directory.Exists(OutputDirectory)) Directory.Delete(OutputDirectory, true);
    }

    protected static Options DefaultOptions() => new();

    protected Input DefaultInput(string query = null) => new()
    {
        Query = query ?? $"SELECT * FROM {TableName} ORDER BY id",
        OutputFilePath = OutputPath,
    };

    protected Connection DefaultConnection() => new() { ConnectionString = ConnectionString };

    protected string[] FilesInOutputDirectory() =>
        Directory.GetFiles(OutputDirectory, "*", SearchOption.TopDirectoryOnly)
            .Select(Path.GetFileName)
            .ToArray();

    private void Execute(string sql)
    {
        using var connection = new NpgsqlConnection(ConnectionString);
        connection.Open();
        using var command = new NpgsqlCommand(sql, connection);
        command.ExecuteNonQuery();
    }
}
