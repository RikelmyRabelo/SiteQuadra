using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SiteQuadra.Migrations
{
    /// <inheritdoc />
    public partial class InitialModelV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cor",
                table: "Agendamentos");

            migrationBuilder.DropColumn(
                name: "Observacoes",
                table: "Agendamentos");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Cor",
                table: "Agendamentos",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Observacoes",
                table: "Agendamentos",
                type: "TEXT",
                nullable: true);
        }
    }
}
