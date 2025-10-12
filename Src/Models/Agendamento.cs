namespace SiteQuadra.Models;

public class Agendamento
{
    public int Id { get; set; }
    public string NomeResponsavel { get; set; } = string.Empty;
    public string Contato { get; set; } = string.Empty;        
    public string CidadeBairro { get; set; } = string.Empty;    
    public DateTime DataHoraInicio { get; set; }
    public DateTime DataHoraFim { get; set; }
}