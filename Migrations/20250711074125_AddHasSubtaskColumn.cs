using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Demoproject.Migrations
{
    /// <inheritdoc />
    public partial class AddHasSubtaskColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HasSubtask",
                table: "Tasks",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "No");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasSubtask",
                table: "Tasks");
        }
    }
}
