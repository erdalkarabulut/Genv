using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class seedim : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserOperationClaims",
                keyColumn: "Id",
                keyValue: new Guid("947e94a0-cc2d-4f94-8b4e-1578e269e710"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("def5d923-1b48-462f-9d6d-82636d27a211"));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AuthenticatorType", "CreatedDate", "DeletedDate", "Email", "PasswordHash", "PasswordSalt", "UpdatedDate" },
                values: new object[] { new Guid("29efa6f9-9bda-4830-a9a7-17372ad77c9d"), 0, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "narch@kodlama.io", new byte[] { 249, 244, 29, 59, 169, 125, 0, 91, 241, 23, 188, 103, 77, 119, 16, 13, 210, 130, 59, 114, 15, 4, 250, 247, 128, 160, 225, 192, 86, 1, 241, 167, 120, 62, 185, 80, 207, 186, 151, 252, 242, 215, 5, 83, 188, 12, 90, 148, 18, 70, 2, 113, 146, 99, 239, 141, 143, 248, 26, 41, 248, 85, 228, 189 }, new byte[] { 167, 252, 188, 202, 232, 57, 167, 59, 64, 237, 239, 47, 228, 137, 93, 245, 102, 140, 105, 4, 156, 116, 144, 214, 63, 227, 149, 212, 248, 236, 80, 49, 218, 136, 107, 228, 229, 214, 206, 98, 176, 100, 30, 42, 131, 100, 116, 95, 115, 20, 230, 212, 191, 231, 255, 109, 58, 85, 231, 220, 72, 71, 52, 13, 203, 248, 78, 51, 82, 0, 186, 114, 91, 105, 129, 113, 111, 7, 109, 58, 95, 22, 94, 160, 66, 226, 233, 81, 235, 125, 175, 226, 121, 233, 200, 8, 75, 199, 185, 123, 161, 144, 89, 73, 77, 157, 138, 218, 208, 85, 149, 208, 163, 40, 195, 179, 33, 181, 216, 85, 139, 163, 92, 91, 148, 87, 127, 86 }, null });

            migrationBuilder.InsertData(
                table: "UserOperationClaims",
                columns: new[] { "Id", "CreatedDate", "DeletedDate", "OperationClaimId", "UpdatedDate", "UserId" },
                values: new object[] { new Guid("a12be4f7-593d-4c8b-9497-898b798fccf7"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, null, new Guid("29efa6f9-9bda-4830-a9a7-17372ad77c9d") });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserOperationClaims",
                keyColumn: "Id",
                keyValue: new Guid("a12be4f7-593d-4c8b-9497-898b798fccf7"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("29efa6f9-9bda-4830-a9a7-17372ad77c9d"));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AuthenticatorType", "CreatedDate", "DeletedDate", "Email", "PasswordHash", "PasswordSalt", "UpdatedDate" },
                values: new object[] { new Guid("def5d923-1b48-462f-9d6d-82636d27a211"), 0, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "narch@kodlama.io", new byte[] { 97, 143, 38, 230, 150, 31, 171, 50, 1, 176, 217, 48, 206, 199, 208, 219, 7, 23, 152, 19, 216, 247, 142, 177, 246, 4, 49, 173, 194, 138, 200, 91, 22, 41, 27, 156, 226, 108, 196, 198, 200, 255, 37, 153, 192, 18, 165, 110, 70, 28, 220, 80, 174, 66, 51, 130, 33, 151, 105, 187, 240, 75, 155, 82 }, new byte[] { 250, 114, 169, 158, 245, 157, 137, 114, 220, 50, 121, 59, 126, 8, 31, 167, 15, 3, 190, 156, 183, 41, 103, 10, 214, 81, 122, 116, 248, 138, 141, 227, 239, 163, 128, 48, 6, 228, 237, 175, 86, 78, 11, 84, 207, 246, 53, 52, 199, 225, 85, 210, 39, 238, 235, 95, 43, 184, 118, 237, 125, 94, 212, 241, 176, 6, 140, 221, 148, 51, 135, 216, 149, 174, 209, 105, 160, 147, 41, 111, 82, 147, 53, 86, 59, 147, 196, 157, 220, 72, 113, 228, 77, 51, 235, 35, 200, 114, 8, 194, 78, 255, 157, 217, 152, 104, 59, 1, 50, 1, 192, 32, 200, 45, 162, 127, 33, 198, 99, 33, 34, 246, 34, 46, 121, 40, 153, 200 }, null });

            migrationBuilder.InsertData(
                table: "UserOperationClaims",
                columns: new[] { "Id", "CreatedDate", "DeletedDate", "OperationClaimId", "UpdatedDate", "UserId" },
                values: new object[] { new Guid("947e94a0-cc2d-4f94-8b4e-1578e269e710"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, null, new Guid("def5d923-1b48-462f-9d6d-82636d27a211") });
        }
    }
}
