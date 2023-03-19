using System.ComponentModel.DataAnnotations;

namespace LoginRegister.DTOs;

public class LoginModel
{
    [Required]
    public string Username { get; set; }

    [Required]
    public string Password { get; set; }
}