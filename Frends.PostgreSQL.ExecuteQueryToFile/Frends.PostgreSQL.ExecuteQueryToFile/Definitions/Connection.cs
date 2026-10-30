using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.PostgreSQL.ExecuteQueryToFile.Definitions;

/// <summary>
/// Connection parameters.
/// </summary>
public class Connection
{
    /// <summary>
    /// Connection string to the PostgreSQL database.
    /// </summary>
    /// <example>Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=secret</example>
    [PasswordPropertyText]
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("")]
    [Required]
    public string ConnectionString { get; set; } = string.Empty;
}
