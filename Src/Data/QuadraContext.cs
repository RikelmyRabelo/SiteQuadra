using Microsoft.EntityFrameworkCore;
using SiteQuadra.Models;

namespace SiteQuadra.Data;

public class QuadraContext : DbContext
{
    public QuadraContext(DbContextOptions<QuadraContext> options) : base(options)
    {
    }

    public DbSet<Agendamento> Agendamentos { get; set; }
}