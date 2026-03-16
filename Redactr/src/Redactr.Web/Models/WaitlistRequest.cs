using System.ComponentModel.DataAnnotations;

namespace Redactr.Web.Models;

public class WaitlistRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;
}
