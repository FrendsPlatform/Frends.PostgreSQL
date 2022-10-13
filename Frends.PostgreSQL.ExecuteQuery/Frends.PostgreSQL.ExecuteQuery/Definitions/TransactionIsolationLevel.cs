namespace Frends.PostgreSQL.ExecuteQuery.Definitions
{
#pragma warning disable CS1591 // Self-explanatory. Information can be found from PostgreSQL documentation.
    public enum TransactionIsolationLevel
    {
        Default,
        ReadCommited,
        None,
        Serializable,
        ReadUncommited,
        RepeatableRead,
        Snapshot
    }
#pragma warning restore CS1591
}
