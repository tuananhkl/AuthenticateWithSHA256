using System.ComponentModel.DataAnnotations;

namespace LoginRegister.DTOs;

public class RegisterModel
{
    [Required]
    public string Username { get; set; }

    [Required]
    [StringLength(20, MinimumLength = 6)]
    public string Password { get; set; }
}