using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyCI.Migrations
{
    /// <inheritdoc />
    public partial class add_columns_controlemenu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DockerContainers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Host = table.Column<string>(type: "TEXT", nullable: false),
                    Port = table.Column<int>(type: "INTEGER", nullable: false),
                    ApiVersion = table.Column<string>(type: "TEXT", nullable: false),
                    UseTLS = table.Column<bool>(type: "INTEGER", nullable: false),
                    CertificatePath = table.Column<string>(type: "TEXT", nullable: false),
                    RemoteWorkspacePath = table.Column<string>(type: "TEXT", nullable: false),
                    UseDockerApi = table.Column<bool>(type: "INTEGER", nullable: false),
                    DateAdded = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DockerContainers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GitRepositories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Url = table.Column<string>(type: "TEXT", nullable: false),
                    Branch = table.Column<string>(type: "TEXT", nullable: false),
                    SshKeyPath = table.Column<string>(type: "TEXT", nullable: false),
                    Username = table.Column<string>(type: "TEXT", nullable: false),
                    DateAdded = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GitRepositories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CIProjects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    GitRepositoryId = table.Column<int>(type: "INTEGER", nullable: false),
                    DockerContainerId = table.Column<int>(type: "INTEGER", nullable: false),
                    ComposeFilePath = table.Column<string>(type: "TEXT", nullable: false),
                    AutoBuild = table.Column<bool>(type: "INTEGER", nullable: false),
                    LastBuildDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CIProjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CIProjects_DockerContainers_DockerContainerId",
                        column: x => x.DockerContainerId,
                        principalTable: "DockerContainers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CIProjects_GitRepositories_GitRepositoryId",
                        column: x => x.GitRepositoryId,
                        principalTable: "GitRepositories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CIProjects_DockerContainerId",
                table: "CIProjects",
                column: "DockerContainerId");

            migrationBuilder.CreateIndex(
                name: "IX_CIProjects_GitRepositoryId",
                table: "CIProjects",
                column: "GitRepositoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CIProjects");

            migrationBuilder.DropTable(
                name: "DockerContainers");

            migrationBuilder.DropTable(
                name: "GitRepositories");
        }
    }
}
