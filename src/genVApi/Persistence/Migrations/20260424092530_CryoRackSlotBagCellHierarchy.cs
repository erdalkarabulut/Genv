using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CryoRackSlotBagCellHierarchy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserOperationClaims",
                keyColumn: "Id",
                keyValue: new Guid("1526e8bd-347c-4986-b3de-40cc1da33d83"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("8bcfdd9f-640e-45f6-a5a4-c153eadd9718"));

            migrationBuilder.DropForeignKey(
                name: "FK_Bags_Slots_SlotId",
                table: "Bags");

            migrationBuilder.DropForeignKey(
                name: "FK_Slots_Boxes_BoxId",
                table: "Slots");

            migrationBuilder.DropForeignKey(
                name: "FK_Boxes_Racks_RackId",
                table: "Boxes");

            migrationBuilder.DropIndex(
                name: "IX_Bags_SlotId",
                table: "Bags");

            // PK ve BoxId+Position indeksini düşürme: tablo yeniden adlandırıldıktan sonra yeniden adlandırılacak.
            // (Önceden DropPrimaryKey kullanılıyordu; bu PK_Slots'u sildiği için RENAME CONSTRAINT hata veriyordu.)

            migrationBuilder.RenameTable(
                name: "Slots",
                newName: "BagCells");

            migrationBuilder.Sql(
                """
                DO $ef$
                BEGIN
                  IF EXISTS (
                    SELECT 1 FROM pg_constraint c
                    JOIN pg_class t ON c.conrelid = t.oid
                    JOIN pg_namespace n ON t.relnamespace = n.oid
                    WHERE n.nspname = 'public' AND t.relname = 'BagCells' AND c.conname = 'PK_Slots' AND c.contype = 'p'
                  ) THEN
                    ALTER TABLE "BagCells" RENAME CONSTRAINT "PK_Slots" TO "PK_BagCells";
                  ELSIF NOT EXISTS (
                    SELECT 1 FROM pg_constraint c
                    JOIN pg_class t ON c.conrelid = t.oid
                    JOIN pg_namespace n ON t.relnamespace = n.oid
                    WHERE n.nspname = 'public' AND t.relname = 'BagCells' AND c.contype = 'p'
                  ) THEN
                    ALTER TABLE "BagCells" ADD CONSTRAINT "PK_BagCells" PRIMARY KEY ("Id");
                  END IF;
                END $ef$;

                DO $ef$
                BEGIN
                  IF EXISTS (
                    SELECT 1 FROM pg_class i
                    JOIN pg_namespace n ON i.relnamespace = n.oid
                    WHERE n.nspname = 'public' AND i.relkind = 'i' AND i.relname = 'IX_Slots_BoxId_Position'
                  ) THEN
                    ALTER INDEX "IX_Slots_BoxId_Position" RENAME TO "IX_BagCells_BoxId_Position";
                  ELSIF NOT EXISTS (
                    SELECT 1 FROM pg_class i
                    JOIN pg_namespace n ON i.relnamespace = n.oid
                    WHERE n.nspname = 'public' AND i.relkind = 'i' AND i.relname = 'IX_BagCells_BoxId_Position'
                  ) THEN
                    CREATE UNIQUE INDEX "IX_BagCells_BoxId_Position" ON "BagCells" ("BoxId", "Position");
                  END IF;
                END $ef$;
                """);

            migrationBuilder.CreateTable(
                name: "RackSlots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RackId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RackSlots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RackSlots_Racks_RackId",
                        column: x => x.RackId,
                        principalTable: "Racks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddColumn<Guid>(
                name: "SlotId",
                table: "Boxes",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql(
                """
                INSERT INTO "RackSlots" ("Id", "RackId", "Name", "CreatedDate", "UpdatedDate", "DeletedDate")
                SELECT gen_random_uuid(), b."RackId", '__b' || replace(b."Id"::text, '-', ''), b."CreatedDate", b."UpdatedDate", b."DeletedDate"
                FROM "Boxes" b;

                UPDATE "Boxes" b
                SET "SlotId" = rs."Id"
                FROM "RackSlots" rs
                WHERE rs."Name" = '__b' || replace(b."Id"::text, '-', '');
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "SlotId",
                table: "Boxes",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.DropIndex(
                name: "IX_Boxes_RackId_Name",
                table: "Boxes");

            migrationBuilder.DropColumn(
                name: "RackId",
                table: "Boxes");

            migrationBuilder.RenameColumn(
                name: "SlotId",
                table: "Bags",
                newName: "BagCellId");

            migrationBuilder.RenameColumn(
                name: "ToSlotId",
                table: "BagMovements",
                newName: "ToBagCellId");

            migrationBuilder.RenameColumn(
                name: "FromSlotId",
                table: "BagMovements",
                newName: "FromBagCellId");

            migrationBuilder.CreateIndex(
                name: "IX_Bags_BagCellId",
                table: "Bags",
                column: "BagCellId",
                unique: true,
                filter: "\"BagCellId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Boxes_SlotId_Name",
                table: "Boxes",
                columns: new[] { "SlotId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RackSlots_RackId_Name",
                table: "RackSlots",
                columns: new[] { "RackId", "Name" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Bags_BagCells_BagCellId",
                table: "Bags",
                column: "BagCellId",
                principalTable: "BagCells",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Boxes_RackSlots_SlotId",
                table: "Boxes",
                column: "SlotId",
                principalTable: "RackSlots",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BagCells_Boxes_BoxId",
                table: "BagCells",
                column: "BoxId",
                principalTable: "Boxes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AuthenticatorType", "CreatedDate", "DeletedDate", "Email", "PasswordHash", "PasswordSalt", "UpdatedDate" },
                values: new object[] { new Guid("eb180b88-f92d-4bc1-a351-59778f738a03"), 0, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "narch@kodlama.io", new byte[] { 236, 194, 243, 138, 13, 62, 59, 181, 83, 6, 213, 222, 65, 250, 112, 165, 134, 91, 94, 126, 160, 159, 145, 42, 203, 144, 251, 73, 166, 200, 206, 97, 2, 175, 233, 197, 237, 7, 92, 106, 154, 134, 161, 205, 168, 1, 246, 28, 13, 4, 188, 34, 204, 172, 93, 237, 66, 219, 237, 200, 204, 163, 105, 180 }, new byte[] { 245, 99, 218, 93, 0, 27, 240, 124, 40, 222, 195, 81, 21, 138, 253, 254, 70, 99, 77, 60, 226, 46, 88, 4, 136, 202, 5, 46, 14, 197, 193, 106, 145, 5, 81, 116, 103, 228, 10, 82, 217, 99, 231, 79, 57, 152, 205, 18, 173, 161, 173, 223, 19, 182, 32, 21, 236, 153, 232, 40, 145, 182, 197, 149, 62, 75, 115, 55, 161, 212, 144, 201, 238, 218, 47, 36, 236, 61, 16, 215, 103, 21, 181, 171, 57, 189, 238, 122, 0, 32, 118, 162, 132, 129, 11, 174, 154, 188, 184, 29, 133, 218, 74, 7, 53, 1, 120, 247, 223, 17, 110, 251, 80, 75, 99, 31, 144, 1, 138, 136, 237, 95, 127, 200, 67, 28, 170, 93 }, null });

            migrationBuilder.InsertData(
                table: "UserOperationClaims",
                columns: new[] { "Id", "CreatedDate", "DeletedDate", "OperationClaimId", "UpdatedDate", "UserId" },
                values: new object[] { new Guid("b48afbfc-1fe8-4a62-b24a-a66ab29b9cde"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, null, new Guid("eb180b88-f92d-4bc1-a351-59778f738a03") });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserOperationClaims",
                keyColumn: "Id",
                keyValue: new Guid("b48afbfc-1fe8-4a62-b24a-a66ab29b9cde"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("eb180b88-f92d-4bc1-a351-59778f738a03"));

            migrationBuilder.DropForeignKey(
                name: "FK_Bags_BagCells_BagCellId",
                table: "Bags");

            migrationBuilder.DropForeignKey(
                name: "FK_Boxes_RackSlots_SlotId",
                table: "Boxes");

            migrationBuilder.DropForeignKey(
                name: "FK_BagCells_Boxes_BoxId",
                table: "BagCells");

            migrationBuilder.DropForeignKey(
                name: "FK_RackSlots_Racks_RackId",
                table: "RackSlots");

            migrationBuilder.DropIndex(
                name: "IX_Bags_BagCellId",
                table: "Bags");

            migrationBuilder.DropIndex(
                name: "IX_Boxes_SlotId_Name",
                table: "Boxes");

            migrationBuilder.DropIndex(
                name: "IX_RackSlots_RackId_Name",
                table: "RackSlots");

            migrationBuilder.AddColumn<Guid>(
                name: "RackId",
                table: "Boxes",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE "Boxes" b
                SET "RackId" = rs."RackId"
                FROM "RackSlots" rs
                WHERE b."SlotId" = rs."Id";
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "RackId",
                table: "Boxes",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.DropColumn(
                name: "SlotId",
                table: "Boxes");

            migrationBuilder.DropTable(
                name: "RackSlots");

            migrationBuilder.RenameColumn(
                name: "BagCellId",
                table: "Bags",
                newName: "SlotId");

            migrationBuilder.RenameColumn(
                name: "ToBagCellId",
                table: "BagMovements",
                newName: "ToSlotId");

            migrationBuilder.RenameColumn(
                name: "FromBagCellId",
                table: "BagMovements",
                newName: "FromSlotId");

            migrationBuilder.RenameTable(
                name: "BagCells",
                newName: "Slots");

            migrationBuilder.Sql(
                """
                ALTER TABLE "Slots" RENAME CONSTRAINT "PK_BagCells" TO "PK_Slots";
                ALTER INDEX "IX_BagCells_BoxId_Position" RENAME TO "IX_Slots_BoxId_Position";
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Boxes_RackId_Name",
                table: "Boxes",
                columns: new[] { "RackId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bags_SlotId",
                table: "Bags",
                column: "SlotId",
                unique: true,
                filter: "\"SlotId\" IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Bags_Slots_SlotId",
                table: "Bags",
                column: "SlotId",
                principalTable: "Slots",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Boxes_Racks_RackId",
                table: "Boxes",
                column: "RackId",
                principalTable: "Racks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Slots_Boxes_BoxId",
                table: "Slots",
                column: "BoxId",
                principalTable: "Boxes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AuthenticatorType", "CreatedDate", "DeletedDate", "Email", "PasswordHash", "PasswordSalt", "UpdatedDate" },
                values: new object[] { new Guid("8bcfdd9f-640e-45f6-a5a4-c153eadd9718"), 0, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "narch@kodlama.io", new byte[] { 23, 150, 255, 225, 162, 129, 114, 173, 176, 92, 70, 40, 232, 83, 81, 77, 30, 1, 164, 206, 16, 250, 40, 198, 163, 150, 234, 81, 187, 6, 222, 4, 24, 83, 251, 86, 253, 72, 238, 65, 236, 157, 165, 70, 215, 54, 161, 251, 28, 95, 167, 23, 20, 101, 24, 190, 195, 26, 197, 31, 8, 37, 86, 118 }, new byte[] { 125, 95, 10, 20, 178, 168, 112, 216, 242, 241, 221, 112, 154, 81, 110, 104, 66, 136, 111, 224, 201, 111, 184, 67, 9, 148, 12, 217, 149, 34, 199, 10, 99, 153, 67, 128, 169, 94, 0, 79, 196, 204, 166, 29, 13, 110, 196, 49, 138, 147, 135, 16, 26, 250, 115, 45, 193, 172, 148, 25, 218, 215, 79, 152, 59, 81, 209, 219, 111, 114, 184, 15, 249, 239, 71, 67, 48, 76, 94, 32, 135, 194, 108, 244, 68, 253, 167, 47, 151, 175, 101, 125, 235, 28, 130, 96, 41, 242, 106, 42, 194, 237, 135, 121, 168, 185, 118, 96, 238, 144, 203, 72, 209, 242, 93, 57, 250, 67, 123, 37, 194, 134, 49, 151, 35, 157, 195, 22 }, null });

            migrationBuilder.InsertData(
                table: "UserOperationClaims",
                columns: new[] { "Id", "CreatedDate", "DeletedDate", "OperationClaimId", "UpdatedDate", "UserId" },
                values: new object[] { new Guid("1526e8bd-347c-4986-b3de-40cc1da33d83"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, null, new Guid("8bcfdd9f-640e-45f6-a5a4-c153eadd9718") });
        }
    }
}
