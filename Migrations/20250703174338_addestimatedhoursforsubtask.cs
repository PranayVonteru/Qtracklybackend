using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Demoproject.Migrations
{
    /// <inheritdoc />
    public partial class addestimatedhoursforsubtask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EstimatedHours",
                table: "SubTasks",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EstimatedHours",
                table: "SubTasks");
        }
    }
}
