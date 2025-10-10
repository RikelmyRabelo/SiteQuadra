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
    
    // NOVO MÉTODO ABAIXO
    // PUT: api/agendamentos/5
    // Rota para atualizar um agendamento existente
    [HttpPut("{id}")]
    public async Task<IActionResult> PutAgendamento(int id, Agendamento agendamento)
    {
        if (id != agendamento.Id)
        {
            return BadRequest(); // Retorna erro se o ID da URL for diferente do ID do objeto
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
                return NotFound(); // Retorna erro se o agendamento não existir mais
            }
            else
            {
                throw;
            }
        }

        return NoContent(); // Retorna sucesso sem conteúdo
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