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
    public async Task<ActionResult<Agendamento>> PostAgendamento(Agendamento agendamento)
    {
        // Regra de negócio: Duração de 1 hora
        agendamento.DataHoraFim = agendamento.DataHoraInicio.AddHours(1);

        var haConflito = await _context.Agendamentos
            .AnyAsync(a => 
                (agendamento.DataHoraInicio < a.DataHoraFim) && (agendamento.DataHoraFim > a.DataHoraInicio)
            );

        if (haConflito)
        {
            // Retorna um erro específico (409 Conflict) se o horário já estiver ocupado.
            return Conflict("Este horário já está reservado.");
        }

        _context.Agendamentos.Add(agendamento);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAgendamentos), new { id = agendamento.Id }, agendamento);
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> PutAgendamento(int id, Agendamento agendamento)
    {
        if (id != agendamento.Id)
        {
            return BadRequest();
        }
        
        agendamento.DataHoraFim = agendamento.DataHoraInicio.AddHours(1);


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