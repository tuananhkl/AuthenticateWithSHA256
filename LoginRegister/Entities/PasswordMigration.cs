namespace LoginRegister.Entities;

public class PasswordMigration
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string OldPassword { get; set; }
    public string NewPassword { get; set; }
    public bool IsMigrated { get; set; }
}