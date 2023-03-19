using System.ComponentModel.DataAnnotations;

namespace LoginRegister.Entities;
// public class User
// {
//     public int Id { get; set; }
//     public string Username { get; set; }
//     public string PasswordHash { get; set; }
//     public string PasswordSalt { get; set; }
//     public string PasswordAlgorithm { get; set; }
// }
public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string PasswordSalt { get; set; }
    public string PasswordAlgorithm { get; set; }
}