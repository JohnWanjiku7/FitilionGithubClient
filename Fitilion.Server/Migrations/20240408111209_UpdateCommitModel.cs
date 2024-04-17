using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fitilion.Server.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCommitModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RepoName",
                table: "GitHubCommits",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RepoOwner",
                table: "GitHubCommits",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RepoName",
                table: "GitHubCommits");

            migrationBuilder.DropColumn(
                name: "RepoOwner",
                table: "GitHubCommits");
        }
    }
}
