using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MosefakApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SomeOfModificatiionsForAppuser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropColumn(
            //    name: "Address_State",
            //    schema: "Security",
            //    table: "Users");

            //migrationBuilder.DropColumn(
            //    name: "Address_ZipCode",
            //    schema: "Security",
            //    table: "Users");

            //migrationBuilder.AlterColumn<string>(
            //    name: "Address_Street",
            //    schema: "Security",
            //    table: "Users",
            //    type: "nvarchar(max)",
            //    nullable: true,
            //    oldClrType: typeof(string),
            //    oldType: "nvarchar(max)");

            //migrationBuilder.AlterColumn<int>(
            //    name: "Address_Id",
            //    schema: "Security",
            //    table: "Users",
            //    type: "int",
            //    nullable: true,
            //    oldClrType: typeof(int),
            //    oldType: "int");

            //migrationBuilder.AlterColumn<string>(
            //    name: "Address_City",
            //    schema: "Security",
            //    table: "Users",
            //    type: "nvarchar(max)",
            //    nullable: true,
            //    oldClrType: typeof(string),
            //    oldType: "nvarchar(max)");

            //migrationBuilder.AddColumn<string>(
            //    name: "Address_Country",
            //    schema: "Security",
            //    table: "Users",
            //    type: "nvarchar(max)",
            //    nullable: true);

            //migrationBuilder.CreateIndex(
            //    name: "IX_Users_Email",
            //    schema: "Security",
            //    table: "Users",
            //    column: "Email",
            //    unique: true,
            //    filter: "[Email] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropIndex(
            //    name: "IX_Users_Email",
            //    schema: "Security",
            //    table: "Users");

            //migrationBuilder.DropColumn(
            //    name: "Address_Country",
            //    schema: "Security",
            //    table: "Users");

            //migrationBuilder.AlterColumn<string>(
            //    name: "Address_Street",
            //    schema: "Security",
            //    table: "Users",
            //    type: "nvarchar(max)",
            //    nullable: false,
            //    defaultValue: "",
            //    oldClrType: typeof(string),
            //    oldType: "nvarchar(max)",
            //    oldNullable: true);

            //migrationBuilder.AlterColumn<int>(
            //    name: "Address_Id",
            //    schema: "Security",
            //    table: "Users",
            //    type: "int",
            //    nullable: false,
            //    defaultValue: 0,
            //    oldClrType: typeof(int),
            //    oldType: "int",
            //    oldNullable: true);

            //migrationBuilder.AlterColumn<string>(
            //    name: "Address_City",
            //    schema: "Security",
            //    table: "Users",
            //    type: "nvarchar(max)",
            //    nullable: false,
            //    defaultValue: "",
            //    oldClrType: typeof(string),
            //    oldType: "nvarchar(max)",
            //    oldNullable: true);

            //migrationBuilder.AddColumn<string>(
            //    name: "Address_State",
            //    schema: "Security",
            //    table: "Users",
            //    type: "nvarchar(max)",
            //    nullable: false,
            //    defaultValue: "");

            //migrationBuilder.AddColumn<int>(
            //    name: "Address_ZipCode",
            //    schema: "Security",
            //    table: "Users",
            //    type: "int",
            //    nullable: false,
            //    defaultValue: 0);
        }
    }
}
