using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAllMissingOperationClaims : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var insertStatements = @"
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (42, '0001-01-01 00:00:00', 'Bags.Admin', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (43, '0001-01-01 00:00:00', 'Bags.Read', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (44, '0001-01-01 00:00:00', 'Bags.Write', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (45, '0001-01-01 00:00:00', 'Bags.Create', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (46, '0001-01-01 00:00:00', 'Bags.Update', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (47, '0001-01-01 00:00:00', 'Bags.Delete', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (48, '0001-01-01 00:00:00', 'Bags.CreateRange', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (49, '0001-01-01 00:00:00', 'Bags.UpdateRange', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (50, '0001-01-01 00:00:00', 'Bags.DeleteRange', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (51, '0001-01-01 00:00:00', 'BagMovements.Admin', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (52, '0001-01-01 00:00:00', 'BagMovements.Read', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (53, '0001-01-01 00:00:00', 'BagMovements.Write', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (54, '0001-01-01 00:00:00', 'BagMovements.Create', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (55, '0001-01-01 00:00:00', 'BagMovements.Update', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (56, '0001-01-01 00:00:00', 'BagMovements.Delete', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (57, '0001-01-01 00:00:00', 'BagMovements.CreateRange', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (58, '0001-01-01 00:00:00', 'BagMovements.UpdateRange', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (59, '0001-01-01 00:00:00', 'BagMovements.DeleteRange', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (60, '0001-01-01 00:00:00', 'Boxes.Admin', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (61, '0001-01-01 00:00:00', 'Boxes.Read', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (62, '0001-01-01 00:00:00', 'Boxes.Write', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (63, '0001-01-01 00:00:00', 'Boxes.Create', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (64, '0001-01-01 00:00:00', 'Boxes.Update', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (65, '0001-01-01 00:00:00', 'Boxes.Delete', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (66, '0001-01-01 00:00:00', 'Boxes.CreateRange', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (67, '0001-01-01 00:00:00', 'Boxes.UpdateRange', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (68, '0001-01-01 00:00:00', 'Boxes.DeleteRange', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (69, '0001-01-01 00:00:00', 'CollectionSessions.Admin', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (70, '0001-01-01 00:00:00', 'CollectionSessions.Read', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (71, '0001-01-01 00:00:00', 'CollectionSessions.Write', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (72, '0001-01-01 00:00:00', 'CollectionSessions.Create', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (73, '0001-01-01 00:00:00', 'CollectionSessions.Update', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (74, '0001-01-01 00:00:00', 'CollectionSessions.Delete', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (75, '0001-01-01 00:00:00', 'CollectionSessions.CreateRange', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (76, '0001-01-01 00:00:00', 'CollectionSessions.UpdateRange', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (77, '0001-01-01 00:00:00', 'CollectionSessions.DeleteRange', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (78, '0001-01-01 00:00:00', 'DliProducts.Admin', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (79, '0001-01-01 00:00:00', 'DliProducts.Read', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (80, '0001-01-01 00:00:00', 'DliProducts.Write', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (81, '0001-01-01 00:00:00', 'DliProducts.Create', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (82, '0001-01-01 00:00:00', 'DliProducts.Update', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (83, '0001-01-01 00:00:00', 'DliProducts.Delete', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (84, '0001-01-01 00:00:00', 'DliProducts.CreateRange', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (85, '0001-01-01 00:00:00', 'DliProducts.UpdateRange', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (86, '0001-01-01 00:00:00', 'DliProducts.DeleteRange', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (87, '0001-01-01 00:00:00', 'Donors.Admin', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (88, '0001-01-01 00:00:00', 'Donors.Read', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (89, '0001-01-01 00:00:00', 'Donors.Write', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (90, '0001-01-01 00:00:00', 'Donors.Create', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (91, '0001-01-01 00:00:00', 'Donors.Update', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (92, '0001-01-01 00:00:00', 'Donors.Delete', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (93, '0001-01-01 00:00:00', 'Donors.CreateRange', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (94, '0001-01-01 00:00:00', 'Donors.UpdateRange', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (95, '0001-01-01 00:00:00', 'Donors.DeleteRange', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (96, '0001-01-01 00:00:00', 'Patients.Admin', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (97, '0001-01-01 00:00:00', 'Patients.Read', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (98, '0001-01-01 00:00:00', 'Patients.Write', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (99, '0001-01-01 00:00:00', 'Patients.Create', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (100, '0001-01-01 00:00:00', 'Patients.Update', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (101, '0001-01-01 00:00:00', 'Patients.Delete', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (102, '0001-01-01 00:00:00', 'Patients.CreateRange', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (103, '0001-01-01 00:00:00', 'Patients.UpdateRange', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (104, '0001-01-01 00:00:00', 'Patients.DeleteRange', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (105, '0001-01-01 00:00:00', 'Products.Admin', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (106, '0001-01-01 00:00:00', 'Products.Read', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (107, '0001-01-01 00:00:00', 'Products.Write', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (108, '0001-01-01 00:00:00', 'Products.Create', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (109, '0001-01-01 00:00:00', 'Products.Update', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (110, '0001-01-01 00:00:00', 'Products.Delete', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (111, '0001-01-01 00:00:00', 'Products.CreateRange', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (112, '0001-01-01 00:00:00', 'Products.UpdateRange', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (113, '0001-01-01 00:00:00', 'Products.DeleteRange', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (114, '0001-01-01 00:00:00', 'Racks.Admin', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (115, '0001-01-01 00:00:00', 'Racks.Read', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (116, '0001-01-01 00:00:00', 'Racks.Write', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (117, '0001-01-01 00:00:00', 'Racks.Create', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (118, '0001-01-01 00:00:00', 'Racks.Update', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (119, '0001-01-01 00:00:00', 'Racks.Delete', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (120, '0001-01-01 00:00:00', 'Racks.CreateRange', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (121, '0001-01-01 00:00:00', 'Racks.UpdateRange', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (122, '0001-01-01 00:00:00', 'Racks.DeleteRange', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (123, '0001-01-01 00:00:00', 'Slots.Admin', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (124, '0001-01-01 00:00:00', 'Slots.Read', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (125, '0001-01-01 00:00:00', 'Slots.Write', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (126, '0001-01-01 00:00:00', 'Slots.Create', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (127, '0001-01-01 00:00:00', 'Slots.Update', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (128, '0001-01-01 00:00:00', 'Slots.Delete', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (129, '0001-01-01 00:00:00', 'Slots.CreateRange', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (130, '0001-01-01 00:00:00', 'Slots.UpdateRange', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (131, '0001-01-01 00:00:00', 'Slots.DeleteRange', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (132, '0001-01-01 00:00:00', 'Tanks.Admin', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (133, '0001-01-01 00:00:00', 'Tanks.Read', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (134, '0001-01-01 00:00:00', 'Tanks.Write', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (135, '0001-01-01 00:00:00', 'Tanks.Create', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (136, '0001-01-01 00:00:00', 'Tanks.Update', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (137, '0001-01-01 00:00:00', 'Tanks.Delete', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (138, '0001-01-01 00:00:00', 'Tanks.CreateRange', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (139, '0001-01-01 00:00:00', 'Tanks.UpdateRange', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (140, '0001-01-01 00:00:00', 'Tanks.DeleteRange', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (141, '0001-01-01 00:00:00', 'ClinicalSettings.Admin', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (142, '0001-01-01 00:00:00', 'ClinicalSettings.Read', NULL, NULL);
                INSERT INTO ""OperationClaims"" (""Id"", ""CreatedDate"", ""Name"", ""UpdatedDate"", ""DeletedDate"") VALUES (143, '0001-01-01 00:00:00', 'ClinicalSettings.Write', NULL, NULL);
            ";

            migrationBuilder.Sql(insertStatements);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var deleteStatements = @"
                DELETE FROM ""OperationClaims"" WHERE ""Id"" >= 42 AND ""Id"" <= 143;
            ";

            migrationBuilder.Sql(deleteStatements);
        }
    }
}

