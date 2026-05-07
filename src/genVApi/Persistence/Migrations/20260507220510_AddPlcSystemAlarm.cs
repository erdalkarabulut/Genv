using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPlcSystemAlarm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserOperationClaims",
                keyColumn: "Id",
                keyValue: new Guid("36f925cd-5158-4305-aaa3-32de64415df4"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("94ba4b35-5044-42cb-98be-aae5e26deb96"));

            migrationBuilder.CreateTable(
                name: "PlcSystemAlarms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    DevicePrefix = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SensorCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    RelatedDeviceAddress = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    OccurredAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsResolved = table.Column<bool>(type: "boolean", nullable: false),
                    ResolvedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlcSystemAlarms", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AuthenticatorType", "CreatedDate", "DeletedDate", "Email", "PasswordHash", "PasswordSalt", "UpdatedDate" },
                values: new object[] { new Guid("e2c827a3-143f-4f0a-a099-3b954eb7d371"), 0, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "narch@kodlama.io", new byte[] { 157, 91, 146, 234, 243, 183, 231, 210, 48, 22, 236, 10, 118, 107, 159, 235, 160, 230, 115, 121, 138, 126, 198, 65, 51, 125, 190, 86, 166, 113, 105, 76, 34, 153, 31, 147, 187, 254, 191, 226, 62, 203, 125, 45, 0, 169, 153, 31, 30, 168, 118, 164, 115, 29, 21, 200, 44, 23, 4, 83, 210, 104, 136, 226 }, new byte[] { 94, 57, 1, 30, 138, 189, 54, 79, 62, 140, 211, 34, 177, 218, 123, 63, 46, 91, 133, 51, 220, 201, 104, 236, 137, 111, 190, 223, 133, 5, 18, 36, 200, 73, 31, 110, 131, 36, 27, 159, 110, 233, 38, 31, 89, 242, 86, 156, 143, 60, 11, 82, 100, 172, 8, 69, 124, 167, 248, 39, 191, 244, 92, 49, 121, 250, 250, 42, 11, 22, 117, 239, 65, 126, 215, 32, 111, 225, 46, 117, 223, 134, 179, 177, 134, 105, 47, 145, 238, 55, 28, 99, 100, 178, 177, 159, 99, 105, 218, 189, 220, 146, 87, 200, 96, 112, 140, 8, 123, 5, 78, 77, 7, 165, 124, 40, 163, 138, 223, 210, 106, 84, 94, 95, 57, 82, 53, 201 }, null });

            migrationBuilder.InsertData(
                table: "UserOperationClaims",
                columns: new[] { "Id", "CreatedDate", "DeletedDate", "OperationClaimId", "UpdatedDate", "UserId" },
                values: new object[] { new Guid("da4d132c-170b-4a16-b37f-6ff5200073c6"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, null, new Guid("e2c827a3-143f-4f0a-a099-3b954eb7d371") });

            migrationBuilder.CreateIndex(
                name: "IX_PlcSystemAlarms_IsResolved_OccurredAtUtc",
                table: "PlcSystemAlarms",
                columns: new[] { "IsResolved", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_PlcSystemAlarms_SensorCode",
                table: "PlcSystemAlarms",
                column: "SensorCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlcSystemAlarms");

            migrationBuilder.DeleteData(
                table: "UserOperationClaims",
                keyColumn: "Id",
                keyValue: new Guid("da4d132c-170b-4a16-b37f-6ff5200073c6"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("e2c827a3-143f-4f0a-a099-3b954eb7d371"));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AuthenticatorType", "CreatedDate", "DeletedDate", "Email", "PasswordHash", "PasswordSalt", "UpdatedDate" },
                values: new object[] { new Guid("94ba4b35-5044-42cb-98be-aae5e26deb96"), 0, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "narch@kodlama.io", new byte[] { 18, 86, 63, 189, 109, 3, 249, 124, 42, 122, 3, 47, 223, 242, 206, 247, 190, 170, 81, 65, 15, 143, 128, 246, 249, 110, 77, 12, 113, 38, 18, 123, 210, 249, 177, 227, 190, 28, 4, 67, 85, 21, 193, 7, 28, 239, 215, 56, 52, 94, 177, 83, 23, 247, 111, 189, 84, 182, 210, 192, 54, 9, 247, 228 }, new byte[] { 38, 198, 58, 199, 103, 241, 240, 101, 165, 239, 25, 170, 83, 49, 58, 217, 210, 112, 175, 127, 95, 104, 30, 65, 131, 196, 67, 195, 225, 177, 114, 9, 110, 81, 115, 170, 192, 219, 19, 145, 165, 89, 183, 43, 125, 43, 147, 159, 121, 135, 144, 26, 172, 94, 82, 141, 205, 246, 210, 251, 136, 116, 213, 58, 211, 214, 212, 111, 224, 174, 82, 185, 1, 91, 216, 206, 248, 180, 223, 249, 253, 40, 25, 113, 203, 207, 52, 61, 222, 76, 183, 39, 248, 50, 129, 193, 45, 229, 191, 248, 89, 208, 125, 194, 76, 31, 235, 30, 13, 241, 104, 135, 25, 178, 75, 110, 29, 126, 158, 244, 246, 97, 151, 231, 213, 163, 115, 52 }, null });

            migrationBuilder.InsertData(
                table: "UserOperationClaims",
                columns: new[] { "Id", "CreatedDate", "DeletedDate", "OperationClaimId", "UpdatedDate", "UserId" },
                values: new object[] { new Guid("36f925cd-5158-4305-aaa3-32de64415df4"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, null, new Guid("94ba4b35-5044-42cb-98be-aae5e26deb96") });
        }
    }
}
