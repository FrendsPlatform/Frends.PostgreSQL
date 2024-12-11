using Frends.PostgreSQL.ExecuteQuery.Definitions;
using Npgsql;
using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Frends.PostgreSQL.ExecuteQuery.Tests;

[TestFixture]
public class ExecuteQueryTests
{

	/// <summary>
	/// These test requires local postgres database, create it e.g. with
	///
	///  docker run -p 5432:5432 -e POSTGRES_PASSWORD=mysecretpassword -d postgres
	///
	/// </summary>

	private readonly string _connection = "Host=localhost;Database=postgres;Port=5432;User Id=postgres;Password=mysecretpassword;";

	private readonly Options _options = new()
    {
        CommandTimeoutSeconds = 10,
        SqlTransactionIsolationLevel = TransactionIsolationLevel.Default
    };

    [OneTimeSetUp]
    public void TestSetup()
    {
        using var conn = new NpgsqlConnection(_connection);
        conn.Open();

        using (var cmd = new NpgsqlCommand(@"CREATE TABLE IF NOT EXISTS ""lista"" (Id int, Selite varchar)", conn))
        {
            cmd.ExecuteNonQuery();
        }
        using (var cmd = new NpgsqlCommand(@"INSERT INTO ""lista"" (Id, Selite) VALUES (1, 'Ensimmäinen'), (2, 'foobar'), (3, ''), (4, null)", conn))
        {
            cmd.ExecuteNonQuery();
        }
        conn.Close();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        using var conn = new NpgsqlConnection(_connection);
        conn.Open();

        using var cmd = new NpgsqlCommand(@"DROP TABLE ""lista""", conn);
        cmd.ExecuteNonQuery();
        conn.Close();
    }

    /// <summary>
    /// Check that returned id values are 1,2,3.
    /// </summary>
    /// 
    [Test]
    public async Task QuerydataThreeRows()
    {
        var input = new Input
        {
            Query = "SELECT * FROM lista",
            Parameters = null,
            ConnectionString = _connection,
			ExecuteType = ExecuteTypes.ExecuteReader
        };

        var result = await PostgreSQL.ExecuteQuery(input, _options, new CancellationToken());

        Assert.AreEqual(1, (int)result.Data[0]["id"]);
        Assert.AreEqual(2, (int)result.Data[1]["id"]);
        Assert.AreEqual(3, (int)result.Data[2]["id"]);
    }

    /// <summary>
    /// Check that returns no rows with wrong id.
    /// </summary>
    [Test]
    public async Task QuerydataNoRows()
    {
        var input = new Input
        {
            Query = "SELECT * from lista WHERE id=:ehto",
            Parameters = new[] { new Parameter { Name = "ehto", Value = 0 } },
            ConnectionString = _connection
        };


        var result = await PostgreSQL.ExecuteQuery(input, _options, new CancellationToken());
        Assert.AreEqual(0, result.Data.Count);
    }

    /// <summary>
    /// Test non-query operation.
    /// </summary>
    [Test]
    public async Task TestInsertQuery()
    {
        var input = new Input
        {
            Query = @"INSERT INTO ""lista"" (Id, Selite) VALUES (5, 'Viides')",
            Parameters = null,
            ConnectionString = _connection
        };

        var result = await PostgreSQL.ExecuteQuery(input, _options, new CancellationToken());
        Assert.AreEqual(1, result.RecordsAffected);

        input.Query = "SELECT * from lista WHERE id=5";
        result = await PostgreSQL.ExecuteQuery(input, _options, new CancellationToken());
        Assert.AreEqual("Viides", (string)result.Data[0]["selite"]);
    }

	/// <summary>
	/// Test non-query operation with select.
	/// </summary>
	[Test]
	public async Task TestInsertSelectQuery()
	{
		var input = new Input
		{
			Query = @"
						INSERT INTO lista (Id, Selite)
						SELECT 99, Selite
						FROM lista WHERE Id = 1
					",
			Parameters = null,
			ExecuteType = ExecuteTypes.NonQuery,
			ConnectionString = _connection
		};

		var result = await PostgreSQL.ExecuteQuery(input, _options, new CancellationToken());
		Assert.AreEqual(1, result.RecordsAffected);

		input.Query = "SELECT * from lista WHERE id=99";
		input.ExecuteType = ExecuteTypes.ExecuteReader;
		result = await PostgreSQL.ExecuteQuery(input, _options, new CancellationToken());
		Assert.AreEqual("Ensimmäinen", (string)result.Data[0]["selite"]);
	}
}
