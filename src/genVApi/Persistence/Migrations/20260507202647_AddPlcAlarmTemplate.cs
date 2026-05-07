using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPlcAlarmTemplate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserOperationClaims",
                keyColumn: "Id",
                keyValue: new Guid("71aecea9-f407-4da1-9912-00f00fd0d879"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("5378d2a0-92d2-4ae2-9b88-69f4c0664782"));

            migrationBuilder.CreateTable(
                name: "PlcAlarmTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SmsTemplate = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    EmailSubjectTemplate = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    EmailBodyTemplate = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    DevicePrefix = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlcAlarmTemplates", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AuthenticatorType", "CreatedDate", "DeletedDate", "Email", "PasswordHash", "PasswordSalt", "UpdatedDate" },
                values: new object[] { new Guid("7ba1293b-c129-44bd-9f44-dfb898958998"), 0, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "narch@kodlama.io", new byte[] { 201, 223, 181, 232, 28, 235, 196, 16, 46, 254, 31, 47, 216, 158, 166, 147, 39, 2, 88, 109, 41, 201, 171, 212, 245, 118, 72, 192, 157, 114, 25, 16, 234, 131, 238, 117, 17, 133, 20, 41, 195, 64, 185, 71, 155, 20, 31, 89, 154, 81, 112, 160, 69, 198, 208, 217, 243, 177, 28, 159, 223, 220, 101, 217 }, new byte[] { 40, 123, 34, 89, 11, 122, 71, 162, 166, 59, 123, 219, 170, 186, 212, 145, 132, 224, 45, 104, 114, 141, 158, 46, 61, 114, 134, 212, 170, 191, 46, 199, 48, 116, 207, 242, 36, 120, 233, 20, 221, 98, 124, 68, 136, 231, 230, 138, 134, 217, 17, 210, 255, 115, 99, 69, 191, 206, 28, 23, 177, 193, 85, 132, 74, 63, 140, 102, 47, 171, 216, 173, 232, 197, 83, 235, 116, 79, 246, 43, 170, 232, 139, 136, 152, 172, 136, 118, 183, 159, 77, 203, 248, 64, 158, 38, 211, 167, 206, 164, 9, 184, 170, 73, 38, 173, 222, 46, 126, 137, 172, 51, 146, 234, 179, 193, 252, 132, 172, 26, 215, 247, 225, 42, 5, 234, 58, 146 }, null });

            migrationBuilder.InsertData(
                table: "UserOperationClaims",
                columns: new[] { "Id", "CreatedDate", "DeletedDate", "OperationClaimId", "UpdatedDate", "UserId" },
                values: new object[] { new Guid("d683c9cc-e369-4654-843e-04d6cbc967ea"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, null, new Guid("7ba1293b-c129-44bd-9f44-dfb898958998") });

            migrationBuilder.CreateIndex(
                name: "IX_BagMovements_FromBagCellId",
                table: "BagMovements",
                column: "FromBagCellId");

            migrationBuilder.CreateIndex(
                name: "IX_BagMovements_ToBagCellId",
                table: "BagMovements",
                column: "ToBagCellId");

            migrationBuilder.AddForeignKey(
                name: "FK_BagMovements_BagCells_FromBagCellId",
                table: "BagMovements",
                column: "FromBagCellId",
                principalTable: "BagCells",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_BagMovements_BagCells_ToBagCellId",
                table: "BagMovements",
                column: "ToBagCellId",
                principalTable: "BagCells",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BagMovements_BagCells_FromBagCellId",
                table: "BagMovements");

            migrationBuilder.DropForeignKey(
                name: "FK_BagMovements_BagCells_ToBagCellId",
                table: "BagMovements");

            migrationBuilder.DropTable(
                name: "PlcAlarmTemplates");

            migrationBuilder.DropIndex(
                name: "IX_BagMovements_FromBagCellId",
                table: "BagMovements");

            migrationBuilder.DropIndex(
                name: "IX_BagMovements_ToBagCellId",
                table: "BagMovements");

            migrationBuilder.DeleteData(
                table: "UserOperationClaims",
                keyColumn: "Id",
                keyValue: new Guid("d683c9cc-e369-4654-843e-04d6cbc967ea"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("7ba1293b-c129-44bd-9f44-dfb898958998"));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AuthenticatorType", "CreatedDate", "DeletedDate", "Email", "PasswordHash", "PasswordSalt", "UpdatedDate" },
                values: new object[] { new Guid("5378d2a0-92d2-4ae2-9b88-69f4c0664782"), 0, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "narch@kodlama.io", new byte[] { 253, 212, 215, 124, 176, 49, 227, 188, 25, 228, 120, 13, 138, 145, 220, 215, 21, 108, 154, 204, 139, 137, 152, 250, 136, 113, 245, 170, 10, 137, 63, 30, 25, 99, 100, 51, 32, 240, 80, 212, 238, 147, 238, 96, 52, 58, 149, 119, 232, 62, 156, 52, 61, 230, 45, 193, 94, 165, 187, 174, 227, 173, 233, 31 }, new byte[] { 210, 136, 47, 76, 182, 213, 39, 250, 116, 232, 3, 64, 131, 132, 124, 218, 148, 45, 123, 151, 60, 239, 241, 194, 101, 197, 20, 208, 125, 139, 160, 228, 4, 110, 109, 177, 168, 55, 200, 45, 75, 124, 253, 132, 242, 6, 50, 79, 189, 213, 23, 175, 131, 33, 79, 116, 46, 16, 51, 51, 22, 69, 23, 86, 239, 207, 3, 162, 184, 117, 26, 96, 15, 208, 246, 47, 174, 177, 75, 231, 137, 64, 204, 130, 61, 168, 109, 108, 46, 13, 108, 212, 68, 236, 221, 232, 65, 198, 138, 228, 45, 158, 214, 29, 10, 144, 3, 58, 227, 119, 209, 106, 137, 240, 42, 175, 97, 98, 250, 3, 196, 159, 207, 10, 53, 226, 86, 164 }, null });

            migrationBuilder.InsertData(
                table: "UserOperationClaims",
                columns: new[] { "Id", "CreatedDate", "DeletedDate", "OperationClaimId", "UpdatedDate", "UserId" },
                values: new object[] { new Guid("71aecea9-f407-4da1-9912-00f00fd0d879"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, null, new Guid("5378d2a0-92d2-4ae2-9b88-69f4c0664782") });
        }
    }
}
