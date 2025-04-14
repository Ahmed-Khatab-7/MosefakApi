using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MosefakApp.Infrastructure.Migrations.AppDb
{
    /// <inheritdoc />
    public partial class addcontactus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            ////migrationBuilder.AddColumn<string>(
            ////    name: "FcmToken",
            ////    table: "AppUser",
            ////    type: "nvarchar(max)",
            ////    nullable: true);

            //migrationBuilder.CreateTable(
            //    name: "ContactUs",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        AppUserId = table.Column<int>(type: "int", nullable: false),
            //        CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
            //        CreatedByUserId = table.Column<int>(type: "int", nullable: false),
            //        FirstUpdatedTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
            //        LastUpdatedTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
            //        FirstUpdatedByUserId = table.Column<int>(type: "int", nullable: true),
            //        LastUpdatedByUserId = table.Column<int>(type: "int", nullable: true),
            //        IsDeleted = table.Column<bool>(type: "bit", nullable: false),
            //        DeletedTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
            //        DeletedByUserId = table.Column<int>(type: "int", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_ContactUs", x => x.Id);
            //    });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropTable(
            //    name: "ContactUs");

            ////migrationBuilder.DropColumn(
            ////    name: "FcmToken",
            ////    table: "AppUser");
        }
    }
}
