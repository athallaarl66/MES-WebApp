using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MES.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStepNameToActivityLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StepName",
                table: "ActivityLogs",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StepName",
                table: "ActivityLogs");
        }
    }
}
