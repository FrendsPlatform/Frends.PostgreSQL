namespace Frends.PostgreSQL.ExecuteQuery.Definitions
{
    /// <summary>
    /// Isolation level for a PostgreSQL transaction.
    /// </summary>
    public enum TransactionIsolationLevel
    {
        /// <summary>Use the default isolation level.</summary>
        Default,

        /// <summary>Read committed data.</summary>
        ReadCommited,

        /// <summary>Do not specify an isolation level.</summary>
        None,

        /// <summary>Use serializable isolation.</summary>
        Serializable,

        /// <summary>Read uncommitted data.</summary>
        ReadUncommited,

        /// <summary>Use repeatable read isolation.</summary>
        RepeatableRead,

        /// <summary>Use snapshot isolation.</summary>
        Snapshot,
    }
}
