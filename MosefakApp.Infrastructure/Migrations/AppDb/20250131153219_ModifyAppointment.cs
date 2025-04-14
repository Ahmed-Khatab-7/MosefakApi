using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MosefakApp.Infrastructure.Migrations.AppDb
{
    /// <inheritdoc />
    public partial class ModifyAppointment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropColumn(
            //    name: "IsPaid",
            //    table: "Appointments");

            //migrationBuilder.AddColumn<string>(
            //    name: "PaymentStatus",
            //    table: "Appointments",
            //    type: "nvarchar(max)",
            //    nullable: false,
            //    defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropColumn(
            //    name: "PaymentStatus",
            //    table: "Appointments");

            //migrationBuilder.AddColumn<bool>(
            //    name: "IsPaid",
            //    table: "Appointments",
            //    type: "bit",
            //    nullable: false,
            //    defaultValue: false);
        }
    }
}
