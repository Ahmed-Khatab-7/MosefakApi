using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MosefakApp.Infrastructure.Migrations.AppDb
{
    /// <inheritdoc />
    public partial class AddAppointmentDomainModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropForeignKey(
            //    name: "FK_Payments_Appointments_AppointmentId_AppointmentAppUserId_AppointmentDoctorId",
            //    table: "Payments");

            //migrationBuilder.DropForeignKey(
            //    name: "FK_WorkingTimes_Doctors_DoctorId",
            //    table: "WorkingTimes");

            //migrationBuilder.DropTable(
            //    name: "ClinicAddresses");

            //migrationBuilder.DropColumn(
            //    name: "EndTime",
            //    table: "WorkingTimes");

            //migrationBuilder.DropColumn(
            //    name: "StartTime",
            //    table: "WorkingTimes");

            //migrationBuilder.DropColumn(
            //    name: "ClientSecret",
            //    table: "Payments");

            //migrationBuilder.DropColumn(
            //    name: "Currency",
            //    table: "Payments");

            //migrationBuilder.DropColumn(
            //    name: "IsSuccessful",
            //    table: "Payments");

            //migrationBuilder.DropColumn(
            //    name: "PaymentDate",
            //    table: "Payments");

            //migrationBuilder.DropColumn(
            //    name: "PaymentIntentId",
            //    table: "Payments");

            //migrationBuilder.DropColumn(
            //    name: "PaymentMethod",
            //    table: "Payments");

            //migrationBuilder.DropColumn(
            //    name: "YearOfExperience",
            //    table: "Doctors");

            //migrationBuilder.RenameColumn(
            //    name: "DoctorId",
            //    table: "WorkingTimes",
            //    newName: "ClinicId");

            //migrationBuilder.RenameColumn(
            //    name: "DayOfWeek",
            //    table: "WorkingTimes",
            //    newName: "Day");

            //migrationBuilder.RenameIndex(
            //    name: "IX_WorkingTimes_DoctorId",
            //    table: "WorkingTimes",
            //    newName: "IX_WorkingTimes_ClinicId");

            //migrationBuilder.RenameColumn(
            //    name: "TransactionId",
            //    table: "Payments",
            //    newName: "StripePaymentIntentId");

            //migrationBuilder.RenameColumn(
            //    name: "PatientId",
            //    table: "Payments",
            //    newName: "Status");

            //migrationBuilder.RenameColumn(
            //    name: "AppointmentAppUserId",
            //    table: "Payments",
            //    newName: "AppointmentPatientId");

            //migrationBuilder.RenameIndex(
            //    name: "IX_Payments_AppointmentId_AppointmentAppUserId_AppointmentDoctorId",
            //    table: "Payments",
            //    newName: "IX_Payments_AppointmentId_AppointmentPatientId_AppointmentDoctorId");

            //migrationBuilder.RenameColumn(
            //    name: "AppUserId",
            //    table: "Appointments",
            //    newName: "PatientId");

            //migrationBuilder.AddColumn<bool>(
            //    name: "ApprovedByDoctor",
            //    table: "Appointments",
            //    type: "bit",
            //    nullable: false,
            //    defaultValue: false);

            //migrationBuilder.AddColumn<DateTime>(
            //    name: "CancelledAt",
            //    table: "Appointments",
            //    type: "datetime2",
            //    nullable: true);

            //migrationBuilder.AddColumn<DateTime>(
            //    name: "CompletedAt",
            //    table: "Appointments",
            //    type: "datetime2",
            //    nullable: true);

            //migrationBuilder.AddColumn<DateTime>(
            //    name: "ConfirmedAt",
            //    table: "Appointments",
            //    type: "datetime2",
            //    nullable: true);

            //migrationBuilder.AddColumn<DateTime>(
            //    name: "PaymentDueTime",
            //    table: "Appointments",
            //    type: "datetime2",
            //    nullable: true);

            //migrationBuilder.AddColumn<bool>(
            //    name: "ServiceProvided",
            //    table: "Appointments",
            //    type: "bit",
            //    nullable: false,
            //    defaultValue: false);

            //migrationBuilder.CreateTable(
            //    name: "Awards",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        DateReceived = table.Column<DateOnly>(type: "date", nullable: false),
            //        Organization = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
            //        table.PrimaryKey("PK_Awards", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_Awards_Doctors_DoctorId",
            //            column: x => x.DoctorId,
            //            principalTable: "Doctors",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Restrict);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Clinics",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Street = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        City = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Country = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        ApartmentOrSuite = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Landmark = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        LogoPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        ClinicImage = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
            //        table.PrimaryKey("PK_Clinics", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_Clinics_Doctors_DoctorId",
            //            column: x => x.DoctorId,
            //            principalTable: "Doctors",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Restrict);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Educations",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        Degree = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Major = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        UniversityName = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        UniversityLogoPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        Location = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        StartDate = table.Column<DateOnly>(type: "date", nullable: false),
            //        EndDate = table.Column<DateOnly>(type: "date", nullable: true),
            //        CurrentlyStudying = table.Column<bool>(type: "bit", nullable: false),
            //        AdditionalNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
            //        table.PrimaryKey("PK_Educations", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_Educations_Doctors_DoctorId",
            //            column: x => x.DoctorId,
            //            principalTable: "Doctors",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Restrict);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Experiences",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        HospitalLogo = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        HospitalName = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Location = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        EmploymentType = table.Column<int>(type: "int", nullable: false),
            //        JobDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        StartDate = table.Column<DateOnly>(type: "date", nullable: false),
            //        EndDate = table.Column<DateOnly>(type: "date", nullable: true),
            //        CurrentlyWorkingHere = table.Column<bool>(type: "bit", nullable: false),
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
            //        table.PrimaryKey("PK_Experiences", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_Experiences_Doctors_DoctorId",
            //            column: x => x.DoctorId,
            //            principalTable: "Doctors",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Restrict);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Periods",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        StartTime = table.Column<TimeOnly>(type: "TIME", nullable: false),
            //        EndTime = table.Column<TimeOnly>(type: "TIME", nullable: false),
            //        WorkingTimeId = table.Column<int>(type: "int", nullable: false),
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
            //        table.PrimaryKey("PK_Periods", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_Periods_WorkingTimes_WorkingTimeId",
            //            column: x => x.WorkingTimeId,
            //            principalTable: "WorkingTimes",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Restrict);
            //    });

            //migrationBuilder.CreateIndex(
            //    name: "IX_Awards_DoctorId",
            //    table: "Awards",
            //    column: "DoctorId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Clinics_DoctorId",
            //    table: "Clinics",
            //    column: "DoctorId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Educations_DoctorId",
            //    table: "Educations",
            //    column: "DoctorId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Experiences_DoctorId",
            //    table: "Experiences",
            //    column: "DoctorId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Periods_WorkingTimeId",
            //    table: "Periods",
            //    column: "WorkingTimeId");

            //migrationBuilder.AddForeignKey(
            //    name: "FK_Payments_Appointments_AppointmentId_AppointmentPatientId_AppointmentDoctorId",
            //    table: "Payments",
            //    columns: new[] { "AppointmentId", "AppointmentPatientId", "AppointmentDoctorId" },
            //    principalTable: "Appointments",
            //    principalColumns: new[] { "Id", "PatientId", "DoctorId" },
            //    onDelete: ReferentialAction.Restrict);

            //migrationBuilder.AddForeignKey(
            //    name: "FK_WorkingTimes_Clinics_ClinicId",
            //    table: "WorkingTimes",
            //    column: "ClinicId",
            //    principalTable: "Clinics",
            //    principalColumn: "Id",
            //    onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropForeignKey(
            //    name: "FK_Payments_Appointments_AppointmentId_AppointmentPatientId_AppointmentDoctorId",
            //    table: "Payments");

            //migrationBuilder.DropForeignKey(
            //    name: "FK_WorkingTimes_Clinics_ClinicId",
            //    table: "WorkingTimes");

            //migrationBuilder.DropTable(
            //    name: "Awards");

            //migrationBuilder.DropTable(
            //    name: "Clinics");

            //migrationBuilder.DropTable(
            //    name: "Educations");

            //migrationBuilder.DropTable(
            //    name: "Experiences");

            //migrationBuilder.DropTable(
            //    name: "Periods");

            //migrationBuilder.DropColumn(
            //    name: "ApprovedByDoctor",
            //    table: "Appointments");

            //migrationBuilder.DropColumn(
            //    name: "CancelledAt",
            //    table: "Appointments");

            //migrationBuilder.DropColumn(
            //    name: "CompletedAt",
            //    table: "Appointments");

            //migrationBuilder.DropColumn(
            //    name: "ConfirmedAt",
            //    table: "Appointments");

            //migrationBuilder.DropColumn(
            //    name: "PaymentDueTime",
            //    table: "Appointments");

            //migrationBuilder.DropColumn(
            //    name: "ServiceProvided",
            //    table: "Appointments");

            //migrationBuilder.RenameColumn(
            //    name: "Day",
            //    table: "WorkingTimes",
            //    newName: "DayOfWeek");

            //migrationBuilder.RenameColumn(
            //    name: "ClinicId",
            //    table: "WorkingTimes",
            //    newName: "DoctorId");

            //migrationBuilder.RenameIndex(
            //    name: "IX_WorkingTimes_ClinicId",
            //    table: "WorkingTimes",
            //    newName: "IX_WorkingTimes_DoctorId");

            //migrationBuilder.RenameColumn(
            //    name: "StripePaymentIntentId",
            //    table: "Payments",
            //    newName: "TransactionId");

            //migrationBuilder.RenameColumn(
            //    name: "Status",
            //    table: "Payments",
            //    newName: "PatientId");

            //migrationBuilder.RenameColumn(
            //    name: "AppointmentPatientId",
            //    table: "Payments",
            //    newName: "AppointmentAppUserId");

            //migrationBuilder.RenameIndex(
            //    name: "IX_Payments_AppointmentId_AppointmentPatientId_AppointmentDoctorId",
            //    table: "Payments",
            //    newName: "IX_Payments_AppointmentId_AppointmentAppUserId_AppointmentDoctorId");

            //migrationBuilder.RenameColumn(
            //    name: "PatientId",
            //    table: "Appointments",
            //    newName: "AppUserId");

            //migrationBuilder.AddColumn<TimeOnly>(
            //    name: "EndTime",
            //    table: "WorkingTimes",
            //    type: "TIME",
            //    nullable: false,
            //    defaultValue: new TimeOnly(0, 0, 0));

            //migrationBuilder.AddColumn<TimeOnly>(
            //    name: "StartTime",
            //    table: "WorkingTimes",
            //    type: "TIME",
            //    nullable: false,
            //    defaultValue: new TimeOnly(0, 0, 0));

            //migrationBuilder.AddColumn<string>(
            //    name: "ClientSecret",
            //    table: "Payments",
            //    type: "nvarchar(max)",
            //    nullable: false,
            //    defaultValue: "");

            //migrationBuilder.AddColumn<string>(
            //    name: "Currency",
            //    table: "Payments",
            //    type: "nvarchar(max)",
            //    nullable: false,
            //    defaultValue: "");

            //migrationBuilder.AddColumn<bool>(
            //    name: "IsSuccessful",
            //    table: "Payments",
            //    type: "bit",
            //    nullable: false,
            //    defaultValue: false);

            //migrationBuilder.AddColumn<DateTime>(
            //    name: "PaymentDate",
            //    table: "Payments",
            //    type: "datetime2",
            //    nullable: false,
            //    defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            //migrationBuilder.AddColumn<string>(
            //    name: "PaymentIntentId",
            //    table: "Payments",
            //    type: "nvarchar(max)",
            //    nullable: false,
            //    defaultValue: "");

            //migrationBuilder.AddColumn<string>(
            //    name: "PaymentMethod",
            //    table: "Payments",
            //    type: "nvarchar(max)",
            //    nullable: false,
            //    defaultValue: "");

            //migrationBuilder.AddColumn<int>(
            //    name: "YearOfExperience",
            //    table: "Doctors",
            //    type: "int",
            //    nullable: false,
            //    defaultValue: 0);

            //migrationBuilder.CreateTable(
            //    name: "ClinicAddresses",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        DoctorId = table.Column<int>(type: "int", nullable: false),
            //        City = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Country = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        CreatedByUserId = table.Column<int>(type: "int", nullable: false),
            //        DeletedByUserId = table.Column<int>(type: "int", nullable: true),
            //        DeletedTime = table.Column<DateTime>(type: "datetime2", nullable: true),
            //        FirstUpdatedByUserId = table.Column<int>(type: "int", nullable: true),
            //        FirstUpdatedTime = table.Column<DateTime>(type: "datetime2", nullable: true),
            //        IsDeleted = table.Column<bool>(type: "bit", nullable: false),
            //        LastUpdatedByUserId = table.Column<int>(type: "int", nullable: true),
            //        LastUpdatedTime = table.Column<DateTime>(type: "datetime2", nullable: true),
            //        Street = table.Column<string>(type: "nvarchar(max)", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_ClinicAddresses", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_ClinicAddresses_Doctors_DoctorId",
            //            column: x => x.DoctorId,
            //            principalTable: "Doctors",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Restrict);
            //    });

            //migrationBuilder.CreateIndex(
            //    name: "IX_ClinicAddresses_DoctorId",
            //    table: "ClinicAddresses",
            //    column: "DoctorId");

            //migrationBuilder.AddForeignKey(
            //    name: "FK_Payments_Appointments_AppointmentId_AppointmentAppUserId_AppointmentDoctorId",
            //    table: "Payments",
            //    columns: new[] { "AppointmentId", "AppointmentAppUserId", "AppointmentDoctorId" },
            //    principalTable: "Appointments",
            //    principalColumns: new[] { "Id", "AppUserId", "DoctorId" },
            //    onDelete: ReferentialAction.Restrict);

            //migrationBuilder.AddForeignKey(
            //    name: "FK_WorkingTimes_Doctors_DoctorId",
            //    table: "WorkingTimes",
            //    column: "DoctorId",
            //    principalTable: "Doctors",
            //    principalColumn: "Id",
            //    onDelete: ReferentialAction.Restrict);
        }
    }
}
