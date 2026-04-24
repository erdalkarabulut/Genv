using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPlcIndustrialMonitoring : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserOperationClaims",
                keyColumn: "Id",
                keyValue: new Guid("9cd5d801-8800-45d9-bddf-7873b23693ad"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("08fcbbaa-6256-4645-9872-63687afac7e7"));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AuthenticatorType", "CreatedDate", "DeletedDate", "Email", "PasswordHash", "PasswordSalt", "UpdatedDate" },
                values: new object[] { new Guid("baefffe2-6b04-415f-a333-93cfd03021f2"), 0, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "narch@kodlama.io", new byte[] { 244, 161, 210, 52, 79, 50, 246, 61, 131, 25, 161, 155, 75, 202, 191, 119, 27, 76, 8, 97, 185, 230, 248, 94, 192, 33, 115, 32, 91, 155, 89, 76, 208, 235, 177, 14, 1, 33, 163, 10, 68, 122, 91, 202, 29, 233, 114, 254, 6, 102, 153, 136, 52, 198, 183, 179, 118, 146, 245, 158, 138, 15, 78, 97 }, new byte[] { 45, 63, 181, 254, 152, 146, 207, 7, 152, 247, 87, 141, 127, 124, 155, 59, 144, 204, 120, 211, 88, 253, 34, 35, 122, 9, 202, 18, 28, 196, 239, 125, 154, 41, 175, 206, 119, 176, 188, 215, 84, 125, 2, 141, 77, 210, 171, 194, 233, 97, 84, 213, 1, 148, 147, 4, 253, 231, 9, 92, 188, 193, 213, 179, 143, 161, 211, 67, 88, 145, 157, 152, 68, 247, 192, 167, 201, 62, 170, 72, 111, 47, 4, 82, 251, 161, 73, 212, 91, 150, 69, 60, 36, 166, 197, 99, 150, 162, 96, 112, 70, 55, 9, 218, 41, 168, 253, 141, 17, 27, 224, 35, 80, 96, 151, 183, 187, 7, 19, 87, 231, 97, 186, 68, 148, 3, 104, 138 }, null });

            migrationBuilder.InsertData(
                table: "UserOperationClaims",
                columns: new[] { "Id", "CreatedDate", "DeletedDate", "OperationClaimId", "UpdatedDate", "UserId" },
                values: new object[] { new Guid("1e8c0269-1aee-4041-be81-c6d28d3b2eb2"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, null, new Guid("baefffe2-6b04-415f-a333-93cfd03021f2") });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserOperationClaims",
                keyColumn: "Id",
                keyValue: new Guid("1e8c0269-1aee-4041-be81-c6d28d3b2eb2"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("baefffe2-6b04-415f-a333-93cfd03021f2"));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AuthenticatorType", "CreatedDate", "DeletedDate", "Email", "PasswordHash", "PasswordSalt", "UpdatedDate" },
                values: new object[] { new Guid("08fcbbaa-6256-4645-9872-63687afac7e7"), 0, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "narch@kodlama.io", new byte[] { 0, 174, 241, 171, 84, 56, 93, 125, 150, 5, 205, 76, 197, 225, 55, 84, 246, 165, 84, 24, 8, 194, 102, 27, 193, 59, 219, 130, 194, 95, 231, 207, 197, 173, 130, 10, 18, 16, 173, 8, 61, 131, 26, 8, 84, 7, 250, 211, 231, 255, 52, 79, 166, 89, 189, 240, 41, 61, 237, 119, 210, 81, 173, 107 }, new byte[] { 233, 240, 47, 239, 13, 214, 224, 53, 231, 126, 215, 144, 92, 97, 76, 80, 46, 229, 33, 38, 188, 13, 38, 169, 215, 112, 31, 207, 221, 15, 176, 203, 166, 186, 165, 227, 117, 51, 76, 251, 125, 126, 238, 201, 202, 2, 128, 152, 78, 252, 92, 69, 166, 159, 1, 230, 121, 227, 26, 237, 193, 249, 111, 215, 242, 229, 67, 244, 94, 213, 199, 245, 233, 198, 138, 178, 27, 130, 180, 39, 250, 157, 74, 131, 55, 148, 116, 213, 225, 53, 211, 224, 127, 87, 220, 134, 134, 46, 15, 183, 62, 33, 78, 237, 158, 96, 38, 84, 219, 109, 37, 229, 112, 31, 249, 75, 96, 151, 194, 235, 184, 93, 66, 179, 94, 252, 205, 213 }, null });

            migrationBuilder.InsertData(
                table: "UserOperationClaims",
                columns: new[] { "Id", "CreatedDate", "DeletedDate", "OperationClaimId", "UpdatedDate", "UserId" },
                values: new object[] { new Guid("9cd5d801-8800-45d9-bddf-7873b23693ad"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, null, new Guid("08fcbbaa-6256-4645-9872-63687afac7e7") });
        }
    }
}
