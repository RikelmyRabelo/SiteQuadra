using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SiteQuadra.Data;
using SiteQuadra.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

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

    // GET: api/agendamentos
    // Rota para buscar todos os agendamentos
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Agendamento>>> GetAgendamentos()
    {
        return await _context.Agendamentos.ToListAsync();
    }

    // POST: api/agendamentos
    // Rota para criar um novo agendamento
    [HttpPost]
    public async Task<ActionResult<Agendamento>> PostAgendamento(Agendamento agendamento)
    {
        _context.Agendamentos.Add(agendamento);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAgendamentos), new { id = agendamento.Id }, agendamento);
    }

    // DELETE: api/agendamentos/5
    // Rota para deletar um agendamento específico pelo ID
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