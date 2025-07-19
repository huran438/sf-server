using MemoryPack;
using System.ComponentModel.DataAnnotations;

namespace SFServer.Shared.Server.Admin;

[MemoryPackable]
public partial class CreateAdminRequest
{
    [Required]
    public string Username { get; set; }

    [EmailAddress]
    public string Email { get; set; }

    [Required]
    public string Password { get; set; }
}
