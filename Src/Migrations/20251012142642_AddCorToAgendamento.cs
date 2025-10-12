using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SiteQuadra.Migrations
{
    /// <inheritdoc />
    public partial class AddCorToAgendamento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Telefone",
                table: "Agendamentos",
                newName: "Cor");

            migrationBuilder.RenameColumn(
                name: "Cidade",
                table: "Agendamentos",
                newName: "Contato");

            migrationBuilder.AddColumn<string>(
                name: "CidadeBairro",
                table: "Agendamentos",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CidadeBairro",
                table: "Agendamentos");

            migrationBuilder.RenameColumn(
                name: "Cor",
                table: "Agendamentos",
                newName: "Telefone");

            migrationBuilder.RenameColumn(
                name: "Contato",
                table: "Agendamentos",
                newName: "Cidade");
        }
    }
}
