using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPatientDonorIdentityNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserOperationClaims",
                keyColumn: "Id",
                keyValue: new Guid("b48afbfc-1fe8-4a62-b24a-a66ab29b9cde"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("eb180b88-f92d-4bc1-a351-59778f738a03"));

            migrationBuilder.AddColumn<string>(
                name: "IdentityNumber",
                table: "Patients",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdentityNumber",
                table: "Donors",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AuthenticatorType", "CreatedDate", "DeletedDate", "Email", "PasswordHash", "PasswordSalt", "UpdatedDate" },
                values: new object[] { new Guid("58fdc981-eef5-4045-be96-7d21c04caf12"), 0, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "narch@kodlama.io", new byte[] { 208, 65, 103, 34, 98, 234, 14, 126, 156, 148, 134, 31, 213, 91, 189, 20, 141, 89, 213, 162, 110, 64, 86, 234, 244, 116, 61, 227, 178, 98, 26, 127, 178, 137, 95, 44, 110, 41, 108, 48, 26, 250, 96, 147, 52, 123, 15, 66, 75, 218, 190, 141, 54, 40, 72, 251, 88, 231, 20, 251, 188, 98, 214, 184 }, new byte[] { 112, 102, 142, 204, 3, 83, 239, 226, 212, 240, 108, 213, 225, 136, 210, 65, 206, 199, 129, 70, 157, 23, 166, 151, 172, 20, 59, 141, 250, 217, 91, 169, 113, 71, 233, 255, 104, 96, 185, 188, 32, 183, 70, 13, 203, 166, 188, 220, 28, 32, 3, 79, 154, 12, 241, 21, 56, 103, 206, 143, 151, 40, 71, 112, 179, 106, 214, 114, 160, 17, 73, 135, 128, 164, 177, 152, 151, 168, 19, 154, 190, 236, 47, 186, 182, 209, 104, 42, 193, 2, 11, 241, 90, 72, 254, 246, 181, 14, 96, 24, 117, 161, 95, 2, 236, 22, 237, 210, 89, 51, 255, 114, 54, 151, 223, 108, 149, 196, 23, 236, 236, 124, 73, 5, 108, 133, 214, 83 }, null });

            migrationBuilder.InsertData(
                table: "UserOperationClaims",
                columns: new[] { "Id", "CreatedDate", "DeletedDate", "OperationClaimId", "UpdatedDate", "UserId" },
                values: new object[] { new Guid("6a0693c8-ed3e-49f8-a993-1c027cc530ae"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, null, new Guid("58fdc981-eef5-4045-be96-7d21c04caf12") });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserOperationClaims",
                keyColumn: "Id",
                keyValue: new Guid("6a0693c8-ed3e-49f8-a993-1c027cc530ae"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("58fdc981-eef5-4045-be96-7d21c04caf12"));

            migrationBuilder.DropColumn(
                name: "IdentityNumber",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "IdentityNumber",
                table: "Donors");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AuthenticatorType", "CreatedDate", "DeletedDate", "Email", "PasswordHash", "PasswordSalt", "UpdatedDate" },
                values: new object[] { new Guid("eb180b88-f92d-4bc1-a351-59778f738a03"), 0, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "narch@kodlama.io", new byte[] { 236, 194, 243, 138, 13, 62, 59, 181, 83, 6, 213, 222, 65, 250, 112, 165, 134, 91, 94, 126, 160, 159, 145, 42, 203, 144, 251, 73, 166, 200, 206, 97, 2, 175, 233, 197, 237, 7, 92, 106, 154, 134, 161, 205, 168, 1, 246, 28, 13, 4, 188, 34, 204, 172, 93, 237, 66, 219, 237, 200, 204, 163, 105, 180 }, new byte[] { 245, 99, 218, 93, 0, 27, 240, 124, 40, 222, 195, 81, 21, 138, 253, 254, 70, 99, 77, 60, 226, 46, 88, 4, 136, 202, 5, 46, 14, 197, 193, 106, 145, 5, 81, 116, 103, 228, 10, 82, 217, 99, 231, 79, 57, 152, 205, 18, 173, 161, 173, 223, 19, 182, 32, 21, 236, 153, 232, 40, 145, 182, 197, 149, 62, 75, 115, 55, 161, 212, 144, 201, 238, 218, 47, 36, 236, 61, 16, 215, 103, 21, 181, 171, 57, 189, 238, 122, 0, 32, 118, 162, 132, 129, 11, 174, 154, 188, 184, 29, 133, 218, 74, 7, 53, 1, 120, 247, 223, 17, 110, 251, 80, 75, 99, 31, 144, 1, 138, 136, 237, 95, 127, 200, 67, 28, 170, 93 }, null });

            migrationBuilder.InsertData(
                table: "UserOperationClaims",
                columns: new[] { "Id", "CreatedDate", "DeletedDate", "OperationClaimId", "UpdatedDate", "UserId" },
                values: new object[] { new Guid("b48afbfc-1fe8-4a62-b24a-a66ab29b9cde"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, null, new Guid("eb180b88-f92d-4bc1-a351-59778f738a03") });
        }
    }
}
