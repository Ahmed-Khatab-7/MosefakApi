using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MosefakApp.Infrastructure.Migrations.AppDb
{
    /// <inheritdoc />
    public partial class SomeOfActionsForAppointment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropColumn(
            //    name: "Address_State",
            //    table: "AppUser");

            //migrationBuilder.DropColumn(
            //    name: "Address_ZipCode",
            //    table: "AppUser");

            //migrationBuilder.AddColumn<Guid>(
            //    name: "TransactionId",
            //    table: "Payments",
            //    type: "uniqueidentifier",
            //    nullable: false,
            //    defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            //migrationBuilder.AlterColumn<string>(
            //    name: "Email",
            //    table: "AppUser",
            //    type: "nvarchar(450)",
            //    nullable: true,
            //    oldClrType: typeof(string),
            //    oldType: "nvarchar(max)",
            //    oldNullable: true);

            //migrationBuilder.AlterColumn<string>(
            //    name: "Address_Street",
            //    table: "AppUser",
            //    type: "nvarchar(max)",
            //    nullable: true,
            //    oldClrType: typeof(string),
            //    oldType: "nvarchar(max)");

            //migrationBuilder.AlterColumn<int>(
            //    name: "Address_Id",
            //    table: "AppUser",
            //    type: "int",
            //    nullable: true,
            //    oldClrType: typeof(int),
            //    oldType: "int");

            //migrationBuilder.AlterColumn<string>(
            //    name: "Address_City",
            //    table: "AppUser",
            //    type: "nvarchar(max)",
            //    nullable: true,
            //    oldClrType: typeof(string),
            //    oldType: "nvarchar(max)");

            //migrationBuilder.AddColumn<string>(
            //    name: "Address_Country",
            //    table: "AppUser",
            //    type: "nvarchar(max)",
            //    nullable: true);

            //migrationBuilder.AlterColumn<string>(
            //    name: "ProblemDescription",
            //    table: "Appointments",
            //    type: "nvarchar(max)",
            //    nullable: true,
            //    oldClrType: typeof(string),
            //    oldType: "nvarchar(max)");

            //migrationBuilder.CreateIndex(
            //    name: "IX_AppUser_Email",
            //    table: "AppUser",
            //    column: "Email",
            //    unique: true,
            //    filter: "[Email] IS NOT NULL");

            //migrationBuilder.Sql(@"
            //        ALTER TABLE Appointments
            //        ADD CONSTRAINT CHK_Appointment_ValidDates
            //        CHECK (StartDate < EndDate);
            //  ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropIndex(
            //    name: "IX_AppUser_Email",
            //    table: "AppUser");

            //migrationBuilder.DropColumn(
            //    name: "TransactionId",
            //    table: "Payments");

            //migrationBuilder.DropColumn(
            //    name: "Address_Country",
            //    table: "AppUser");

            //migrationBuilder.AlterColumn<string>(
            //    name: "Email",
            //    table: "AppUser",
            //    type: "nvarchar(max)",
            //    nullable: true,
            //    oldClrType: typeof(string),
            //    oldType: "nvarchar(450)",
            //    oldNullable: true);

            //migrationBuilder.AlterColumn<string>(
            //    name: "Address_Street",
            //    table: "AppUser",
            //    type: "nvarchar(max)",
            //    nullable: false,
            //    defaultValue: "",
            //    oldClrType: typeof(string),
            //    oldType: "nvarchar(max)",
            //    oldNullable: true);

            //migrationBuilder.AlterColumn<int>(
            //    name: "Address_Id",
            //    table: "AppUser",
            //    type: "int",
            //    nullable: false,
            //    defaultValue: 0,
            //    oldClrType: typeof(int),
            //    oldType: "int",
            //    oldNullable: true);

            //migrationBuilder.AlterColumn<string>(
            //    name: "Address_City",
            //    table: "AppUser",
            //    type: "nvarchar(max)",
            //    nullable: false,
            //    defaultValue: "",
            //    oldClrType: typeof(string),
            //    oldType: "nvarchar(max)",
            //    oldNullable: true);

            //migrationBuilder.AddColumn<string>(
            //    name: "Address_State",
            //    table: "AppUser",
            //    type: "nvarchar(max)",
            //    nullable: false,
            //    defaultValue: "");

            //migrationBuilder.AddColumn<int>(
            //    name: "Address_ZipCode",
            //    table: "AppUser",
            //    type: "int",
            //    nullable: false,
            //    defaultValue: 0);

            //migrationBuilder.AlterColumn<string>(
            //    name: "ProblemDescription",
            //    table: "Appointments",
            //    type: "nvarchar(max)",
            //    nullable: false,
            //    defaultValue: "",
            //    oldClrType: typeof(string),
            //    oldType: "nvarchar(max)",
            //    oldNullable: true);
        }
    }
}
