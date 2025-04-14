using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MosefakApp.Infrastructure.Migrations.AppDb
{
    /// <inheritdoc />
    public partial class updateAppointment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //// 🔴 Step 1: Drop Old Foreign Key in Payments Table
            //migrationBuilder.DropForeignKey(
            //    name: "FK_Payments_Appointments_AppointmentId_AppointmentPatientId_AppointmentDoctorId",
            //    table: "Payments");

            //// 🔴 Step 2: Drop the Old Index that Uses AppointmentDoctorId
            //migrationBuilder.DropIndex(
            //    name: "IX_Payments_AppointmentId_AppointmentPatientId_AppointmentDoctorId",
            //    table: "Payments"
            //);

            //// 🔴 Step 3: Drop the Composite Primary Key in Appointments
            //migrationBuilder.DropPrimaryKey(
            //    name: "PK_Appointments",
            //    table: "Appointments"
            //);

            //// 🔴 Step 4: Drop Old Composite Key Columns in Payments Table
            //migrationBuilder.DropColumn(
            //    name: "AppointmentDoctorId",
            //    table: "Payments"
            //);

            //migrationBuilder.DropColumn(
            //    name: "AppointmentPatientId",
            //    table: "Payments"
            //);

            //// 🔴 Step 5: Set 'Id' as the New Primary Key in Appointments
            //migrationBuilder.AddPrimaryKey(
            //    name: "PK_Appointments",
            //    table: "Appointments",
            //    column: "Id"
            //);

            //// 🔴 Step 6: Create a New Index for 'AppointmentId' in Payments
            //migrationBuilder.CreateIndex(
            //    name: "IX_Payments_AppointmentId",
            //    table: "Payments",
            //    column: "AppointmentId",
            //    unique: false
            //);

            //// 🔴 Step 7: Recreate the Foreign Key for Payments → Appointments
            //migrationBuilder.AddForeignKey(
            //    name: "FK_Payments_Appointments_AppointmentId",
            //    table: "Payments",
            //    column: "AppointmentId",
            //    principalTable: "Appointments",
            //    principalColumn: "Id",
            //    onDelete: ReferentialAction.Cascade);
        }



        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //// 🔴 Step 1: Remove New Foreign Key
            //migrationBuilder.DropForeignKey(
            //    name: "FK_Payments_Appointments_AppointmentId",
            //    table: "Payments"
            //);

            //// 🔴 Step 2: Remove the New Index
            //migrationBuilder.DropIndex(
            //    name: "IX_Payments_AppointmentId",
            //    table: "Payments"
            //);

            //// 🔴 Step 3: Drop Primary Key (Id) and Restore Composite Key
            //migrationBuilder.DropPrimaryKey(
            //    name: "PK_Appointments",
            //    table: "Appointments"
            //);

            //migrationBuilder.AddColumn<int>(
            //    name: "AppointmentDoctorId",
            //    table: "Payments",
            //    type: "int",
            //    nullable: false,
            //    defaultValue: 0);

            //migrationBuilder.AddColumn<int>(
            //    name: "AppointmentPatientId",
            //    table: "Payments",
            //    type: "int",
            //    nullable: false,
            //    defaultValue: 0);

            //migrationBuilder.AddPrimaryKey(
            //    name: "PK_Appointments",
            //    table: "Appointments",
            //    columns: new[] { "DoctorId", "PatientId" });

            //// 🔴 Step 4: Restore the Old Index
            //migrationBuilder.CreateIndex(
            //    name: "IX_Payments_AppointmentId_AppointmentPatientId_AppointmentDoctorId",
            //    table: "Payments",
            //    columns: new[] { "AppointmentId", "AppointmentPatientId", "AppointmentDoctorId" },
            //    unique: true
            //);

            //// 🔴 Step 5: Restore the Old Foreign Key
            //migrationBuilder.AddForeignKey(
            //    name: "FK_Payments_Appointments_AppointmentId_AppointmentPatientId_AppointmentDoctorId",
            //    table: "Payments",
            //    columns: new[] { "AppointmentId", "AppointmentPatientId", "AppointmentDoctorId" },
            //    principalTable: "Appointments",
            //    principalColumns: new[] { "DoctorId", "PatientId" },
            //    onDelete: ReferentialAction.Restrict);
        }


    }
}
