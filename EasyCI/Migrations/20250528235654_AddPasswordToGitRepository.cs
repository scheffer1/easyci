using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyCI.Migrations
{
    /// <inheritdoc />
    public partial class AddPasswordToGitRepository : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Password",
                table: "GitRepositories",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Password",
                table: "GitRepositories");
        }
    }
}
