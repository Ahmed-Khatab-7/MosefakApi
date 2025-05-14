using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MosefakApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFirebaseFieldsToAppUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FirebaseUid",
                schema: "Security",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirebaseUid",
                schema: "Security",
                table: "Users");
        }
    }
}
