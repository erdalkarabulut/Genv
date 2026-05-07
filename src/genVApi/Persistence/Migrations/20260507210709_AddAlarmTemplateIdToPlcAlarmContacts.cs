using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAlarmTemplateIdToPlcAlarmContacts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserOperationClaims",
                keyColumn: "Id",
                keyValue: new Guid("d683c9cc-e369-4654-843e-04d6cbc967ea"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("7ba1293b-c129-44bd-9f44-dfb898958998"));

            migrationBuilder.AddColumn<Guid>(
                name: "AlarmTemplateId",
                table: "PlcAlarmContacts",
                type: "uuid",
                nullable: true);

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AuthenticatorType", "CreatedDate", "DeletedDate", "Email", "PasswordHash", "PasswordSalt", "UpdatedDate" },
                values: new object[] { new Guid("94ba4b35-5044-42cb-98be-aae5e26deb96"), 0, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "narch@kodlama.io", new byte[] { 18, 86, 63, 189, 109, 3, 249, 124, 42, 122, 3, 47, 223, 242, 206, 247, 190, 170, 81, 65, 15, 143, 128, 246, 249, 110, 77, 12, 113, 38, 18, 123, 210, 249, 177, 227, 190, 28, 4, 67, 85, 21, 193, 7, 28, 239, 215, 56, 52, 94, 177, 83, 23, 247, 111, 189, 84, 182, 210, 192, 54, 9, 247, 228 }, new byte[] { 38, 198, 58, 199, 103, 241, 240, 101, 165, 239, 25, 170, 83, 49, 58, 217, 210, 112, 175, 127, 95, 104, 30, 65, 131, 196, 67, 195, 225, 177, 114, 9, 110, 81, 115, 170, 192, 219, 19, 145, 165, 89, 183, 43, 125, 43, 147, 159, 121, 135, 144, 26, 172, 94, 82, 141, 205, 246, 210, 251, 136, 116, 213, 58, 211, 214, 212, 111, 224, 174, 82, 185, 1, 91, 216, 206, 248, 180, 223, 249, 253, 40, 25, 113, 203, 207, 52, 61, 222, 76, 183, 39, 248, 50, 129, 193, 45, 229, 191, 248, 89, 208, 125, 194, 76, 31, 235, 30, 13, 241, 104, 135, 25, 178, 75, 110, 29, 126, 158, 244, 246, 97, 151, 231, 213, 163, 115, 52 }, null });

            migrationBuilder.InsertData(
                table: "UserOperationClaims",
                columns: new[] { "Id", "CreatedDate", "DeletedDate", "OperationClaimId", "UpdatedDate", "UserId" },
                values: new object[] { new Guid("36f925cd-5158-4305-aaa3-32de64415df4"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, null, new Guid("94ba4b35-5044-42cb-98be-aae5e26deb96") });

            migrationBuilder.CreateIndex(
                name: "IX_PlcAlarmContacts_AlarmTemplateId",
                table: "PlcAlarmContacts",
                column: "AlarmTemplateId");

            migrationBuilder.AddForeignKey(
                name: "FK_PlcAlarmContacts_PlcAlarmTemplates_AlarmTemplateId",
                table: "PlcAlarmContacts",
                column: "AlarmTemplateId",
                principalTable: "PlcAlarmTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlcAlarmContacts_PlcAlarmTemplates_AlarmTemplateId",
                table: "PlcAlarmContacts");

            migrationBuilder.DropIndex(
                name: "IX_PlcAlarmContacts_AlarmTemplateId",
                table: "PlcAlarmContacts");

            migrationBuilder.DeleteData(
                table: "UserOperationClaims",
                keyColumn: "Id",
                keyValue: new Guid("36f925cd-5158-4305-aaa3-32de64415df4"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("94ba4b35-5044-42cb-98be-aae5e26deb96"));

            migrationBuilder.DropColumn(
                name: "AlarmTemplateId",
                table: "PlcAlarmContacts");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AuthenticatorType", "CreatedDate", "DeletedDate", "Email", "PasswordHash", "PasswordSalt", "UpdatedDate" },
                values: new object[] { new Guid("7ba1293b-c129-44bd-9f44-dfb898958998"), 0, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "narch@kodlama.io", new byte[] { 201, 223, 181, 232, 28, 235, 196, 16, 46, 254, 31, 47, 216, 158, 166, 147, 39, 2, 88, 109, 41, 201, 171, 212, 245, 118, 72, 192, 157, 114, 25, 16, 234, 131, 238, 117, 17, 133, 20, 41, 195, 64, 185, 71, 155, 20, 31, 89, 154, 81, 112, 160, 69, 198, 208, 217, 243, 177, 28, 159, 223, 220, 101, 217 }, new byte[] { 40, 123, 34, 89, 11, 122, 71, 162, 166, 59, 123, 219, 170, 186, 212, 145, 132, 224, 45, 104, 114, 141, 158, 46, 61, 114, 134, 212, 170, 191, 46, 199, 48, 116, 207, 242, 36, 120, 233, 20, 221, 98, 124, 68, 136, 231, 230, 138, 134, 217, 17, 210, 255, 115, 99, 69, 191, 206, 28, 23, 177, 193, 85, 132, 74, 63, 140, 102, 47, 171, 216, 173, 232, 197, 83, 235, 116, 79, 246, 43, 170, 232, 139, 136, 152, 172, 136, 118, 183, 159, 77, 203, 248, 64, 158, 38, 211, 167, 206, 164, 9, 184, 170, 73, 38, 173, 222, 46, 126, 137, 172, 51, 146, 234, 179, 193, 252, 132, 172, 26, 215, 247, 225, 42, 5, 234, 58, 146 }, null });

            migrationBuilder.InsertData(
                table: "UserOperationClaims",
                columns: new[] { "Id", "CreatedDate", "DeletedDate", "OperationClaimId", "UpdatedDate", "UserId" },
                values: new object[] { new Guid("d683c9cc-e369-4654-843e-04d6cbc967ea"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, null, new Guid("7ba1293b-c129-44bd-9f44-dfb898958998") });
        }
    }
}
