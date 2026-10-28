using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using NUnit.Framework;

namespace Frends.PostgreSQL.ExecuteQueryToFile.Tests.HelpersTests;

[TestFixture]
internal class ErrorHandlerTest : TestBase
{
    private const string CustomErrorMessage = "CustomErrorMessage";
    private const string FailingQuery = "SELECT * FROM missing_table";

    [Test]
    public async Task Should_Throw_Original_Exception_When_ThrowErrorOnFailure_Is_True()
    {
        var ex = await Assert.ThrowsAsync<PostgresException>(() =>
            PostgreSQL.ExecuteQueryToFile(DefaultInput(FailingQuery), DefaultConnection(), DefaultOptions(), CancellationToken.None));

        Assert.That(ex.SqlState, Is.EqualTo(PostgresErrorCodes.UndefinedTable));
    }

    [Test]
    public async Task Should_Return_Failed_Result_When_ThrowErrorOnFailure_Is_False()
    {
        var options = DefaultOptions();
        options.ThrowErrorOnFailure = false;

        var result = await PostgreSQL.ExecuteQueryToFile(DefaultInput(FailingQuery), DefaultConnection(), options, CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.FilePath, Is.Null);
        Assert.That(result.RowsWritten, Is.Zero);
        Assert.That(result.Error.Message, Does.Contain("missing_table"));
        Assert.That(result.Error.AdditionalInfo, Is.InstanceOf<PostgresException>());
    }

    [Test]
    public async Task Should_Use_Custom_ErrorMessageOnFailure_And_Keep_Inner_Exception()
    {
        var options = DefaultOptions();
        options.ErrorMessageOnFailure = CustomErrorMessage;

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            PostgreSQL.ExecuteQueryToFile(DefaultInput(FailingQuery), DefaultConnection(), options, CancellationToken.None));

        Assert.That(ex.Message, Is.EqualTo(CustomErrorMessage));
        Assert.That(ex.InnerException, Is.InstanceOf<PostgresException>());
    }

    [Test]
    public async Task Should_Prefix_Custom_ErrorMessageOnFailure_In_Failed_Result()
    {
        var options = DefaultOptions();
        options.ThrowErrorOnFailure = false;
        options.ErrorMessageOnFailure = CustomErrorMessage;

        var result = await PostgreSQL.ExecuteQueryToFile(DefaultInput(FailingQuery), DefaultConnection(), options, CancellationToken.None);

        Assert.That(result.Error.Message, Does.StartWith(CustomErrorMessage + ": "));
    }

    [Test]
    public async Task Should_Report_Validation_Errors_In_Failed_Result()
    {
        var options = DefaultOptions();
        options.ThrowErrorOnFailure = false;
        var connection = DefaultConnection();
        connection.ConnectionString = string.Empty;

        var result = await PostgreSQL.ExecuteQueryToFile(DefaultInput(), connection, options, CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error.AdditionalInfo, Is.InstanceOf<ValidationException>());
    }

    [Test]
    public async Task Should_Not_Convert_Cancellation_Into_Failed_Result()
    {
        var options = DefaultOptions();
        options.ThrowErrorOnFailure = false;
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.CatchAsync<OperationCanceledException>(() =>
            PostgreSQL.ExecuteQueryToFile(DefaultInput(), DefaultConnection(), options, cts.Token));
    }
}
