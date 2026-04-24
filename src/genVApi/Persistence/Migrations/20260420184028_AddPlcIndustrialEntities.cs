using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPlcIndustrialEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserOperationClaims",
                keyColumn: "Id",
                keyValue: new Guid("1e8c0269-1aee-4041-be81-c6d28d3b2eb2"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("baefffe2-6b04-415f-a333-93cfd03021f2"));

            migrationBuilder.CreateTable(
                name: "PlcAlarmContacts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DevicePrefix = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    DisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Phone = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    SmsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    EmailEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlcAlarmContacts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlcSensorPoints",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SensorCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    DeviceName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DevicePrefix = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    DataLabel = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ModbusHost = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ModbusPort = table.Column<int>(type: "integer", nullable: false),
                    SlaveId = table.Column<int>(type: "integer", nullable: false),
                    RegisterAddress = table.Column<int>(type: "integer", nullable: false),
                    RegisterLength = table.Column<int>(type: "integer", nullable: false),
                    ScaleDivisor = table.Column<double>(type: "double precision", nullable: false),
                    PollIntervalSeconds = table.Column<int>(type: "integer", nullable: false),
                    AlarmLow = table.Column<double>(type: "double precision", nullable: true),
                    AlarmHigh = table.Column<double>(type: "double precision", nullable: true),
                    AlarmActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlcSensorPoints", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlcTelemetryReadings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SensorPointId = table.Column<Guid>(type: "uuid", nullable: false),
                    Value = table.Column<double>(type: "double precision", nullable: false),
                    ReadAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RawRegisterValue = table.Column<int>(type: "integer", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlcTelemetryReadings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlcTelemetryReadings_PlcSensorPoints_SensorPointId",
                        column: x => x.SensorPointId,
                        principalTable: "PlcSensorPoints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AuthenticatorType", "CreatedDate", "DeletedDate", "Email", "PasswordHash", "PasswordSalt", "UpdatedDate" },
                values: new object[] { new Guid("b8e3fbb5-e99c-4d74-8d82-447d0e8734c3"), 0, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "narch@kodlama.io", new byte[] { 234, 194, 71, 72, 59, 52, 31, 41, 42, 151, 137, 4, 65, 135, 60, 2, 224, 89, 55, 246, 124, 68, 72, 125, 179, 38, 221, 146, 18, 219, 129, 139, 2, 79, 46, 4, 130, 52, 212, 249, 76, 22, 210, 71, 49, 113, 137, 99, 2, 61, 74, 182, 203, 94, 252, 54, 251, 37, 116, 210, 128, 197, 133, 32 }, new byte[] { 238, 185, 227, 106, 215, 192, 153, 7, 40, 252, 225, 96, 58, 36, 64, 50, 134, 49, 39, 199, 105, 242, 147, 43, 90, 186, 22, 182, 76, 250, 198, 136, 163, 234, 172, 212, 132, 168, 199, 58, 38, 20, 22, 196, 236, 171, 29, 230, 167, 179, 253, 123, 47, 235, 253, 70, 154, 115, 190, 14, 41, 36, 154, 75, 217, 152, 99, 115, 59, 47, 96, 172, 199, 156, 33, 244, 24, 52, 37, 69, 138, 109, 194, 59, 155, 196, 176, 97, 126, 13, 239, 229, 218, 0, 91, 3, 20, 109, 49, 12, 124, 6, 55, 233, 35, 71, 165, 89, 43, 213, 254, 22, 75, 105, 188, 211, 92, 209, 5, 120, 184, 158, 244, 175, 45, 21, 167, 182 }, null });

            migrationBuilder.InsertData(
                table: "UserOperationClaims",
                columns: new[] { "Id", "CreatedDate", "DeletedDate", "OperationClaimId", "UpdatedDate", "UserId" },
                values: new object[] { new Guid("12688ce4-9bd8-4d7c-84ff-150d953a4760"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, null, new Guid("b8e3fbb5-e99c-4d74-8d82-447d0e8734c3") });

            migrationBuilder.CreateIndex(
                name: "IX_PlcSensorPoints_SensorCode",
                table: "PlcSensorPoints",
                column: "SensorCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlcTelemetryReadings_SensorPointId_ReadAtUtc",
                table: "PlcTelemetryReadings",
                columns: new[] { "SensorPointId", "ReadAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlcAlarmContacts");

            migrationBuilder.DropTable(
                name: "PlcTelemetryReadings");

            migrationBuilder.DropTable(
                name: "PlcSensorPoints");

            migrationBuilder.DeleteData(
                table: "UserOperationClaims",
                keyColumn: "Id",
                keyValue: new Guid("12688ce4-9bd8-4d7c-84ff-150d953a4760"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("b8e3fbb5-e99c-4d74-8d82-447d0e8734c3"));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AuthenticatorType", "CreatedDate", "DeletedDate", "Email", "PasswordHash", "PasswordSalt", "UpdatedDate" },
                values: new object[] { new Guid("baefffe2-6b04-415f-a333-93cfd03021f2"), 0, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "narch@kodlama.io", new byte[] { 244, 161, 210, 52, 79, 50, 246, 61, 131, 25, 161, 155, 75, 202, 191, 119, 27, 76, 8, 97, 185, 230, 248, 94, 192, 33, 115, 32, 91, 155, 89, 76, 208, 235, 177, 14, 1, 33, 163, 10, 68, 122, 91, 202, 29, 233, 114, 254, 6, 102, 153, 136, 52, 198, 183, 179, 118, 146, 245, 158, 138, 15, 78, 97 }, new byte[] { 45, 63, 181, 254, 152, 146, 207, 7, 152, 247, 87, 141, 127, 124, 155, 59, 144, 204, 120, 211, 88, 253, 34, 35, 122, 9, 202, 18, 28, 196, 239, 125, 154, 41, 175, 206, 119, 176, 188, 215, 84, 125, 2, 141, 77, 210, 171, 194, 233, 97, 84, 213, 1, 148, 147, 4, 253, 231, 9, 92, 188, 193, 213, 179, 143, 161, 211, 67, 88, 145, 157, 152, 68, 247, 192, 167, 201, 62, 170, 72, 111, 47, 4, 82, 251, 161, 73, 212, 91, 150, 69, 60, 36, 166, 197, 99, 150, 162, 96, 112, 70, 55, 9, 218, 41, 168, 253, 141, 17, 27, 224, 35, 80, 96, 151, 183, 187, 7, 19, 87, 231, 97, 186, 68, 148, 3, 104, 138 }, null });

            migrationBuilder.InsertData(
                table: "UserOperationClaims",
                columns: new[] { "Id", "CreatedDate", "DeletedDate", "OperationClaimId", "UpdatedDate", "UserId" },
                values: new object[] { new Guid("1e8c0269-1aee-4041-be81-c6d28d3b2eb2"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, null, new Guid("baefffe2-6b04-415f-a333-93cfd03021f2") });
        }
    }
}
