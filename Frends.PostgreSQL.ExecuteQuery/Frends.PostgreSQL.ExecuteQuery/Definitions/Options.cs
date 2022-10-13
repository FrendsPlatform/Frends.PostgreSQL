using System.ComponentModel;
namespace Frends.PostgreSQL.ExecuteQuery.Definitions;

/// <summary>
/// Options-class for ExecuteQuery-task.
/// </summary>
public class Options
{

    /// <summary>
    /// Timeout in seconds.
    /// </summary>
    /// <example>30</example>
    [DefaultValue(30)]
    public int CommandTimeoutSeconds { get; set; }

    /// <summary>
    /// Transaction isolation level for the query.
    /// Options:
    ///     - Default
    ///     - ReadCommited
    ///     - None
    ///     - Serializable
    ///     - ReadUncommited
    ///     - RepeatableRead
    ///     - Snapshot
    /// Additional information can be found from [here](https://www.postgresql.org/docs/current/transaction-iso.html)
    /// </summary>
    /// <example>Default</example>
    [DefaultValue(TransactionIsolationLevel.Default)]
    public TransactionIsolationLevel SqlTransactionIsolationLevel { get; set; }

}
