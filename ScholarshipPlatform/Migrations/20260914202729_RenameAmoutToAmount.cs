using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScholarshipPlatform.Migrations
{
    /// <inheritdoc />
    public partial class RenameAmoutToAmount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Amout",
                table: "Payments",
                newName: "Amount");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Amount",
                table: "Payments",
                newName: "Amout");
        }
    }
}
