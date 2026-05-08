using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCollectionSessionAbsoluteCellCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserOperationClaims",
                keyColumn: "Id",
                keyValue: new Guid("da4d132c-170b-4a16-b37f-6ff5200073c6"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("e2c827a3-143f-4f0a-a099-3b954eb7d371"));

            migrationBuilder.AddColumn<double>(
                name: "AbsoluteCellCount",
                table: "CollectionSessions",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.Sql(
                """
                UPDATE "CollectionSessions"
                SET "AbsoluteCellCount" = ("WBC" * "Cd45Percent" * "Cd34Percent") / 10000.0,
                    "PreMhs" = ("WBC" * "Cd45Percent" * "Cd34Percent") / 10000.0
                WHERE "WBC" > 0;
                """);

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AuthenticatorType", "CreatedDate", "DeletedDate", "Email", "PasswordHash", "PasswordSalt", "UpdatedDate" },
                values: new object[] { new Guid("64eb971c-44d8-42f5-b0e6-6876d559def1"), 0, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "narch@kodlama.io", new byte[] { 27, 218, 78, 108, 3, 149, 45, 7, 55, 194, 172, 86, 155, 159, 33, 158, 132, 192, 50, 85, 190, 201, 190, 115, 53, 9, 101, 151, 56, 179, 208, 236, 143, 6, 99, 198, 80, 189, 11, 253, 215, 129, 87, 17, 193, 59, 189, 187, 236, 62, 103, 19, 117, 19, 48, 122, 144, 186, 9, 217, 208, 219, 54, 134 }, new byte[] { 85, 182, 113, 176, 146, 145, 181, 41, 32, 184, 136, 124, 12, 176, 159, 13, 14, 202, 238, 161, 75, 84, 237, 160, 60, 77, 130, 219, 9, 135, 221, 17, 8, 39, 211, 26, 197, 17, 70, 206, 60, 242, 135, 25, 183, 151, 151, 23, 26, 223, 95, 88, 196, 95, 196, 12, 199, 155, 8, 47, 1, 6, 98, 171, 49, 129, 19, 127, 19, 86, 184, 125, 252, 140, 151, 72, 35, 168, 180, 70, 91, 104, 78, 227, 2, 158, 187, 167, 250, 184, 93, 248, 144, 55, 247, 125, 85, 217, 76, 79, 24, 213, 9, 249, 90, 151, 234, 5, 25, 164, 35, 71, 27, 88, 184, 246, 8, 240, 176, 72, 128, 116, 106, 223, 248, 202, 102, 214 }, null });

            migrationBuilder.InsertData(
                table: "UserOperationClaims",
                columns: new[] { "Id", "CreatedDate", "DeletedDate", "OperationClaimId", "UpdatedDate", "UserId" },
                values: new object[] { new Guid("a09af27c-2cc6-4deb-b31a-0ce34a5ab35f"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, null, new Guid("64eb971c-44d8-42f5-b0e6-6876d559def1") });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserOperationClaims",
                keyColumn: "Id",
                keyValue: new Guid("a09af27c-2cc6-4deb-b31a-0ce34a5ab35f"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("64eb971c-44d8-42f5-b0e6-6876d559def1"));

            migrationBuilder.DropColumn(
                name: "AbsoluteCellCount",
                table: "CollectionSessions");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AuthenticatorType", "CreatedDate", "DeletedDate", "Email", "PasswordHash", "PasswordSalt", "UpdatedDate" },
                values: new object[] { new Guid("e2c827a3-143f-4f0a-a099-3b954eb7d371"), 0, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "narch@kodlama.io", new byte[] { 157, 91, 146, 234, 243, 183, 231, 210, 48, 22, 236, 10, 118, 107, 159, 235, 160, 230, 115, 121, 138, 126, 198, 65, 51, 125, 190, 86, 166, 113, 105, 76, 34, 153, 31, 147, 187, 254, 191, 226, 62, 203, 125, 45, 0, 169, 153, 31, 30, 168, 118, 164, 115, 29, 21, 200, 44, 23, 4, 83, 210, 104, 136, 226 }, new byte[] { 94, 57, 1, 30, 138, 189, 54, 79, 62, 140, 211, 34, 177, 218, 123, 63, 46, 91, 133, 51, 220, 201, 104, 236, 137, 111, 190, 223, 133, 5, 18, 36, 200, 73, 31, 110, 131, 36, 27, 159, 110, 233, 38, 31, 89, 242, 86, 156, 143, 60, 11, 82, 100, 172, 8, 69, 124, 167, 248, 39, 191, 244, 92, 49, 121, 250, 250, 42, 11, 22, 117, 239, 65, 126, 215, 32, 111, 225, 46, 117, 223, 134, 179, 177, 134, 105, 47, 145, 238, 55, 28, 99, 100, 178, 177, 159, 99, 105, 218, 189, 220, 146, 87, 200, 96, 112, 140, 8, 123, 5, 78, 77, 7, 165, 124, 40, 163, 138, 223, 210, 106, 84, 94, 95, 57, 82, 53, 201 }, null });

            migrationBuilder.InsertData(
                table: "UserOperationClaims",
                columns: new[] { "Id", "CreatedDate", "DeletedDate", "OperationClaimId", "UpdatedDate", "UserId" },
                values: new object[] { new Guid("da4d132c-170b-4a16-b37f-6ff5200073c6"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, null, new Guid("e2c827a3-143f-4f0a-a099-3b954eb7d371") });
        }
    }
}
