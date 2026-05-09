using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimelyBackEnd.Migrations
{
    /// <inheritdoc />
    public partial class AddImageUrlToHomework : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Homeworks",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Homeworks");
        }
    }
}
