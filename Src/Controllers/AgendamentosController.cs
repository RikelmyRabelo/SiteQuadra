using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SiteQuadra.Data;
using SiteQuadra.Models;
using System.Linq; 

namespace SiteQuadra.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AgendamentosController : ControllerBase
{
    private readonly QuadraContext _context;

    public AgendamentosController(QuadraContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Agendamento>>> GetAgendamentos()
    {
        return await _context.Agendamentos.ToListAsync();
    }

    [HttpPost]
    public IActionResult Post([FromBody] Agendamento agendamento)
    {
        // Garante que DataHoraFim seja sempre 1 hora após DataHoraInicio
        agendamento.DataHoraFim = agendamento.DataHoraInicio.AddHours(1);
        
        // Garante cor padrão se estiver vazia
        if (string.IsNullOrWhiteSpace(agendamento.Cor))
        {
            agendamento.Cor = "#3788d8";
        }
        
        // Busca agendamentos que colidem
        var existeConflito = _context.Agendamentos.Any(a =>
            a.DataHoraInicio.Date == agendamento.DataHoraInicio.Date && // Mesmo dia
            (
                (agendamento.DataHoraInicio < a.DataHoraFim) && // Início do novo dentro de outro
                (agendamento.DataHoraFim > a.DataHoraInicio)    // Fim do novo dentro de outro
            )
        );

        if (existeConflito)
        {
            return Conflict("Já existe um agendamento que colide com este horário.");
        }

        // Salva normalmente
        _context.Agendamentos.Add(agendamento);
        _context.SaveChanges();

        return Ok(agendamento);
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> PutAgendamento(int id, Agendamento agendamento)
    {
        if (id != agendamento.Id)
        {
            return BadRequest();
        }
        
        agendamento.DataHoraFim = agendamento.DataHoraInicio.AddHours(1);
        
        // Garante cor padrão se estiver vazia
        if (string.IsNullOrWhiteSpace(agendamento.Cor))
        {
            agendamento.Cor = "#3788d8";
        }


        _context.Entry(agendamento).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Agendamentos.Any(e => e.Id == id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAgendamento(int id)
    {
        var agendamento = await _context.Agendamentos.FindAsync(id);
        if (agendamento == null)
        {
            return NotFound();
        }

        _context.Agendamentos.Remove(agendamento);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}