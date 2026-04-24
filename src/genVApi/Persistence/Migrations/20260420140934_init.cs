using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserOperationClaims",
                keyColumn: "Id",
                keyValue: new Guid("86828272-3b4e-47cc-bad0-8360f7e5972a"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("bb29564d-751e-4159-99f2-205c172b1c01"));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AuthenticatorType", "CreatedDate", "DeletedDate", "Email", "PasswordHash", "PasswordSalt", "UpdatedDate" },
                values: new object[] { new Guid("def5d923-1b48-462f-9d6d-82636d27a211"), 0, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "narch@kodlama.io", new byte[] { 97, 143, 38, 230, 150, 31, 171, 50, 1, 176, 217, 48, 206, 199, 208, 219, 7, 23, 152, 19, 216, 247, 142, 177, 246, 4, 49, 173, 194, 138, 200, 91, 22, 41, 27, 156, 226, 108, 196, 198, 200, 255, 37, 153, 192, 18, 165, 110, 70, 28, 220, 80, 174, 66, 51, 130, 33, 151, 105, 187, 240, 75, 155, 82 }, new byte[] { 250, 114, 169, 158, 245, 157, 137, 114, 220, 50, 121, 59, 126, 8, 31, 167, 15, 3, 190, 156, 183, 41, 103, 10, 214, 81, 122, 116, 248, 138, 141, 227, 239, 163, 128, 48, 6, 228, 237, 175, 86, 78, 11, 84, 207, 246, 53, 52, 199, 225, 85, 210, 39, 238, 235, 95, 43, 184, 118, 237, 125, 94, 212, 241, 176, 6, 140, 221, 148, 51, 135, 216, 149, 174, 209, 105, 160, 147, 41, 111, 82, 147, 53, 86, 59, 147, 196, 157, 220, 72, 113, 228, 77, 51, 235, 35, 200, 114, 8, 194, 78, 255, 157, 217, 152, 104, 59, 1, 50, 1, 192, 32, 200, 45, 162, 127, 33, 198, 99, 33, 34, 246, 34, 46, 121, 40, 153, 200 }, null });

            migrationBuilder.InsertData(
                table: "UserOperationClaims",
                columns: new[] { "Id", "CreatedDate", "DeletedDate", "OperationClaimId", "UpdatedDate", "UserId" },
                values: new object[] { new Guid("947e94a0-cc2d-4f94-8b4e-1578e269e710"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, null, new Guid("def5d923-1b48-462f-9d6d-82636d27a211") });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
                values: new object[] { new Guid("bb29564d-751e-4159-99f2-205c172b1c01"), 0, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "narch@kodlama.io", new byte[] { 192, 191, 234, 182, 114, 50, 239, 138, 113, 236, 214, 128, 232, 192, 224, 50, 180, 254, 197, 163, 14, 76, 6, 119, 133, 200, 101, 167, 126, 16, 8, 116, 101, 80, 72, 175, 179, 222, 79, 253, 78, 114, 52, 81, 185, 69, 213, 89, 100, 6, 241, 251, 170, 179, 211, 81, 66, 200, 57, 253, 201, 99, 55, 214 }, new byte[] { 19, 233, 50, 98, 129, 163, 251, 29, 109, 48, 133, 173, 135, 251, 131, 219, 16, 52, 7, 240, 26, 253, 245, 2, 68, 19, 96, 201, 155, 200, 17, 147, 1, 218, 57, 88, 217, 43, 24, 50, 52, 40, 138, 159, 197, 172, 145, 156, 28, 197, 236, 71, 117, 6, 193, 122, 112, 55, 47, 222, 107, 165, 141, 202, 178, 96, 102, 120, 208, 106, 250, 29, 28, 166, 247, 103, 227, 139, 100, 222, 207, 175, 132, 117, 168, 33, 117, 46, 171, 42, 5, 5, 4, 163, 95, 137, 50, 215, 118, 248, 102, 21, 206, 123, 127, 63, 22, 53, 156, 124, 96, 236, 191, 102, 209, 218, 17, 249, 44, 180, 26, 4, 216, 220, 247, 57, 61, 9 }, null });

            migrationBuilder.InsertData(
                table: "UserOperationClaims",
                columns: new[] { "Id", "CreatedDate", "DeletedDate", "OperationClaimId", "UpdatedDate", "UserId" },
                values: new object[] { new Guid("86828272-3b4e-47cc-bad0-8360f7e5972a"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, null, new Guid("bb29564d-751e-4159-99f2-205c172b1c01") });
        }
    }
}
