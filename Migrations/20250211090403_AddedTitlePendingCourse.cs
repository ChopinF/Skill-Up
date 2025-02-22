using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace platform.Migrations
{
    /// <inheritdoc />
    public partial class AddedTitlePendingCourse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "PendingCourses",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Title",
                table: "PendingCourses");
        }
    }
}
