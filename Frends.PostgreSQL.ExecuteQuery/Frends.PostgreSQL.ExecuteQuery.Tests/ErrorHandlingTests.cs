using System;
using System.Threading;
using System.Threading.Tasks;
using Frends.PostgreSQL.ExecuteQuery.Definitions;
using NUnit.Framework;

namespace Frends.PostgreSQL.ExecuteQuery.Tests;

[TestFixture]
public class ErrorHandlingTests
{
    [Test]
    public async Task ExecuteQueryReturnsFailureWhenThrowingIsDisabled()
    {
        var input = new Input { ConnectionString = "invalid connection string" };
        var options = new Options
        {
            ThrowErrorOnFailure = false,
            ErrorMessageOnFailure = "Unable to execute the PostgreSQL query",
        };

        var result = await PostgreSQL.ExecuteQuery(input, options, CancellationToken.None);

        Assert.IsFalse(result.Success);
        Assert.IsNull(result.QueryResult);
        Assert.That(result.Error.Message, Does.StartWith("Unable to execute the PostgreSQL query: "));
        Assert.IsInstanceOf<ArgumentException>(result.Error.AdditionalInfo);
    }

    [Test]
    public void ExecuteQueryStillThrowsByDefault()
    {
        var input = new Input { ConnectionString = "invalid connection string" };

        Assert.ThrowsAsync<ArgumentException>(async () =>
            await PostgreSQL.ExecuteQuery(input, new Options(), CancellationToken.None));
    }
}
