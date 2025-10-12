namespace SiteQuadra.Models;

public class Agendamento
{
    public int Id { get; set; }
    public string NomeResponsavel { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty; // NOVO CAMPO
    public string Cidade { get; set; } = string.Empty;   // NOVO CAMPO
    public DateTime DataHoraInicio { get; set; }
    public DateTime DataHoraFim { get; set; }
}