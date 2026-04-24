using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSessionPreFlowAndBagWbcAndDliDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserOperationClaims",
                keyColumn: "Id",
                keyValue: new Guid("a12be4f7-593d-4c8b-9497-898b798fccf7"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("29efa6f9-9bda-4830-a9a7-17372ad77c9d"));

            migrationBuilder.AddColumn<DateTime>(
                name: "BirthDate",
                table: "Donors",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DonorType",
                table: "Donors",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Related");

            migrationBuilder.AddColumn<double>(
                name: "Cd3Percent",
                table: "DliProducts",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Date",
                table: "DliProducts",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "DonorId",
                table: "DliProducts",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "LymphocytePercent",
                table: "DliProducts",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SessionId",
                table: "DliProducts",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "TotalCd3",
                table: "DliProducts",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Wbc",
                table: "DliProducts",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "HctPost",
                table: "CollectionSessions",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "HgbPost",
                table: "CollectionSessions",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "PltPost",
                table: "CollectionSessions",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "PreCd34Percent",
                table: "CollectionSessions",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "PreCd45Percent",
                table: "CollectionSessions",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "PreMhs",
                table: "CollectionSessions",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "WbcPost",
                table: "CollectionSessions",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Cd34Percent",
                table: "Bags",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Cd3Percent",
                table: "Bags",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Cd45Percent",
                table: "Bags",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompositionNote",
                table: "Bags",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Wbc",
                table: "Bags",
                type: "double precision",
                nullable: true);

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AuthenticatorType", "CreatedDate", "DeletedDate", "Email", "PasswordHash", "PasswordSalt", "UpdatedDate" },
                values: new object[] { new Guid("08fcbbaa-6256-4645-9872-63687afac7e7"), 0, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "narch@kodlama.io", new byte[] { 0, 174, 241, 171, 84, 56, 93, 125, 150, 5, 205, 76, 197, 225, 55, 84, 246, 165, 84, 24, 8, 194, 102, 27, 193, 59, 219, 130, 194, 95, 231, 207, 197, 173, 130, 10, 18, 16, 173, 8, 61, 131, 26, 8, 84, 7, 250, 211, 231, 255, 52, 79, 166, 89, 189, 240, 41, 61, 237, 119, 210, 81, 173, 107 }, new byte[] { 233, 240, 47, 239, 13, 214, 224, 53, 231, 126, 215, 144, 92, 97, 76, 80, 46, 229, 33, 38, 188, 13, 38, 169, 215, 112, 31, 207, 221, 15, 176, 203, 166, 186, 165, 227, 117, 51, 76, 251, 125, 126, 238, 201, 202, 2, 128, 152, 78, 252, 92, 69, 166, 159, 1, 230, 121, 227, 26, 237, 193, 249, 111, 215, 242, 229, 67, 244, 94, 213, 199, 245, 233, 198, 138, 178, 27, 130, 180, 39, 250, 157, 74, 131, 55, 148, 116, 213, 225, 53, 211, 224, 127, 87, 220, 134, 134, 46, 15, 183, 62, 33, 78, 237, 158, 96, 38, 84, 219, 109, 37, 229, 112, 31, 249, 75, 96, 151, 194, 235, 184, 93, 66, 179, 94, 252, 205, 213 }, null });

            migrationBuilder.InsertData(
                table: "UserOperationClaims",
                columns: new[] { "Id", "CreatedDate", "DeletedDate", "OperationClaimId", "UpdatedDate", "UserId" },
                values: new object[] { new Guid("9cd5d801-8800-45d9-bddf-7873b23693ad"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, null, new Guid("08fcbbaa-6256-4645-9872-63687afac7e7") });

            migrationBuilder.CreateIndex(
                name: "IX_DliProducts_DonorId",
                table: "DliProducts",
                column: "DonorId");

            migrationBuilder.CreateIndex(
                name: "IX_DliProducts_SessionId",
                table: "DliProducts",
                column: "SessionId");

            migrationBuilder.AddForeignKey(
                name: "FK_DliProducts_CollectionSessions_SessionId",
                table: "DliProducts",
                column: "SessionId",
                principalTable: "CollectionSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_DliProducts_Donors_DonorId",
                table: "DliProducts",
                column: "DonorId",
                principalTable: "Donors",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DliProducts_CollectionSessions_SessionId",
                table: "DliProducts");

            migrationBuilder.DropForeignKey(
                name: "FK_DliProducts_Donors_DonorId",
                table: "DliProducts");

            migrationBuilder.DropIndex(
                name: "IX_DliProducts_DonorId",
                table: "DliProducts");

            migrationBuilder.DropIndex(
                name: "IX_DliProducts_SessionId",
                table: "DliProducts");

            migrationBuilder.DeleteData(
                table: "UserOperationClaims",
                keyColumn: "Id",
                keyValue: new Guid("9cd5d801-8800-45d9-bddf-7873b23693ad"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("08fcbbaa-6256-4645-9872-63687afac7e7"));

            migrationBuilder.DropColumn(
                name: "BirthDate",
                table: "Donors");

            migrationBuilder.DropColumn(
                name: "DonorType",
                table: "Donors");

            migrationBuilder.DropColumn(
                name: "Cd3Percent",
                table: "DliProducts");

            migrationBuilder.DropColumn(
                name: "Date",
                table: "DliProducts");

            migrationBuilder.DropColumn(
                name: "DonorId",
                table: "DliProducts");

            migrationBuilder.DropColumn(
                name: "LymphocytePercent",
                table: "DliProducts");

            migrationBuilder.DropColumn(
                name: "SessionId",
                table: "DliProducts");

            migrationBuilder.DropColumn(
                name: "TotalCd3",
                table: "DliProducts");

            migrationBuilder.DropColumn(
                name: "Wbc",
                table: "DliProducts");

            migrationBuilder.DropColumn(
                name: "HctPost",
                table: "CollectionSessions");

            migrationBuilder.DropColumn(
                name: "HgbPost",
                table: "CollectionSessions");

            migrationBuilder.DropColumn(
                name: "PltPost",
                table: "CollectionSessions");

            migrationBuilder.DropColumn(
                name: "PreCd34Percent",
                table: "CollectionSessions");

            migrationBuilder.DropColumn(
                name: "PreCd45Percent",
                table: "CollectionSessions");

            migrationBuilder.DropColumn(
                name: "PreMhs",
                table: "CollectionSessions");

            migrationBuilder.DropColumn(
                name: "WbcPost",
                table: "CollectionSessions");

            migrationBuilder.DropColumn(
                name: "Cd34Percent",
                table: "Bags");

            migrationBuilder.DropColumn(
                name: "Cd3Percent",
                table: "Bags");

            migrationBuilder.DropColumn(
                name: "Cd45Percent",
                table: "Bags");

            migrationBuilder.DropColumn(
                name: "CompositionNote",
                table: "Bags");

            migrationBuilder.DropColumn(
                name: "Wbc",
                table: "Bags");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AuthenticatorType", "CreatedDate", "DeletedDate", "Email", "PasswordHash", "PasswordSalt", "UpdatedDate" },
                values: new object[] { new Guid("29efa6f9-9bda-4830-a9a7-17372ad77c9d"), 0, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "narch@kodlama.io", new byte[] { 249, 244, 29, 59, 169, 125, 0, 91, 241, 23, 188, 103, 77, 119, 16, 13, 210, 130, 59, 114, 15, 4, 250, 247, 128, 160, 225, 192, 86, 1, 241, 167, 120, 62, 185, 80, 207, 186, 151, 252, 242, 215, 5, 83, 188, 12, 90, 148, 18, 70, 2, 113, 146, 99, 239, 141, 143, 248, 26, 41, 248, 85, 228, 189 }, new byte[] { 167, 252, 188, 202, 232, 57, 167, 59, 64, 237, 239, 47, 228, 137, 93, 245, 102, 140, 105, 4, 156, 116, 144, 214, 63, 227, 149, 212, 248, 236, 80, 49, 218, 136, 107, 228, 229, 214, 206, 98, 176, 100, 30, 42, 131, 100, 116, 95, 115, 20, 230, 212, 191, 231, 255, 109, 58, 85, 231, 220, 72, 71, 52, 13, 203, 248, 78, 51, 82, 0, 186, 114, 91, 105, 129, 113, 111, 7, 109, 58, 95, 22, 94, 160, 66, 226, 233, 81, 235, 125, 175, 226, 121, 233, 200, 8, 75, 199, 185, 123, 161, 144, 89, 73, 77, 157, 138, 218, 208, 85, 149, 208, 163, 40, 195, 179, 33, 181, 216, 85, 139, 163, 92, 91, 148, 87, 127, 86 }, null });

            migrationBuilder.InsertData(
                table: "UserOperationClaims",
                columns: new[] { "Id", "CreatedDate", "DeletedDate", "OperationClaimId", "UpdatedDate", "UserId" },
                values: new object[] { new Guid("a12be4f7-593d-4c8b-9497-898b798fccf7"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, null, new Guid("29efa6f9-9bda-4830-a9a7-17372ad77c9d") });
        }
    }
}
