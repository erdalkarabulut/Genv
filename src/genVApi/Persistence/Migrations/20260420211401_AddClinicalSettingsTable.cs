using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddClinicalSettingsTable : Migration
    {
        private static readonly Guid SingletonId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClinicalSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionCd34Cd3Divisor = table.Column<double>(type: "double precision", nullable: false),
                    DliCd3CalculationDivisor = table.Column<double>(type: "double precision", nullable: false),
                    TargetCd34AutologousPerKg = table.Column<double>(type: "double precision", nullable: false),
                    TargetCd34AllogeneicPerKg = table.Column<double>(type: "double precision", nullable: false),
                    IdealCd34AutologousPerKg = table.Column<double>(type: "double precision", nullable: false),
                    IdealCd34AllogeneicPerKg = table.Column<double>(type: "double precision", nullable: false),
                    MaxApheresisDaysAutologous = table.Column<int>(type: "integer", nullable: false),
                    MaxApheresisDaysAllogeneic = table.Column<int>(type: "integer", nullable: false),
                    DliHighDoseCd3PerKgThreshold = table.Column<double>(type: "double precision", nullable: false),
                    ProductMinimumCd34PerKg = table.Column<double>(type: "double precision", nullable: false),
                    DashboardCd34LowThreshold = table.Column<double>(type: "double precision", nullable: false),
                    DashboardCd34ElevatedFloor = table.Column<double>(type: "double precision", nullable: false),
                    DashboardCd3HighRiskThreshold = table.Column<double>(type: "double precision", nullable: false),
                    DashboardCd3LowImmuneThreshold = table.Column<double>(type: "double precision", nullable: false),
                    DashboardCd3OptimalMin = table.Column<double>(type: "double precision", nullable: false),
                    DashboardCd3OptimalMax = table.Column<double>(type: "double precision", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicalSettings", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "ClinicalSettings",
                columns: new[]
                {
                    "Id", "SessionCd34Cd3Divisor", "DliCd3CalculationDivisor",
                    "TargetCd34AutologousPerKg", "TargetCd34AllogeneicPerKg",
                    "IdealCd34AutologousPerKg", "IdealCd34AllogeneicPerKg",
                    "MaxApheresisDaysAutologous", "MaxApheresisDaysAllogeneic",
                    "DliHighDoseCd3PerKgThreshold", "ProductMinimumCd34PerKg",
                    "DashboardCd34LowThreshold", "DashboardCd34ElevatedFloor",
                    "DashboardCd3HighRiskThreshold", "DashboardCd3LowImmuneThreshold",
                    "DashboardCd3OptimalMin", "DashboardCd3OptimalMax",
                    "CreatedDate", "UpdatedDate", "DeletedDate"
                },
                values: new object[]
                {
                    SingletonId,
                    10000d,
                    10000d,
                    2d,
                    4d,
                    4d,
                    5d,
                    4,
                    2,
                    10d,
                    2d,
                    2d,
                    4d,
                    10d,
                    2d,
                    3d,
                    8d,
                    new DateTime(2026, 4, 20, 0, 0, 0, DateTimeKind.Utc),
                    null,
                    null
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClinicalSettings");
        }
    }
}
