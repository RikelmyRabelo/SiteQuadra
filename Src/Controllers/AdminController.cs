using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SiteQuadra.Data;
using SiteQuadra.Models;
using Microsoft.Extensions.Configuration;

namespace SiteQuadra.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly QuadraContext _context;
    private readonly string _adminPassword;

    public AdminController(QuadraContext context, IConfiguration configuration)
    {
        _context = context;
        _adminPassword = configuration["AdminPassword"] ?? "admin123";
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] AdminLoginRequest request)
    {
        if (request.Password == _adminPassword)
        {
            return Ok(new { token = _adminPassword, message = "Login realizado com sucesso" });
        }
        
        return Unauthorized(new { message = "Senha incorreta" });
    }

    [HttpGet("agendamentos")]
    public async Task<ActionResult<IEnumerable<object>>> GetAllAgendamentos()
    {
        var agendamentos = await _context.Agendamentos
            .OrderBy(a => a.DataHoraInicio)
            .Select(a => new
            {
                a.Id,
                a.NomeResponsavel,
                a.Contato,
                a.CidadeBairro,
                a.DataHoraInicio,
                a.DataHoraFim,
                a.Cor,
                DataFormatada = a.DataHoraInicio.ToString("dd/MM/yyyy"),
                HorarioFormatado = a.DataHoraInicio.ToString("HH:mm") + " - " + a.DataHoraFim.ToString("HH:mm")
            })
            .ToListAsync();
        
        return Ok(agendamentos);
    }

    [HttpDelete("agendamentos/{id}")]
    public async Task<IActionResult> DeleteAgendamento(int id)
    {
        var agendamento = await _context.Agendamentos.FindAsync(id);
        if (agendamento == null)
        {
            return NotFound(new { message = "Agendamento não encontrado" });
        }

        _context.Agendamentos.Remove(agendamento);
        await _context.SaveChangesAsync();

        return Ok(new { 
            message = "Agendamento removido com sucesso",
            agendamentoRemovido = new
            {
                agendamento.Id,
                agendamento.NomeResponsavel,
                agendamento.DataHoraInicio,
                DataFormatada = agendamento.DataHoraInicio.ToString("dd/MM/yyyy"),
                HorarioFormatado = agendamento.DataHoraInicio.ToString("HH:mm") + " - " + agendamento.DataHoraFim.ToString("HH:mm")
            }
        });
    }

    [HttpGet("estatisticas")]
    public async Task<ActionResult<object>> GetEstatisticas()
    {
        var hoje = DateTime.Today;
        var inicioSemana = hoje.AddDays(-(int)hoje.DayOfWeek);
        var fimSemana = inicioSemana.AddDays(6);

        var totalAgendamentos = await _context.Agendamentos.CountAsync();
        var agendamentosSemana = await _context.Agendamentos
            .CountAsync(a => a.DataHoraInicio.Date >= inicioSemana && a.DataHoraInicio.Date <= fimSemana);
        
        var agendamentosHoje = await _context.Agendamentos
            .CountAsync(a => a.DataHoraInicio.Date == hoje);

        return Ok(new
        {
            TotalAgendamentos = totalAgendamentos,
            AgendamentosSemana = agendamentosSemana,
            AgendamentosHoje = agendamentosHoje
        });
    }
}

public class AdminLoginRequest
{
    public string Password { get; set; } = string.Empty;
}