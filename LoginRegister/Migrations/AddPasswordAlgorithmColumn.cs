using Microsoft.EntityFrameworkCore.Migrations;

namespace LoginRegister.Migrations;

public partial class AddPasswordAlgorithmColumn : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "PasswordAlgorithm",
            table: "Users",
            nullable: false,
            defaultValue: "SHA256");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "PasswordAlgorithm",
            table: "Users");
    }
}