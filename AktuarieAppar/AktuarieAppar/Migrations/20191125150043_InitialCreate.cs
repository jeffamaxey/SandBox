using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AktuarieAppar.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TAT",
                columns: table => new
                {
                    DWFromDate = table.Column<DateTime>(nullable: false),
                    TATDate = table.Column<DateTime>(nullable: false),
                    AssetClass = table.Column<string>(nullable: false),
                    MV_SEK = table.Column<decimal>(nullable: false),
                    AssetAllocation = table.Column<double>(nullable: false),
                    RETURN_SEK_YTD = table.Column<decimal>(nullable: false),
                    SavedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TAT", x => new { x.DWFromDate, x.TATDate, x.AssetClass });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TAT");
        }
    }
}
