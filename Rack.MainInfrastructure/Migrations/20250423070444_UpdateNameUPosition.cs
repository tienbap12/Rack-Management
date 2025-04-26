using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rack.MainInfrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateNameUPosition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PositionInRack",
                table: "Devices",
                newName: "UPosition");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UPosition",
                table: "Devices",
                newName: "PositionInRack");
        }
    }
}
