using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScholarshipPlatform.Migrations
{
    /// <inheritdoc />
    public partial class addProprietiesToScholarship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Eligibility",
                table: "Scholarships",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HowToApply",
                table: "Scholarships",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OfficialUrl",
                table: "Scholarships",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequiredDocuments",
                table: "Scholarships",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Eligibility",
                table: "Scholarships");

            migrationBuilder.DropColumn(
                name: "HowToApply",
                table: "Scholarships");

            migrationBuilder.DropColumn(
                name: "OfficialUrl",
                table: "Scholarships");

            migrationBuilder.DropColumn(
                name: "RequiredDocuments",
                table: "Scholarships");
        }
    }
}
