using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MosefakApp.Infrastructure.Migrations.AppDb
{
    /// <inheritdoc />
    public partial class addBookingAndSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropForeignKey(
            //    name: "FK_Appointments_Doctors_DoctorId",
            //    table: "Appointments");

            //migrationBuilder.DropForeignKey(
            //    name: "FK_ClinicAddresses_Doctors_DoctorId",
            //    table: "ClinicAddresses");

            //migrationBuilder.DropForeignKey(
            //    name: "FK_Payments_Appointments_AppointmentId_AppointmentAppUserId_AppointmentDoctorId",
            //    table: "Payments");

            //migrationBuilder.DropForeignKey(
            //    name: "FK_Specializations_Doctors_DoctorId",
            //    table: "Specializations");

            //migrationBuilder.DropForeignKey(
            //    name: "FK_WorkingTimes_Doctors_DoctorId",
            //    table: "WorkingTimes");

            //migrationBuilder.DropColumn(
            //    name: "ConsultationFee",
            //    table: "Doctors");

            //migrationBuilder.DropColumn(
            //    name: "AppointmentType",
            //    table: "Appointments");

            //migrationBuilder.AddColumn<int>(
            //    name: "AppointmentTypeId",
            //    table: "Appointments",
            //    type: "int",
            //    nullable: false,
            //    defaultValue: 0);

            //migrationBuilder.CreateTable(
            //    name: "AppointmentTypes",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        Duration = table.Column<TimeOnly>(type: "time", nullable: false),
            //        VisitType = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        ConsultationFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        DoctorId = table.Column<int>(type: "int", nullable: false),
            //        CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        CreatedByUserId = table.Column<int>(type: "int", nullable: false),
            //        FirstUpdatedTime = table.Column<DateTime>(type: "datetime2", nullable: true),
            //        LastUpdatedTime = table.Column<DateTime>(type: "datetime2", nullable: true),
            //        FirstUpdatedByUserId = table.Column<int>(type: "int", nullable: true),
            //        LastUpdatedByUserId = table.Column<int>(type: "int", nullable: true),
            //        IsDeleted = table.Column<bool>(type: "bit", nullable: false),
            //        DeletedTime = table.Column<DateTime>(type: "datetime2", nullable: true),
            //        DeletedByUserId = table.Column<int>(type: "int", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_AppointmentTypes", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_AppointmentTypes_Doctors_DoctorId",
            //            column: x => x.DoctorId,
            //            principalTable: "Doctors",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Restrict);
            //    });

            //migrationBuilder.CreateIndex(
            //    name: "IX_Appointments_AppointmentTypeId",
            //    table: "Appointments",
            //    column: "AppointmentTypeId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_AppointmentTypes_DoctorId",
            //    table: "AppointmentTypes",
            //    column: "DoctorId");

            //migrationBuilder.AddForeignKey(
            //    name: "FK_Appointments_AppointmentTypes_AppointmentTypeId",
            //    table: "Appointments",
            //    column: "AppointmentTypeId",
            //    principalTable: "AppointmentTypes",
            //    principalColumn: "Id",
            //    onDelete: ReferentialAction.Restrict);

            //migrationBuilder.AddForeignKey(
            //    name: "FK_Appointments_Doctors_DoctorId",
            //    table: "Appointments",
            //    column: "DoctorId",
            //    principalTable: "Doctors",
            //    principalColumn: "Id",
            //    onDelete: ReferentialAction.Restrict);

            //migrationBuilder.AddForeignKey(
            //    name: "FK_ClinicAddresses_Doctors_DoctorId",
            //    table: "ClinicAddresses",
            //    column: "DoctorId",
            //    principalTable: "Doctors",
            //    principalColumn: "Id",
            //    onDelete: ReferentialAction.Restrict);

            //migrationBuilder.AddForeignKey(
            //    name: "FK_Payments_Appointments_AppointmentId_AppointmentAppUserId_AppointmentDoctorId",
            //    table: "Payments",
            //    columns: new[] { "AppointmentId", "AppointmentAppUserId", "AppointmentDoctorId" },
            //    principalTable: "Appointments",
            //    principalColumns: new[] { "Id", "AppUserId", "DoctorId" },
            //    onDelete: ReferentialAction.Restrict);

            //migrationBuilder.AddForeignKey(
            //    name: "FK_Specializations_Doctors_DoctorId",
            //    table: "Specializations",
            //    column: "DoctorId",
            //    principalTable: "Doctors",
            //    principalColumn: "Id",
            //    onDelete: ReferentialAction.Restrict);

            //migrationBuilder.AddForeignKey(
            //    name: "FK_WorkingTimes_Doctors_DoctorId",
            //    table: "WorkingTimes",
            //    column: "DoctorId",
            //    principalTable: "Doctors",
            //    principalColumn: "Id",
            //    onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropForeignKey(
            //    name: "FK_Appointments_AppointmentTypes_AppointmentTypeId",
            //    table: "Appointments");

            //migrationBuilder.DropForeignKey(
            //    name: "FK_Appointments_Doctors_DoctorId",
            //    table: "Appointments");

            //migrationBuilder.DropForeignKey(
            //    name: "FK_ClinicAddresses_Doctors_DoctorId",
            //    table: "ClinicAddresses");

            //migrationBuilder.DropForeignKey(
            //    name: "FK_Payments_Appointments_AppointmentId_AppointmentAppUserId_AppointmentDoctorId",
            //    table: "Payments");

            //migrationBuilder.DropForeignKey(
            //    name: "FK_Specializations_Doctors_DoctorId",
            //    table: "Specializations");

            //migrationBuilder.DropForeignKey(
            //    name: "FK_WorkingTimes_Doctors_DoctorId",
            //    table: "WorkingTimes");

            //migrationBuilder.DropTable(
            //    name: "AppointmentTypes");

            //migrationBuilder.DropIndex(
            //    name: "IX_Appointments_AppointmentTypeId",
            //    table: "Appointments");

            //migrationBuilder.DropColumn(
            //    name: "AppointmentTypeId",
            //    table: "Appointments");

            //migrationBuilder.AddColumn<decimal>(
            //    name: "ConsultationFee",
            //    table: "Doctors",
            //    type: "decimal(18,2)",
            //    nullable: false,
            //    defaultValue: 0m);

            //migrationBuilder.AddColumn<string>(
            //    name: "AppointmentType",
            //    table: "Appointments",
            //    type: "nvarchar(max)",
            //    nullable: false,
            //    defaultValue: "");

            //migrationBuilder.AddForeignKey(
            //    name: "FK_Appointments_Doctors_DoctorId",
            //    table: "Appointments",
            //    column: "DoctorId",
            //    principalTable: "Doctors",
            //    principalColumn: "Id",
            //    onDelete: ReferentialAction.Cascade);

            //migrationBuilder.AddForeignKey(
            //    name: "FK_ClinicAddresses_Doctors_DoctorId",
            //    table: "ClinicAddresses",
            //    column: "DoctorId",
            //    principalTable: "Doctors",
            //    principalColumn: "Id",
            //    onDelete: ReferentialAction.Cascade);

            //migrationBuilder.AddForeignKey(
            //    name: "FK_Payments_Appointments_AppointmentId_AppointmentAppUserId_AppointmentDoctorId",
            //    table: "Payments",
            //    columns: new[] { "AppointmentId", "AppointmentAppUserId", "AppointmentDoctorId" },
            //    principalTable: "Appointments",
            //    principalColumns: new[] { "Id", "AppUserId", "DoctorId" },
            //    onDelete: ReferentialAction.Cascade);

            //migrationBuilder.AddForeignKey(
            //    name: "FK_Specializations_Doctors_DoctorId",
            //    table: "Specializations",
            //    column: "DoctorId",
            //    principalTable: "Doctors",
            //    principalColumn: "Id",
            //    onDelete: ReferentialAction.Cascade);

            //migrationBuilder.AddForeignKey(
            //    name: "FK_WorkingTimes_Doctors_DoctorId",
            //    table: "WorkingTimes",
            //    column: "DoctorId",
            //    principalTable: "Doctors",
            //    principalColumn: "Id",
            //    onDelete: ReferentialAction.Cascade);
        }
    }
}
