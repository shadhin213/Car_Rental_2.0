using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarRentalManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddRoleToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("IF COL_LENGTH('Users','Role') IS NULL ALTER TABLE [Users] ADD [Role] NVARCHAR(20) NOT NULL DEFAULT N'Customer'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("IF COL_LENGTH('Users','Role') IS NOT NULL ALTER TABLE [Users] DROP COLUMN [Role]");
        }
    }
}
