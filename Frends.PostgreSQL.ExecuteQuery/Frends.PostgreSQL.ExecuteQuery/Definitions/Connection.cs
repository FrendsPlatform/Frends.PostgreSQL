using System.ComponentModel;

namespace Frends.PostgreSQL.ExecuteQuery.Definitions;

/// <summary>
/// Connection settings for the PostgreSQL database.
/// </summary>
public class Connection
{
    /// <summary>
    /// Connection string.
    /// </summary>
    /// <example>Server=myServerAddress;Database=myDataBase;User Id=myUsername;******;</example>
    [PasswordPropertyText]
    public string ConnectionString { get; set; }
}
