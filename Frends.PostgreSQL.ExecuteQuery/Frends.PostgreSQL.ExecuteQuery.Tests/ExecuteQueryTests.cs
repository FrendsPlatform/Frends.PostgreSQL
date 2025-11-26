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
        using (var cmd = new NpgsqlCommand(@"INSERT INTO ""lista"" (Id, Selite) VALUES (1, 'Ensimm�inen'), (2, 'foobar'), (3, ''), (4, null)", conn))
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
            ConnectionString = _connection
        };

        var result = await PostgreSQL.ExecuteQuery(input, _options, new CancellationToken());

        Assert.AreEqual(1, (int)result.QueryResult[0]["id"]);
        Assert.AreEqual(2, (int)result.QueryResult[1]["id"]);
        Assert.AreEqual(3, (int)result.QueryResult[2]["id"]);
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
        Assert.AreEqual(0, result.QueryResult.Count);
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
        Assert.AreEqual(1, (int)result.QueryResult["AffectedRows"]);

        input.Query = "SELECT * from lista WHERE id=5";
        result = await PostgreSQL.ExecuteQuery(input, _options, new CancellationToken());
        Assert.AreEqual("Viides", (string)result.QueryResult[0]["selite"]);
    }

    /// <summary>
    /// Test INSERT with RETURNING clause.
    /// </summary>
    [Test]
    public async Task TestInsertWithReturning()
    {
        var input = new Input
        {
            Query = @"INSERT INTO ""lista"" (Id, Selite) VALUES (6, 'Kuudes') RETURNING Id, Selite",
            Parameters = null,
            ConnectionString = _connection
        };

        var result = await PostgreSQL.ExecuteQuery(input, _options, new CancellationToken());
        
        // Should return the inserted values, not AffectedRows
        Assert.IsNotNull(result.QueryResult);
        Assert.AreEqual(1, result.QueryResult.Count);
        Assert.AreEqual(6, (int)result.QueryResult[0]["id"]);
        Assert.AreEqual("Kuudes", (string)result.QueryResult[0]["selite"]);
    }

    /// <summary>
    /// Test UPDATE with RETURNING clause.
    /// </summary>
    [Test]
    public async Task TestUpdateWithReturning()
    {
        var input = new Input
        {
            Query = @"UPDATE ""lista"" SET Selite = 'Updated' WHERE Id = 1 RETURNING Id, Selite",
            Parameters = null,
            ConnectionString = _connection
        };

        var result = await PostgreSQL.ExecuteQuery(input, _options, new CancellationToken());
        
        // Should return the updated values
        Assert.IsNotNull(result.QueryResult);
        Assert.AreEqual(1, result.QueryResult.Count);
        Assert.AreEqual(1, (int)result.QueryResult[0]["id"]);
        Assert.AreEqual("Updated", (string)result.QueryResult[0]["selite"]);

        // Restore original value
        input.Query = @"UPDATE ""lista"" SET Selite = 'Ensimm�inen' WHERE Id = 1";
        await PostgreSQL.ExecuteQuery(input, _options, new CancellationToken());
    }

    /// <summary>
    /// Test DELETE with RETURNING clause.
    /// </summary>
    [Test]
    public async Task TestDeleteWithReturning()
    {
        // First, insert a row to delete
        var input = new Input
        {
            Query = @"INSERT INTO ""lista"" (Id, Selite) VALUES (7, 'Seitsem�s')",
            Parameters = null,
            ConnectionString = _connection
        };
        await PostgreSQL.ExecuteQuery(input, _options, new CancellationToken());

        // Now delete it with RETURNING
        input.Query = @"DELETE FROM ""lista"" WHERE Id = 7 RETURNING Id, Selite";
        var result = await PostgreSQL.ExecuteQuery(input, _options, new CancellationToken());
        
        // Should return the deleted values
        Assert.IsNotNull(result.QueryResult);
        Assert.AreEqual(1, result.QueryResult.Count);
        Assert.AreEqual(7, (int)result.QueryResult[0]["id"]);
        Assert.AreEqual("Seitsem�s", (string)result.QueryResult[0]["selite"]);
    }

    /// <summary>
    /// Test INSERT with RETURNING clause using ExecuteType.ExecuteReader explicitly.
    /// </summary>
    [Test]
    public async Task TestInsertWithReturningExplicit()
    {
        var input = new Input
        {
            Query = @"INSERT INTO ""lista"" (Id, Selite) VALUES (8, 'Kahdeksas') RETURNING Id, Selite",
            Parameters = null,
            ConnectionString = _connection,
            ExecuteType = ExecuteTypes.ExecuteReader
        };

        var result = await PostgreSQL.ExecuteQuery(input, _options, new CancellationToken());
        
        // Should return the inserted values
        Assert.IsNotNull(result.QueryResult);
        Assert.AreEqual(1, result.QueryResult.Count);
        Assert.AreEqual(8, (int)result.QueryResult[0]["id"]);
        Assert.AreEqual("Kahdeksas", (string)result.QueryResult[0]["selite"]);
    }

    /// <summary>
    /// Test INSERT without RETURNING using ExecuteType.NonQuery explicitly.
    /// </summary>
    [Test]
    public async Task TestInsertWithNonQueryExplicit()
    {
        var input = new Input
        {
            Query = @"INSERT INTO ""lista"" (Id, Selite) VALUES (9, 'Yhdeks�s')",
            Parameters = null,
            ConnectionString = _connection,
            ExecuteType = ExecuteTypes.NonQuery
        };

        var result = await PostgreSQL.ExecuteQuery(input, _options, new CancellationToken());
        
        // Should return affected rows
        Assert.AreEqual(1, (int)result.QueryResult["AffectedRows"]);
    }
}
