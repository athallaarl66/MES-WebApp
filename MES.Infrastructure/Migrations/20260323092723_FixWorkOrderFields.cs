using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MES.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixWorkOrderFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "id",
                table: "WorkOrders",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ProductNumber",
                table: "WorkOrders",
                newName: "ProductName");

            migrationBuilder.AddColumn<string>(
                name: "ProductCode",
                table: "WorkOrders",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProductCode",
                table: "WorkOrders");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "WorkOrders",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "ProductName",
                table: "WorkOrders",
                newName: "ProductNumber");
        }
    }
}
