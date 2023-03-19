using Microsoft.EntityFrameworkCore.Migrations;

namespace LoginRegister.Migrations;

public partial class AddPasswordMigrationsTable : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "PasswordMigrations",
            columns: table => new
            {
                UserId = table.Column<int>(nullable: false),
                OldPassword = table.Column<string>(nullable: true),
                NewPassword = table.Column<string>(nullable: true),
                IsMigrated = table.Column<bool>(nullable: false, defaultValue: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PasswordMigrations", x => x.UserId);
                table.ForeignKey(
                    name: "FK_PasswordMigrations_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "PasswordMigrations");
    }
}
