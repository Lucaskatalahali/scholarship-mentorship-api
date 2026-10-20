using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScholarshipPlatform.Migrations
{
    /// <inheritdoc />
    public partial class FixScholarshipCourseRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Scholarships_ScholarshipId",
                table: "Courses");

            migrationBuilder.DropIndex(
                name: "IX_Courses_ScholarshipId",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "ScholarshipId",
                table: "Courses");

            migrationBuilder.CreateTable(
                name: "ScholarshipCourses",
                columns: table => new
                {
                    CoursesId = table.Column<int>(type: "integer", nullable: false),
                    ScholarshipId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScholarshipCourses", x => new { x.CoursesId, x.ScholarshipId });
                    table.ForeignKey(
                        name: "FK_ScholarshipCourses_Courses_CoursesId",
                        column: x => x.CoursesId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ScholarshipCourses_Scholarships_ScholarshipId",
                        column: x => x.ScholarshipId,
                        principalTable: "Scholarships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScholarshipCourses_ScholarshipId",
                table: "ScholarshipCourses",
                column: "ScholarshipId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScholarshipCourses");

            migrationBuilder.AddColumn<int>(
                name: "ScholarshipId",
                table: "Courses",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Courses_ScholarshipId",
                table: "Courses",
                column: "ScholarshipId");

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Scholarships_ScholarshipId",
                table: "Courses",
                column: "ScholarshipId",
                principalTable: "Scholarships",
                principalColumn: "Id");
        }
    }
}
