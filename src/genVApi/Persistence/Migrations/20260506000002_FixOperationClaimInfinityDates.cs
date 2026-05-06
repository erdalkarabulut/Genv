using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixOperationClaimInfinityDates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE ""OperationClaims""
                SET ""UpdatedDate"" = NULL
                WHERE ""UpdatedDate"" = '-infinity'::timestamp with time zone;

                UPDATE ""OperationClaims""
                SET ""DeletedDate"" = NULL
                WHERE ""DeletedDate"" = '-infinity'::timestamp with time zone;

                UPDATE ""OperationClaims""
                SET ""CreatedDate"" = '0001-01-01 00:00:00+00'::timestamp with time zone
                WHERE ""CreatedDate"" = '-infinity'::timestamp with time zone;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No-op: this migration repairs invalid timestamp values.
        }
    }
}
