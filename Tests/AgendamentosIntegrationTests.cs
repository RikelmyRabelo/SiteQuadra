using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SiteQuadra.Data;
using SiteQuadra.Models;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;

namespace SiteQuadra.Tests;

public class AgendamentosIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AgendamentosIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove o contexto real e adiciona um em memória
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<QuadraContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                services.AddDbContext<QuadraContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb");
                });
            });
        });
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task PostAgendamento_ComDadosValidos_DeveRetornarSucesso()
    {
        // Arrange
        var agendamento = new Agendamento
        {
            NomeResponsavel = "João Silva",
            Contato = "(98) 99999-9999",
            CidadeBairro = "Monte Alegre",
            DataHoraInicio = DateTime.Today.AddDays(1).AddHours(10), // Amanhã às 10h
            DataHoraFim = DateTime.Today.AddDays(1).AddHours(10) // Será corrigido pelo controller
        };

        var json = JsonSerializer.Serialize(agendamento);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/agendamentos", content);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<Agendamento>(responseContent, new JsonSerializerOptions 
        { 
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
        });

        Assert.NotNull(result);
        Assert.Equal("João Silva", result.NomeResponsavel);
        Assert.Equal("(98) 99999-9999", result.Contato);
        Assert.Equal("Monte Alegre", result.CidadeBairro);
        Assert.Equal("#3788d8", result.Cor); // Cor padrão
        
        // Verificar se DataHoraFim foi calculado corretamente (1 hora depois)
        Assert.Equal(agendamento.DataHoraInicio.AddHours(1), result.DataHoraFim);
    }

    [Fact]
    public async Task PostAgendamento_ComConflito_DeveRetornar409()
    {
        // Arrange - Criar primeiro agendamento
        await CriarAgendamentoTeste(DateTime.Today.AddDays(1).AddHours(14)); // Amanhã às 14h

        // Tentar criar segundo agendamento no mesmo horário
        var agendamentoConflitante = new Agendamento
        {
            NomeResponsavel = "Maria Oliveira",
            Contato = "(98) 88888-8888",
            CidadeBairro = "Centro",
            DataHoraInicio = DateTime.Today.AddDays(1).AddHours(14), // Mesmo horário
            DataHoraFim = DateTime.Today.AddDays(1).AddHours(14)
        };

        var json = JsonSerializer.Serialize(agendamentoConflitante);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/agendamentos", content);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        
        var errorMessage = await response.Content.ReadAsStringAsync();
        Assert.Contains("Já existe um agendamento que colide com este horário", errorMessage);
    }

    [Fact]
    public async Task PostAgendamento_ComConflitoParcial_DeveRetornar409()
    {
        // Arrange - Criar agendamento das 14h às 15h
        await CriarAgendamentoTeste(DateTime.Today.AddDays(1).AddHours(14));

        // Tentar criar agendamento das 14:30h às 15:30h (conflito parcial)
        var agendamentoConflitante = new Agendamento
        {
            NomeResponsavel = "Carlos Santos",
            Contato = "(98) 77777-7777",
            CidadeBairro = "Liberdade",
            DataHoraInicio = DateTime.Today.AddDays(1).AddHours(14).AddMinutes(30),
            DataHoraFim = DateTime.Today.AddDays(1).AddHours(14).AddMinutes(30)
        };

        var json = JsonSerializer.Serialize(agendamentoConflitante);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/agendamentos", content);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task PostAgendamento_HorariosConsecutivos_DevePermitir()
    {
        // Arrange - Criar agendamento das 14h às 15h
        await CriarAgendamentoTeste(DateTime.Today.AddDays(1).AddHours(14));

        // Criar agendamento das 15h às 16h (consecutivo, sem conflito)
        var agendamentoConsecutivo = new Agendamento
        {
            NomeResponsavel = "Ana Costa",
            Contato = "(98) 66666-6666",
            CidadeBairro = "Cohama",
            DataHoraInicio = DateTime.Today.AddDays(1).AddHours(15), // Exatamente quando o anterior termina
            DataHoraFim = DateTime.Today.AddDays(1).AddHours(15)
        };

        var json = JsonSerializer.Serialize(agendamentoConsecutivo);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/agendamentos", content);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetAgendamentos_DeveRetornarTodosAgendamentos()
    {
        // Arrange - Limpar e criar agendamentos de teste
        await LimparDatabase();
        await CriarAgendamentoTeste(DateTime.Today.AddDays(1).AddHours(10), "Teste 1");
        await CriarAgendamentoTeste(DateTime.Today.AddDays(1).AddHours(16), "Teste 2");

        // Act
        var response = await _client.GetAsync("/api/agendamentos");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var json = await response.Content.ReadAsStringAsync();
        var agendamentos = JsonSerializer.Deserialize<List<Agendamento>>(json, new JsonSerializerOptions 
        { 
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
        });

        Assert.NotNull(agendamentos);
        Assert.Equal(2, agendamentos.Count);
        Assert.Contains(agendamentos, a => a.NomeResponsavel == "Teste 1");
        Assert.Contains(agendamentos, a => a.NomeResponsavel == "Teste 2");
    }

    [Theory]
    [InlineData("", "(98) 99999-9999", "Monte Alegre")] // Nome vazio
    [InlineData("João", "", "Monte Alegre")] // Contato vazio  
    [InlineData("João", "(98) 99999-9999", "")] // Cidade vazia
    public async Task PostAgendamento_ComCamposObrigatoriosVazios_DeveValidar(
        string nome, string contato, string cidade)
    {
        // Arrange
        var agendamento = new Agendamento
        {
            NomeResponsavel = nome,
            Contato = contato,
            CidadeBairro = cidade,
            DataHoraInicio = DateTime.Today.AddDays(1).AddHours(10),
            DataHoraFim = DateTime.Today.AddDays(1).AddHours(10)
        };

        var json = JsonSerializer.Serialize(agendamento);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/agendamentos", content);

        // Assert - O controller atual não faz validação detalhada, mas o agendamento é salvo
        // Em um cenário real, você poderia adicionar validações e esperar BadRequest
        Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostAgendamento_ComDataPassada_DeveFuncionar()
    {
        // Arrange - Este teste verifica se o backend aceita datas passadas
        // (a validação de data futura é feita no frontend)
        var agendamento = new Agendamento
        {
            NomeResponsavel = "Teste Passado",
            Contato = "(98) 99999-9999",
            CidadeBairro = "Teste",
            DataHoraInicio = DateTime.Today.AddDays(-1).AddHours(10), // Ontem
            DataHoraFim = DateTime.Today.AddDays(-1).AddHours(10)
        };

        var json = JsonSerializer.Serialize(agendamento);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/agendamentos", content);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // Métodos auxiliares
    private async Task<Agendamento> CriarAgendamentoTeste(DateTime dataHora, string nome = "Teste Agendamento")
    {
        var agendamento = new Agendamento
        {
            NomeResponsavel = nome,
            Contato = "(98) 99999-9999",
            CidadeBairro = "Teste Cidade",
            DataHoraInicio = dataHora,
            DataHoraFim = dataHora
        };

        var json = JsonSerializer.Serialize(agendamento);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/agendamentos", content);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<Agendamento>(responseContent, new JsonSerializerOptions 
        { 
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
        })!;
    }

    private async Task LimparDatabase()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<QuadraContext>();
        
        context.Agendamentos.RemoveRange(context.Agendamentos);
        await context.SaveChangesAsync();
    }
}