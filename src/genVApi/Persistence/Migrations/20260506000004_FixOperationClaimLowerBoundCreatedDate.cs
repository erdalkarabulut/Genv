using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixOperationClaimLowerBoundCreatedDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE ""OperationClaims""
                SET ""CreatedDate"" = '2000-01-01 00:00:00+00'::timestamptz
                WHERE ""CreatedDate"" < '0001-01-01 00:00:00+00'::timestamptz;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No-op data correction migration.
        }
    }
}
