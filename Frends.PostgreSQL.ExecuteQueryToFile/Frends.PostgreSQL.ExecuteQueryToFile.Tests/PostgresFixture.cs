using System.Threading.Tasks;
using NUnit.Framework;
using Testcontainers.PostgreSql;

namespace Frends.PostgreSQL.ExecuteQueryToFile.Tests;

[SetUpFixture]
public class PostgresFixture
{
    private static PostgreSqlContainer container;

    internal static string ConnectionString => container.GetConnectionString();

    [OneTimeSetUp]
    public async Task StartContainer()
    {
        container = new PostgreSqlBuilder("postgres:16").Build();
        await container.StartAsync();
    }

    [OneTimeTearDown]
    public async Task StopContainer() => await container.DisposeAsync();
}
