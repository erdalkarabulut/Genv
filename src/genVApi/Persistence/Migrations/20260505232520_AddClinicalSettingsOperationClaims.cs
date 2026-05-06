using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddClinicalSettingsOperationClaims : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserOperationClaims",
                keyColumn: "Id",
                keyValue: new Guid("6a0693c8-ed3e-49f8-a993-1c027cc530ae"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("58fdc981-eef5-4045-be96-7d21c04caf12"));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AuthenticatorType", "CreatedDate", "DeletedDate", "Email", "PasswordHash", "PasswordSalt", "UpdatedDate" },
                values: new object[] { new Guid("1d883cf3-8187-47b3-8e8e-e3eb68daf4fe"), 0, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "narch@kodlama.io", new byte[] { 153, 159, 61, 4, 104, 82, 248, 82, 151, 135, 124, 86, 24, 249, 206, 15, 220, 93, 26, 203, 37, 123, 222, 203, 252, 63, 238, 120, 20, 171, 96, 24, 224, 62, 222, 113, 141, 109, 36, 77, 167, 173, 14, 112, 227, 170, 61, 145, 138, 25, 153, 99, 115, 177, 161, 124, 226, 201, 19, 20, 209, 110, 132, 239 }, new byte[] { 49, 224, 24, 226, 42, 71, 78, 127, 67, 218, 187, 0, 159, 84, 91, 140, 235, 106, 17, 135, 55, 132, 36, 26, 59, 118, 173, 53, 113, 127, 241, 166, 183, 64, 64, 164, 35, 222, 81, 112, 205, 118, 202, 230, 154, 14, 92, 237, 116, 20, 245, 175, 222, 187, 255, 132, 58, 162, 93, 30, 172, 132, 106, 147, 145, 90, 155, 85, 214, 164, 168, 103, 20, 137, 74, 6, 191, 67, 152, 170, 197, 183, 104, 101, 132, 150, 64, 2, 209, 21, 118, 190, 29, 143, 44, 152, 179, 27, 66, 21, 4, 135, 82, 71, 27, 22, 86, 33, 32, 82, 163, 249, 253, 136, 104, 165, 238, 170, 162, 222, 218, 172, 5, 39, 154, 52, 16, 101 }, null });

            migrationBuilder.InsertData(
                table: "UserOperationClaims",
                columns: new[] { "Id", "CreatedDate", "DeletedDate", "OperationClaimId", "UpdatedDate", "UserId" },
                values: new object[] { new Guid("11971417-bbba-4e30-b97b-f7262a3fd4c5"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, null, new Guid("1d883cf3-8187-47b3-8e8e-e3eb68daf4fe") });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserOperationClaims",
                keyColumn: "Id",
                keyValue: new Guid("11971417-bbba-4e30-b97b-f7262a3fd4c5"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("1d883cf3-8187-47b3-8e8e-e3eb68daf4fe"));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AuthenticatorType", "CreatedDate", "DeletedDate", "Email", "PasswordHash", "PasswordSalt", "UpdatedDate" },
                values: new object[] { new Guid("58fdc981-eef5-4045-be96-7d21c04caf12"), 0, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "narch@kodlama.io", new byte[] { 208, 65, 103, 34, 98, 234, 14, 126, 156, 148, 134, 31, 213, 91, 189, 20, 141, 89, 213, 162, 110, 64, 86, 234, 244, 116, 61, 227, 178, 98, 26, 127, 178, 137, 95, 44, 110, 41, 108, 48, 26, 250, 96, 147, 52, 123, 15, 66, 75, 218, 190, 141, 54, 40, 72, 251, 88, 231, 20, 251, 188, 98, 214, 184 }, new byte[] { 112, 102, 142, 204, 3, 83, 239, 226, 212, 240, 108, 213, 225, 136, 210, 65, 206, 199, 129, 70, 157, 23, 166, 151, 172, 20, 59, 141, 250, 217, 91, 169, 113, 71, 233, 255, 104, 96, 185, 188, 32, 183, 70, 13, 203, 166, 188, 220, 28, 32, 3, 79, 154, 12, 241, 21, 56, 103, 206, 143, 151, 40, 71, 112, 179, 106, 214, 114, 160, 17, 73, 135, 128, 164, 177, 152, 151, 168, 19, 154, 190, 236, 47, 186, 182, 209, 104, 42, 193, 2, 11, 241, 90, 72, 254, 246, 181, 14, 96, 24, 117, 161, 95, 2, 236, 22, 237, 210, 89, 51, 255, 114, 54, 151, 223, 108, 149, 196, 23, 236, 236, 124, 73, 5, 108, 133, 214, 83 }, null });

            migrationBuilder.InsertData(
                table: "UserOperationClaims",
                columns: new[] { "Id", "CreatedDate", "DeletedDate", "OperationClaimId", "UpdatedDate", "UserId" },
                values: new object[] { new Guid("6a0693c8-ed3e-49f8-a993-1c027cc530ae"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, null, new Guid("58fdc981-eef5-4045-be96-7d21c04caf12") });
        }
    }
}
