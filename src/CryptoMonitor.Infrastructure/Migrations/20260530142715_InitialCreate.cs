using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CryptoMonitor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Assets",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Symbol = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    PriceUsd = table.Column<decimal>(type: "TEXT", precision: 28, scale: 10, nullable: false),
                    MarketCapUsd = table.Column<decimal>(type: "TEXT", precision: 28, scale: 10, nullable: true),
                    VolumeUsd24Hr = table.Column<decimal>(type: "TEXT", precision: 28, scale: 10, nullable: true),
                    ChangePercent24Hr = table.Column<decimal>(type: "TEXT", precision: 10, scale: 4, nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PriceHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AssetId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    PriceUsd = table.Column<decimal>(type: "TEXT", precision: 28, scale: 10, nullable: false),
                    RecordedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PriceHistories_Assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PriceHistories_AssetId_RecordedAt",
                table: "PriceHistories",
                columns: new[] { "AssetId", "RecordedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PriceHistories");

            migrationBuilder.DropTable(
                name: "Assets");
        }
    }
}
