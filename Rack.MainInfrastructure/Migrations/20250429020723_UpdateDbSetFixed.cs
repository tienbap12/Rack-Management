using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rack.MainInfrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDbSetFixed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Card_Devices_DeviceID",
                table: "Card");

            migrationBuilder.DropForeignKey(
                name: "FK_Port_Card_CardID",
                table: "Port");

            migrationBuilder.DropForeignKey(
                name: "FK_Port_Devices_DeviceID",
                table: "Port");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Port",
                table: "Port");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Card",
                table: "Card");

            migrationBuilder.RenameTable(
                name: "Port",
                newName: "Ports");

            migrationBuilder.RenameTable(
                name: "Card",
                newName: "Cards");

            migrationBuilder.RenameIndex(
                name: "IX_Port_DeviceID",
                table: "Ports",
                newName: "IX_Ports_DeviceID");

            migrationBuilder.RenameIndex(
                name: "IX_Port_CardID",
                table: "Ports",
                newName: "IX_Ports_CardID");

            migrationBuilder.RenameIndex(
                name: "IX_Card_DeviceID",
                table: "Cards",
                newName: "IX_Cards_DeviceID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Ports",
                table: "Ports",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Cards",
                table: "Cards",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "PortConnections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourcePortID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DestinationPortID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CableType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PortConnections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PortConnections_Ports_DestinationPortID",
                        column: x => x.DestinationPortID,
                        principalTable: "Ports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PortConnections_Ports_SourcePortID",
                        column: x => x.SourcePortID,
                        principalTable: "Ports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PortConnections_DestinationPortID",
                table: "PortConnections",
                column: "DestinationPortID");

            migrationBuilder.CreateIndex(
                name: "IX_PortConnections_SourcePortID",
                table: "PortConnections",
                column: "SourcePortID");

            migrationBuilder.AddForeignKey(
                name: "FK_Cards_Devices_DeviceID",
                table: "Cards",
                column: "DeviceID",
                principalTable: "Devices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Ports_Cards_CardID",
                table: "Ports",
                column: "CardID",
                principalTable: "Cards",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Ports_Devices_DeviceID",
                table: "Ports",
                column: "DeviceID",
                principalTable: "Devices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cards_Devices_DeviceID",
                table: "Cards");

            migrationBuilder.DropForeignKey(
                name: "FK_Ports_Cards_CardID",
                table: "Ports");

            migrationBuilder.DropForeignKey(
                name: "FK_Ports_Devices_DeviceID",
                table: "Ports");

            migrationBuilder.DropTable(
                name: "PortConnections");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Ports",
                table: "Ports");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Cards",
                table: "Cards");

            migrationBuilder.RenameTable(
                name: "Ports",
                newName: "Port");

            migrationBuilder.RenameTable(
                name: "Cards",
                newName: "Card");

            migrationBuilder.RenameIndex(
                name: "IX_Ports_DeviceID",
                table: "Port",
                newName: "IX_Port_DeviceID");

            migrationBuilder.RenameIndex(
                name: "IX_Ports_CardID",
                table: "Port",
                newName: "IX_Port_CardID");

            migrationBuilder.RenameIndex(
                name: "IX_Cards_DeviceID",
                table: "Card",
                newName: "IX_Card_DeviceID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Port",
                table: "Port",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Card",
                table: "Card",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Card_Devices_DeviceID",
                table: "Card",
                column: "DeviceID",
                principalTable: "Devices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Port_Card_CardID",
                table: "Port",
                column: "CardID",
                principalTable: "Card",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Port_Devices_DeviceID",
                table: "Port",
                column: "DeviceID",
                principalTable: "Devices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
