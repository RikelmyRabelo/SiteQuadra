using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SiteQuadra.Controllers;
using SiteQuadra.Data;
using SiteQuadra.Models;
using Xunit;

namespace SiteQuadra.Tests;

public class AgendamentosControllerTests : IDisposable
{
    private readonly QuadraContext _context;
    private readonly AgendamentosController _controller;

    public AgendamentosControllerTests()
    {
        var options = new DbContextOptionsBuilder<QuadraContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new QuadraContext(options);
        _controller = new AgendamentosController(_context);
    }

    [Fact]
    public async Task GetAgendamentos_DeveRetornarListaVazia_QuandoNaoHaAgendamentos()
    {
        // Act
        var result = await _controller.GetAgendamentos();

        // Assert
        var actionResult = Assert.IsType<ActionResult<IEnumerable<Agendamento>>>(result);
        var agendamentos = Assert.IsAssignableFrom<IEnumerable<Agendamento>>(actionResult.Value);
        Assert.Empty(agendamentos);
    }

    [Fact]
    public async Task GetAgendamentos_DeveRetornarTodosAgendamentos()
    {
        // Arrange
        var agendamento1 = new Agendamento
        {
            NomeResponsavel = "João",
            Contato = "123456789",
            CidadeBairro = "Centro",
            DataHoraInicio = DateTime.Today.AddHours(10),
            DataHoraFim = DateTime.Today.AddHours(11)
        };
        
        var agendamento2 = new Agendamento
        {
            NomeResponsavel = "Maria",
            Contato = "987654321",
            CidadeBairro = "Liberdade",
            DataHoraInicio = DateTime.Today.AddHours(14),
            DataHoraFim = DateTime.Today.AddHours(15)
        };

        _context.Agendamentos.AddRange(agendamento1, agendamento2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetAgendamentos();

        // Assert
        var actionResult = Assert.IsType<ActionResult<IEnumerable<Agendamento>>>(result);
        var agendamentos = Assert.IsAssignableFrom<IEnumerable<Agendamento>>(actionResult.Value);
        Assert.Equal(2, agendamentos.Count());
    }

    [Fact]
    public void Post_ComAgendamentoValido_DeveRetornarOk()
    {
        // Arrange
        var agendamento = new Agendamento
        {
            NomeResponsavel = "João Silva",
            Contato = "(98) 99999-9999",
            CidadeBairro = "Monte Alegre",
            DataHoraInicio = DateTime.Today.AddDays(1).AddHours(10),
            DataHoraFim = DateTime.Today.AddDays(1).AddHours(10) // Será corrigido pelo controller
        };

        // Act
        var result = _controller.Post(agendamento);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var agendamentoSalvo = Assert.IsType<Agendamento>(okResult.Value);
        
        Assert.Equal("João Silva", agendamentoSalvo.NomeResponsavel);
        Assert.Equal("(98) 99999-9999", agendamentoSalvo.Contato);
        Assert.Equal("Monte Alegre", agendamentoSalvo.CidadeBairro);
        
        // Verificar se DataHoraFim foi calculado corretamente
        var expectedFim = agendamento.DataHoraInicio.AddHours(1);
        Assert.Equal(expectedFim, agendamentoSalvo.DataHoraFim);
        
        // Verificar se tem cor padrão
        Assert.Equal("#3788d8", agendamentoSalvo.Cor);
    }

    [Fact]
    public void Post_ComConflito_DeveRetornarConflict()
    {
        // Arrange - Criar agendamento existente
        var agendamentoExistente = new Agendamento
        {
            NomeResponsavel = "Maria",
            Contato = "111111111",
            CidadeBairro = "Centro",
            DataHoraInicio = DateTime.Today.AddDays(1).AddHours(14),
            DataHoraFim = DateTime.Today.AddDays(1).AddHours(15)
        };
        
        _context.Agendamentos.Add(agendamentoExistente);
        _context.SaveChanges();

        // Tentar criar conflitante
        var agendamentoConflitante = new Agendamento
        {
            NomeResponsavel = "João",
            Contato = "222222222",
            CidadeBairro = "Liberdade",
            DataHoraInicio = DateTime.Today.AddDays(1).AddHours(14), // Mesmo horário
            DataHoraFim = DateTime.Today.AddDays(1).AddHours(14)
        };

        // Act
        var result = _controller.Post(agendamentoConflitante);

        // Assert
        var conflictResult = Assert.IsType<ConflictObjectResult>(result);
        Assert.Equal("Já existe um agendamento que colide com este horário.", conflictResult.Value);
    }

    [Fact]
    public void Post_ComConflitoParcial_DeveRetornarConflict()
    {
        // Arrange - Agendamento das 14h às 15h
        var agendamentoExistente = new Agendamento
        {
            NomeResponsavel = "Ana",
            Contato = "333333333",
            CidadeBairro = "Cohama",
            DataHoraInicio = DateTime.Today.AddDays(1).AddHours(14),
            DataHoraFim = DateTime.Today.AddDays(1).AddHours(15)
        };
        
        _context.Agendamentos.Add(agendamentoExistente);
        _context.SaveChanges();

        // Tentar criar das 14:30 às 15:30 (conflito parcial)
        var agendamentoConflitante = new Agendamento
        {
            NomeResponsavel = "Carlos",
            Contato = "444444444",
            CidadeBairro = "São Francisco",
            DataHoraInicio = DateTime.Today.AddDays(1).AddHours(14).AddMinutes(30),
            DataHoraFim = DateTime.Today.AddDays(1).AddHours(14).AddMinutes(30)
        };

        // Act
        var result = _controller.Post(agendamentoConflitante);

        // Assert
        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public void Post_HorariosConsecutivos_DevePermitir()
    {
        // Arrange - Agendamento das 14h às 15h
        var agendamento1 = new Agendamento
        {
            NomeResponsavel = "Pedro",
            Contato = "555555555",
            CidadeBairro = "Turu",
            DataHoraInicio = DateTime.Today.AddDays(1).AddHours(14),
            DataHoraFim = DateTime.Today.AddDays(1).AddHours(15)
        };
        
        _context.Agendamentos.Add(agendamento1);
        _context.SaveChanges();

        // Agendamento das 15h às 16h (consecutivo)
        var agendamento2 = new Agendamento
        {
            NomeResponsavel = "Paula",
            Contato = "666666666",
            CidadeBairro = "Vila Palmeira",
            DataHoraInicio = DateTime.Today.AddDays(1).AddHours(15), // Exato fim do anterior
            DataHoraFim = DateTime.Today.AddDays(1).AddHours(15)
        };

        // Act
        var result = _controller.Post(agendamento2);

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public void Post_ComDiferentes_SemConflito_DevePermitir()
    {
        // Arrange - Agendamento das 10h às 11h
        var agendamento1 = new Agendamento
        {
            NomeResponsavel = "Roberto",
            Contato = "777777777",
            CidadeBairro = "Centro",
            DataHoraInicio = DateTime.Today.AddDays(1).AddHours(10),
            DataHoraFim = DateTime.Today.AddDays(1).AddHours(11)
        };
        
        _context.Agendamentos.Add(agendamento1);
        _context.SaveChanges();

        // Agendamento das 16h às 17h (sem conflito)
        var agendamento2 = new Agendamento
        {
            NomeResponsavel = "Roberta",
            Contato = "888888888",
            CidadeBairro = "Liberdade",
            DataHoraInicio = DateTime.Today.AddDays(1).AddHours(16),
            DataHoraFim = DateTime.Today.AddDays(1).AddHours(16)
        };

        // Act
        var result = _controller.Post(agendamento2);

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public void Post_DeveSalvarComCorPadrao()
    {
        // Arrange
        var agendamento = new Agendamento
        {
            NomeResponsavel = "Teste Cor",
            Contato = "999999999",
            CidadeBairro = "Teste",
            DataHoraInicio = DateTime.Today.AddDays(1).AddHours(10),
            DataHoraFim = DateTime.Today.AddDays(1).AddHours(10),
            Cor = "" // Cor vazia, deve usar padrão
        };

        // Act
        var result = _controller.Post(agendamento);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var agendamentoSalvo = Assert.IsType<Agendamento>(okResult.Value);
        
        // Se cor estava vazia, o modelo deve ter aplicado a cor padrão
        Assert.Equal("#3788d8", agendamentoSalvo.Cor);
    }

    [Theory]
    [InlineData(8, 0)]   // 08:00
    [InlineData(12, 30)] // 12:30  
    [InlineData(18, 45)] // 18:45
    [InlineData(21, 0)]  // 21:00
    public void Post_ComHorariosDentroDoFuncionamento_DevePermitir(int hora, int minuto)
    {
        // Arrange
        var agendamento = new Agendamento
        {
            NomeResponsavel = $"Teste {hora}:{minuto:D2}",
            Contato = "000000000",
            CidadeBairro = "Teste",
            DataHoraInicio = DateTime.Today.AddDays(1).AddHours(hora).AddMinutes(minuto),
            DataHoraFim = DateTime.Today.AddDays(1).AddHours(hora).AddMinutes(minuto)
        };

        // Act
        var result = _controller.Post(agendamento);

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public void Post_DeveCalcularDataHoraFimCorretamente()
    {
        // Arrange
        var dataHoraInicio = DateTime.Today.AddDays(1).AddHours(15).AddMinutes(30);
        var agendamento = new Agendamento
        {
            NomeResponsavel = "Teste Cálculo",
            Contato = "123123123",
            CidadeBairro = "Teste",
            DataHoraInicio = dataHoraInicio,
            DataHoraFim = dataHoraInicio // Será sobrescrito
        };

        // Act
        var result = _controller.Post(agendamento);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var agendamentoSalvo = Assert.IsType<Agendamento>(okResult.Value);
        
        var expectedFim = dataHoraInicio.AddHours(1);
        Assert.Equal(expectedFim, agendamentoSalvo.DataHoraFim);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}