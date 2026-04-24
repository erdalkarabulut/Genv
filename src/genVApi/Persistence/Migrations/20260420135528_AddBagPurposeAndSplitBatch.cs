using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBagPurposeAndSplitBatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserOperationClaims",
                keyColumn: "Id",
                keyValue: new Guid("2caa40aa-3f5b-4046-a22f-2f796fd857f0"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("4767bb64-ab49-42e6-99dc-f9f3d7d1715f"));

            migrationBuilder.AddColumn<string>(
                name: "Purpose",
                table: "Bags",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "SplitBatchId",
                table: "Bags",
                type: "uuid",
                nullable: true);

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AuthenticatorType", "CreatedDate", "DeletedDate", "Email", "PasswordHash", "PasswordSalt", "UpdatedDate" },
                values: new object[] { new Guid("bb29564d-751e-4159-99f2-205c172b1c01"), 0, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "narch@kodlama.io", new byte[] { 192, 191, 234, 182, 114, 50, 239, 138, 113, 236, 214, 128, 232, 192, 224, 50, 180, 254, 197, 163, 14, 76, 6, 119, 133, 200, 101, 167, 126, 16, 8, 116, 101, 80, 72, 175, 179, 222, 79, 253, 78, 114, 52, 81, 185, 69, 213, 89, 100, 6, 241, 251, 170, 179, 211, 81, 66, 200, 57, 253, 201, 99, 55, 214 }, new byte[] { 19, 233, 50, 98, 129, 163, 251, 29, 109, 48, 133, 173, 135, 251, 131, 219, 16, 52, 7, 240, 26, 253, 245, 2, 68, 19, 96, 201, 155, 200, 17, 147, 1, 218, 57, 88, 217, 43, 24, 50, 52, 40, 138, 159, 197, 172, 145, 156, 28, 197, 236, 71, 117, 6, 193, 122, 112, 55, 47, 222, 107, 165, 141, 202, 178, 96, 102, 120, 208, 106, 250, 29, 28, 166, 247, 103, 227, 139, 100, 222, 207, 175, 132, 117, 168, 33, 117, 46, 171, 42, 5, 5, 4, 163, 95, 137, 50, 215, 118, 248, 102, 21, 206, 123, 127, 63, 22, 53, 156, 124, 96, 236, 191, 102, 209, 218, 17, 249, 44, 180, 26, 4, 216, 220, 247, 57, 61, 9 }, null });

            migrationBuilder.InsertData(
                table: "UserOperationClaims",
                columns: new[] { "Id", "CreatedDate", "DeletedDate", "OperationClaimId", "UpdatedDate", "UserId" },
                values: new object[] { new Guid("86828272-3b4e-47cc-bad0-8360f7e5972a"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, null, new Guid("bb29564d-751e-4159-99f2-205c172b1c01") });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserOperationClaims",
                keyColumn: "Id",
                keyValue: new Guid("86828272-3b4e-47cc-bad0-8360f7e5972a"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("bb29564d-751e-4159-99f2-205c172b1c01"));

            migrationBuilder.DropColumn(
                name: "Purpose",
                table: "Bags");

            migrationBuilder.DropColumn(
                name: "SplitBatchId",
                table: "Bags");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AuthenticatorType", "CreatedDate", "DeletedDate", "Email", "PasswordHash", "PasswordSalt", "UpdatedDate" },
                values: new object[] { new Guid("4767bb64-ab49-42e6-99dc-f9f3d7d1715f"), 0, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "narch@kodlama.io", new byte[] { 247, 24, 211, 107, 57, 98, 174, 160, 54, 59, 166, 241, 6, 16, 53, 159, 131, 252, 24, 80, 149, 240, 91, 241, 178, 29, 240, 170, 207, 120, 73, 42, 242, 213, 22, 88, 11, 104, 115, 201, 185, 23, 2, 253, 16, 181, 37, 222, 154, 218, 169, 187, 143, 234, 52, 143, 38, 156, 62, 226, 85, 27, 97, 59 }, new byte[] { 110, 232, 231, 139, 140, 196, 228, 162, 155, 251, 43, 149, 198, 172, 233, 2, 238, 41, 78, 71, 233, 60, 74, 164, 106, 136, 248, 144, 239, 159, 179, 40, 208, 222, 102, 195, 233, 194, 4, 210, 60, 230, 119, 14, 105, 87, 65, 195, 175, 254, 32, 246, 108, 127, 207, 202, 115, 183, 188, 168, 188, 134, 85, 109, 249, 46, 109, 42, 246, 49, 211, 11, 3, 221, 86, 149, 24, 249, 62, 216, 130, 164, 163, 89, 6, 59, 225, 219, 80, 76, 197, 55, 20, 162, 241, 178, 209, 184, 216, 33, 80, 150, 136, 148, 141, 135, 61, 88, 77, 122, 224, 196, 160, 23, 110, 189, 211, 1, 101, 56, 142, 160, 70, 233, 19, 221, 124, 21 }, null });

            migrationBuilder.InsertData(
                table: "UserOperationClaims",
                columns: new[] { "Id", "CreatedDate", "DeletedDate", "OperationClaimId", "UpdatedDate", "UserId" },
                values: new object[] { new Guid("2caa40aa-3f5b-4046-a22f-2f796fd857f0"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, null, new Guid("4767bb64-ab49-42e6-99dc-f9f3d7d1715f") });
        }
    }
}
