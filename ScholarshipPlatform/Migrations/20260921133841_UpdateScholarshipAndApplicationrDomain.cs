using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ScholarshipPlatform.Migrations
{
    /// <inheritdoc />
    public partial class UpdateScholarshipAndApplicationrDomain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScholarshipApplications_Scholarships_ScholarshipId",
                table: "ScholarshipApplications");

            migrationBuilder.DropColumn(
                name: "HowToApply",
                table: "Scholarships");

            migrationBuilder.DropColumn(
                name: "Gpa",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "Scholarships",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "ScholarshipApplications",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Payments",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "BirthDate",
                table: "AspNetUsers",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1),
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AccountStatus",
                table: "AspNetUsers",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "Address_AddressLine",
                table: "AspNetUsers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Address_Country",
                table: "AspNetUsers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Address_Province",
                table: "AspNetUsers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Average",
                table: "AspNetUsers",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "EducationLevel",
                table: "AspNetUsers",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Courses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    ScholarshipId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Courses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Courses_Scholarships_ScholarshipId",
                        column: x => x.ScholarshipId,
                        principalTable: "Scholarships",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ApplicationEnrolledCourses",
                columns: table => new
                {
                    EnrolledCoursesId = table.Column<int>(type: "integer", nullable: false),
                    ScholarshipApplication1Id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationEnrolledCourses", x => new { x.EnrolledCoursesId, x.ScholarshipApplication1Id });
                    table.ForeignKey(
                        name: "FK_ApplicationEnrolledCourses_Courses_EnrolledCoursesId",
                        column: x => x.EnrolledCoursesId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApplicationEnrolledCourses_ScholarshipApplications_Scholars~",
                        column: x => x.ScholarshipApplication1Id,
                        principalTable: "ScholarshipApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationSelectedCourses",
                columns: table => new
                {
                    ScholarshipApplicationId = table.Column<int>(type: "integer", nullable: false),
                    SelectedCoursesId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationSelectedCourses", x => new { x.ScholarshipApplicationId, x.SelectedCoursesId });
                    table.ForeignKey(
                        name: "FK_ApplicationSelectedCourses_Courses_SelectedCoursesId",
                        column: x => x.SelectedCoursesId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApplicationSelectedCourses_ScholarshipApplications_Scholars~",
                        column: x => x.ScholarshipApplicationId,
                        principalTable: "ScholarshipApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationEnrolledCourses_ScholarshipApplication1Id",
                table: "ApplicationEnrolledCourses",
                column: "ScholarshipApplication1Id");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationSelectedCourses_SelectedCoursesId",
                table: "ApplicationSelectedCourses",
                column: "SelectedCoursesId");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_ScholarshipId",
                table: "Courses",
                column: "ScholarshipId");

            migrationBuilder.AddForeignKey(
                name: "FK_ScholarshipApplications_Scholarships_ScholarshipId",
                table: "ScholarshipApplications",
                column: "ScholarshipId",
                principalTable: "Scholarships",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScholarshipApplications_Scholarships_ScholarshipId",
                table: "ScholarshipApplications");

            migrationBuilder.DropTable(
                name: "ApplicationEnrolledCourses");

            migrationBuilder.DropTable(
                name: "ApplicationSelectedCourses");

            migrationBuilder.DropTable(
                name: "Courses");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "Scholarships");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "Address_AddressLine",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Address_Country",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Address_Province",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Average",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "EducationLevel",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<string>(
                name: "HowToApply",
                table: "Scholarships",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "ScholarshipApplications",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "BirthDate",
                table: "AspNetUsers",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AlterColumn<int>(
                name: "AccountStatus",
                table: "AspNetUsers",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30);

            migrationBuilder.AddColumn<decimal>(
                name: "Gpa",
                table: "AspNetUsers",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ScholarshipApplications_Scholarships_ScholarshipId",
                table: "ScholarshipApplications",
                column: "ScholarshipId",
                principalTable: "Scholarships",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
