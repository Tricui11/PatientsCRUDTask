using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PatientApi.Migrations
{
    /// <inheritdoc />
    public partial class one_to_one_corrections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Patients_NameId",
                table: "Patients");

            migrationBuilder.CreateIndex(
                name: "IX_Patients_NameId",
                table: "Patients",
                column: "NameId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Patients_NameId",
                table: "Patients");

            migrationBuilder.CreateIndex(
                name: "IX_Patients_NameId",
                table: "Patients",
                column: "NameId");
        }
    }
}
