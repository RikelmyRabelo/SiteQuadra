using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SiteQuadra.Migrations
{
    /// <inheritdoc />
    public partial class AddColorToAgendamento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Cor",
                table: "Agendamentos",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cor",
                table: "Agendamentos");
        }
    }
}
