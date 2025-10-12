using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SiteQuadra.Migrations
{
    /// <inheritdoc />
    public partial class AddTelefoneECidadeToAgendamento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Cidade",
                table: "Agendamentos",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Telefone",
                table: "Agendamentos",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cidade",
                table: "Agendamentos");

            migrationBuilder.DropColumn(
                name: "Telefone",
                table: "Agendamentos");
        }
    }
}
