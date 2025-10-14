using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SiteQuadra.Data;
using SiteQuadra.Models;
using Microsoft.Extensions.Configuration;
using SiteQuadra.Services;

namespace SiteQuadra.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly QuadraContext _context;
    private readonly IAdminSecurityService _adminSecurity;

    public AdminController(QuadraContext context, IAdminSecurityService adminSecurity)
    {
        _context = context;
        _adminSecurity = adminSecurity;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] AdminLoginRequest request)
    {
        var storedPasswordHash = await _adminSecurity.GetStoredPasswordHashAsync();
        
        if (storedPasswordHash == null)
        {
            return StatusCode(500, new { message = "Sistema de autenticação não inicializado" });
        }
        
        if (_adminSecurity.VerifyPassword(request.Password, storedPasswordHash))
        {
            var secureToken = _adminSecurity.GenerateSecureToken();
            _adminSecurity.StoreToken(secureToken);
            return Ok(new { token = secureToken, message = "Login realizado com sucesso" });
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