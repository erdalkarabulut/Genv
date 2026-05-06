using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeOperationClaimTimestamps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE ""OperationClaims""
                SET ""CreatedDate"" = '0001-01-01 00:00:00+00'::timestamp with time zone
                WHERE NOT isfinite(""CreatedDate"");

                UPDATE ""OperationClaims""
                SET ""UpdatedDate"" = NULL
                WHERE ""UpdatedDate"" IS NOT NULL AND NOT isfinite(""UpdatedDate"");

                UPDATE ""OperationClaims""
                SET ""DeletedDate"" = NULL
                WHERE ""DeletedDate"" IS NOT NULL AND NOT isfinite(""DeletedDate"");
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No-op normalization migration.
        }
    }
}
