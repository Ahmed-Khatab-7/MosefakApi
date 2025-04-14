using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MosefakApp.Infrastructure.Migrations.AppDb
{
    /// <inheritdoc />
    public partial class AddBusinessDomains : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.CreateTable(
            //    name: "AppUser",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        FirstName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
            //        LastName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
            //        Gender = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        Address_Id = table.Column<int>(type: "int", nullable: false),
            //        Address_State = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Address_City = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Address_Street = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Address_ZipCode = table.Column<int>(type: "int", nullable: false),
            //        DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
            //        ImagePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        IsDeleted = table.Column<bool>(type: "bit", nullable: false),
            //        IsDisabled = table.Column<bool>(type: "bit", nullable: false),
            //        UserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        NormalizedUserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        NormalizedEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
            //        PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
            //        TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
            //        LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
            //        LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
            //        AccessFailedCount = table.Column<int>(type: "int", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_AppUser", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Doctors",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        AppUserId = table.Column<int>(type: "int", nullable: false),
            //        YearOfExperience = table.Column<int>(type: "int", nullable: false),
            //        LicenseNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        AboutMe = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        NumberOfReviews = table.Column<int>(type: "int", nullable: false),
            //        ConsultationFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
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
            //        table.PrimaryKey("PK_Doctors", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Appointments",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false),
            //        DoctorId = table.Column<int>(type: "int", nullable: false),
            //        AppUserId = table.Column<int>(type: "int", nullable: false),
            //        StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        ProblemDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        AppointmentStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        CancellationReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        IsPaid = table.Column<bool>(type: "bit", nullable: false),
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
            //        table.PrimaryKey("PK_Appointments", x => new { x.Id, x.AppUserId, x.DoctorId });
            //        table.ForeignKey(
            //            name: "FK_Appointments_Doctors_DoctorId",
            //            column: x => x.DoctorId,
            //            principalTable: "Doctors",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "ClinicAddresses",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        Street = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        City = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Country = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
            //        table.PrimaryKey("PK_ClinicAddresses", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_ClinicAddresses_Doctors_DoctorId",
            //            column: x => x.DoctorId,
            //            principalTable: "Doctors",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Reviews",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        Rate = table.Column<int>(type: "int", nullable: false),
            //        Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        AppUserId = table.Column<int>(type: "int", nullable: false),
            //        DoctorId = table.Column<int>(type: "int", nullable: true),
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
            //        table.PrimaryKey("PK_Reviews", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_Reviews_Doctors_DoctorId",
            //            column: x => x.DoctorId,
            //            principalTable: "Doctors",
            //            principalColumn: "Id");
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Specializations",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        Name = table.Column<int>(type: "int", nullable: false),
            //        Category = table.Column<int>(type: "int", nullable: false),
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
            //        table.PrimaryKey("PK_Specializations", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_Specializations_Doctors_DoctorId",
            //            column: x => x.DoctorId,
            //            principalTable: "Doctors",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "WorkingTimes",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        DoctorId = table.Column<int>(type: "int", nullable: false),
            //        DayOfWeek = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        StartTime = table.Column<TimeOnly>(type: "TIME", nullable: false),
            //        EndTime = table.Column<TimeOnly>(type: "TIME", nullable: false),
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
            //        table.PrimaryKey("PK_WorkingTimes", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_WorkingTimes_Doctors_DoctorId",
            //            column: x => x.DoctorId,
            //            principalTable: "Doctors",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Payments",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        Currency = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        PaymentMethod = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        TransactionId = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        IsSuccessful = table.Column<bool>(type: "bit", nullable: false),
            //        PaymentIntentId = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        ClientSecret = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        AppointmentId = table.Column<int>(type: "int", nullable: false),
            //        AppointmentAppUserId = table.Column<int>(type: "int", nullable: false),
            //        AppointmentDoctorId = table.Column<int>(type: "int", nullable: false),
            //        PatientId = table.Column<int>(type: "int", nullable: false),
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
            //        table.PrimaryKey("PK_Payments", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_Payments_Appointments_AppointmentId_AppointmentAppUserId_AppointmentDoctorId",
            //            columns: x => new { x.AppointmentId, x.AppointmentAppUserId, x.AppointmentDoctorId },
            //            principalTable: "Appointments",
            //            principalColumns: new[] { "Id", "AppUserId", "DoctorId" },
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateIndex(
            //    name: "IX_Appointments_DoctorId_StartDate_EndDate",
            //    table: "Appointments",
            //    columns: new[] { "DoctorId", "StartDate", "EndDate" },
            //    unique: true);

            //migrationBuilder.CreateIndex(
            //    name: "IX_ClinicAddresses_DoctorId",
            //    table: "ClinicAddresses",
            //    column: "DoctorId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Payments_AppointmentId_AppointmentAppUserId_AppointmentDoctorId",
            //    table: "Payments",
            //    columns: new[] { "AppointmentId", "AppointmentAppUserId", "AppointmentDoctorId" },
            //    unique: true);

            //migrationBuilder.CreateIndex(
            //    name: "IX_Reviews_DoctorId",
            //    table: "Reviews",
            //    column: "DoctorId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Specializations_DoctorId",
            //    table: "Specializations",
            //    column: "DoctorId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_WorkingTimes_DoctorId",
            //    table: "WorkingTimes",
            //    column: "DoctorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropTable(
            //    name: "AppUser");

            //migrationBuilder.DropTable(
            //    name: "ClinicAddresses");

            //migrationBuilder.DropTable(
            //    name: "Payments");

            //migrationBuilder.DropTable(
            //    name: "Reviews");

            //migrationBuilder.DropTable(
            //    name: "Specializations");

            //migrationBuilder.DropTable(
            //    name: "WorkingTimes");

            //migrationBuilder.DropTable(
            //    name: "Appointments");

            //migrationBuilder.DropTable(
            //    name: "Doctors");
        }
    }
}
