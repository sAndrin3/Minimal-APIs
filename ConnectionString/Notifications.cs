using System.ComponentModel.DataAnnotations;

namespace MinimalApi.ConnectionString;

public class Notifications
{
    public const string Section =  "Notifications";
    [Required]
    public required string NotificationType { get; set; }
}
