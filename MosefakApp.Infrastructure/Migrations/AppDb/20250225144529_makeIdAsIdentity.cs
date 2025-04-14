using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MosefakApp.Infrastructure.Migrations.AppDb
{
    /// <inheritdoc />
    public partial class makeIdAsIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropPrimaryKey(name: "PK_Appointments", table: "Appointments");

            //migrationBuilder.AlterColumn<int>(
            //    name: "Id",
            //    table: "Appointments",
            //    nullable: false)
            //    .Annotation("SqlServer:Identity", "1, 1");

            //migrationBuilder.AddPrimaryKey(name: "PK_Appointments", table: "Appointments", column: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropPrimaryKey(name: "PK_Appointments", table: "Appointments");

            //migrationBuilder.AlterColumn<int>(
            //    name: "Id",
            //    table: "Appointments",
            //    nullable: false)
            //    .OldAnnotation("SqlServer:Identity", "1, 1");

            //migrationBuilder.AddPrimaryKey(name: "PK_Appointments", table: "Appointments", column: "Id");
        }

    }
}
