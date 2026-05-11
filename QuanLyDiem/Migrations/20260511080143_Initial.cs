using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyDiem.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Courses",
                columns: table => new
                {
                    CourseID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CourseName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Credits = table.Column<int>(type: "int", nullable: false),
                    ProcessWeight = table.Column<decimal>(type: "decimal(4,2)", nullable: false),
                    FinalWeight = table.Column<decimal>(type: "decimal(4,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Courses", x => x.CourseID);
                });

            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    StudentID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClassID = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.StudentID);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserID);
                });

            migrationBuilder.CreateTable(
                name: "Lecturers",
                columns: table => new
                {
                    LecturerID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Department = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lecturers", x => x.LecturerID);
                    table.ForeignKey(
                        name: "FK_Lecturers_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SectionClasses",
                columns: table => new
                {
                    SectionClassID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CourseID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LecturerID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SemesterID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaxCapacity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SectionClasses", x => x.SectionClassID);
                    table.ForeignKey(
                        name: "FK_SectionClasses_Courses_CourseID",
                        column: x => x.CourseID,
                        principalTable: "Courses",
                        principalColumn: "CourseID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SectionClasses_Lecturers_LecturerID",
                        column: x => x.LecturerID,
                        principalTable: "Lecturers",
                        principalColumn: "LecturerID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClassGradeSummaries",
                columns: table => new
                {
                    SectionClassID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    StudentID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AttendanceScore = table.Column<decimal>(type: "decimal(4,2)", nullable: false),
                    MidtermScore = table.Column<decimal>(type: "decimal(4,2)", nullable: false),
                    FinalScore = table.Column<decimal>(type: "decimal(4,2)", nullable: false),
                    AttendanceWeight = table.Column<decimal>(type: "decimal(4,2)", nullable: false),
                    MidtermWeight = table.Column<decimal>(type: "decimal(4,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassGradeSummaries", x => new { x.SectionClassID, x.StudentID });
                    table.ForeignKey(
                        name: "FK_ClassGradeSummaries_SectionClasses_SectionClassID",
                        column: x => x.SectionClassID,
                        principalTable: "SectionClasses",
                        principalColumn: "SectionClassID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClassGradeSummaries_Students_StudentID",
                        column: x => x.StudentID,
                        principalTable: "Students",
                        principalColumn: "StudentID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClassSchedules",
                columns: table => new
                {
                    ScheduleID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SectionClassID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false),
                    StartSlot = table.Column<int>(type: "int", nullable: false),
                    SlotCount = table.Column<int>(type: "int", nullable: false),
                    RoomID = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassSchedules", x => x.ScheduleID);
                    table.ForeignKey(
                        name: "FK_ClassSchedules_SectionClasses_SectionClassID",
                        column: x => x.SectionClassID,
                        principalTable: "SectionClasses",
                        principalColumn: "SectionClassID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Enrollments",
                columns: table => new
                {
                    SectionClassID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    StudentID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EnrollmentType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Grade10 = table.Column<decimal>(type: "decimal(4,2)", nullable: false),
                    Grade4 = table.Column<decimal>(type: "decimal(4,2)", nullable: false),
                    LetterGrade = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enrollments", x => new { x.SectionClassID, x.StudentID });
                    table.ForeignKey(
                        name: "FK_Enrollments_SectionClasses_SectionClassID",
                        column: x => x.SectionClassID,
                        principalTable: "SectionClasses",
                        principalColumn: "SectionClassID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Enrollments_Students_StudentID",
                        column: x => x.StudentID,
                        principalTable: "Students",
                        principalColumn: "StudentID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubGradeDetails",
                columns: table => new
                {
                    SubGradeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SectionClassID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    StudentID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    GradeCategory = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AssessmentMethod = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Score = table.Column<decimal>(type: "decimal(4,2)", nullable: false),
                    Weight = table.Column<decimal>(type: "decimal(4,2)", nullable: false),
                    AssessmentNote = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubGradeDetails", x => x.SubGradeID);
                    table.ForeignKey(
                        name: "FK_SubGradeDetails_SectionClasses_SectionClassID",
                        column: x => x.SectionClassID,
                        principalTable: "SectionClasses",
                        principalColumn: "SectionClassID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubGradeDetails_Students_StudentID",
                        column: x => x.StudentID,
                        principalTable: "Students",
                        principalColumn: "StudentID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClassGradeSummaries_StudentID",
                table: "ClassGradeSummaries",
                column: "StudentID");

            migrationBuilder.CreateIndex(
                name: "IX_ClassSchedules_SectionClassID",
                table: "ClassSchedules",
                column: "SectionClassID");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_StudentID",
                table: "Enrollments",
                column: "StudentID");

            migrationBuilder.CreateIndex(
                name: "IX_Lecturers_UserID",
                table: "Lecturers",
                column: "UserID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SectionClasses_CourseID",
                table: "SectionClasses",
                column: "CourseID");

            migrationBuilder.CreateIndex(
                name: "IX_SectionClasses_LecturerID",
                table: "SectionClasses",
                column: "LecturerID");

            migrationBuilder.CreateIndex(
                name: "IX_SubGradeDetails_SectionClassID",
                table: "SubGradeDetails",
                column: "SectionClassID");

            migrationBuilder.CreateIndex(
                name: "IX_SubGradeDetails_StudentID",
                table: "SubGradeDetails",
                column: "StudentID");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClassGradeSummaries");

            migrationBuilder.DropTable(
                name: "ClassSchedules");

            migrationBuilder.DropTable(
                name: "Enrollments");

            migrationBuilder.DropTable(
                name: "SubGradeDetails");

            migrationBuilder.DropTable(
                name: "SectionClasses");

            migrationBuilder.DropTable(
                name: "Students");

            migrationBuilder.DropTable(
                name: "Courses");

            migrationBuilder.DropTable(
                name: "Lecturers");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
