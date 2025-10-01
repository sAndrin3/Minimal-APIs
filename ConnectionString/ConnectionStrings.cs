using System.ComponentModel.DataAnnotations;

namespace MinimalApi.ConnectionString;

public class ConnectionStrings
{
    public const string Section = "ConnectionStrings";
    [Required]
    public required string DB { get; set; }
    [Required]
    public required string Redis { get; set; }
}
