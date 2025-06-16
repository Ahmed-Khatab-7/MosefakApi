using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MosefakApp.Infrastructure.Migrations.AppDb
{
    /// <inheritdoc />
    public partial class AddMissingFieldsToPaymentEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Appointments_DoctorId_StartDate_EndDate",
                table: "Appointments");

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "PaymentMethod",
                table: "Payments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            //migrationBuilder.AddColumn<string>(
            //    name: "FirebaseUid",
            //    table: "AppUser",
            //    type: "nvarchar(max)",
            //    nullable: true);

            //migrationBuilder.AddColumn<int>(
            //    name: "VerificationAttempts",
            //    table: "AppUser",
            //    type: "int",
            //    nullable: false,
            //    defaultValue: 0);

            //migrationBuilder.AddColumn<string>(
            //    name: "VerificationCode",
            //    table: "AppUser",
            //    type: "nvarchar(max)",
            //    nullable: true);

            //migrationBuilder.AddColumn<DateTime>(
            //    name: "VerificationCodeExpiry",
            //    table: "AppUser",
            //    type: "datetime2",
            //    nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_DoctorId_StartDate_EndDate",
                table: "Appointments",
                columns: new[] { "DoctorId", "StartDate", "EndDate" },
                unique: true,
                filter: "[AppointmentStatus] != 'CanceledByDoctor' AND [AppointmentStatus] != 'CanceledByPatient'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Appointments_DoctorId_StartDate_EndDate",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "Payments");

            //migrationBuilder.DropColumn(
            //    name: "FirebaseUid",
            //    table: "AppUser");

            //migrationBuilder.DropColumn(
            //    name: "VerificationAttempts",
            //    table: "AppUser");

            //migrationBuilder.DropColumn(
            //    name: "VerificationCode",
            //    table: "AppUser");

            //migrationBuilder.DropColumn(
            //    name: "VerificationCodeExpiry",
            //    table: "AppUser");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_DoctorId_StartDate_EndDate",
                table: "Appointments",
                columns: new[] { "DoctorId", "StartDate", "EndDate" },
                unique: true);
        }
    }
}
