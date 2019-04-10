using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BillionSongs.Data.Migrations
{
    public partial class CreateSongsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Songs",
                columns: table => new
                {
                    ID = table.Column<long>(nullable: false),
                    Title = table.Column<string>(maxLength: 129, nullable: true),
                    Lyrics = table.Column<string>(maxLength: 4096, nullable: true),
                    Generated = table.Column<DateTimeOffset>(nullable: false),
                    GeneratorError = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Songs", x => x.ID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Songs");
        }
    }
}
