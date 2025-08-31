using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedanoClinicBE.Migrations
{
    /// <inheritdoc />
    public partial class CreateAppointmentHoursTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppointmentHours",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DoctorId = table.Column<int>(type: "int", nullable: false),
                    Hour = table.Column<TimeSpan>(type: "time", nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppointmentHours", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppointmentHours_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentHours_DoctorId",
                table: "AppointmentHours",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentHours_DoctorId_Hour_DayOfWeek",
                table: "AppointmentHours",
                columns: new[] { "DoctorId", "Hour", "DayOfWeek" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppointmentHours");
        }
    }
}