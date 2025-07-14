using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthServices.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPasswordFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "Users",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "PasswordSalt",
                table: "Users",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "PasswordHash", table: "Users");

            migrationBuilder.DropColumn(name: "PasswordSalt", table: "Users");
        }
    }
}
