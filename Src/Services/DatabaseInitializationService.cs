using Microsoft.EntityFrameworkCore;
using SiteQuadra.Data;
using SiteQuadra.Models;

namespace SiteQuadra.Services;

public interface IDatabaseInitializationService
{
    Task InitializeAsync();
    Task SeedDataAsync();
}

public class DatabaseInitializationService : IDatabaseInitializationService
{
    private readonly QuadraContext _context;
    private readonly ILogger<DatabaseInitializationService> _logger;
    private readonly IWebHostEnvironment _environment;

    public DatabaseInitializationService(
        QuadraContext context,
        ILogger<DatabaseInitializationService> logger,
        IWebHostEnvironment environment)
    {
        _context = context;
        _logger = logger;
        _environment = environment;
    }

    public async Task InitializeAsync()
    {
        try
        {
            _logger.LogInformation("🗄️ Inicializando banco de dados...");

            // Verifica se é um banco relacional antes de executar migrations
            if (_context.Database.IsRelational())
            {
                _logger.LogInformation("🔄 Executando migrations...");
                await _context.Database.MigrateAsync();
                _logger.LogInformation("✅ Database inicializado via migrations");
            }
            else
            {
                _logger.LogInformation("🔄 Garantindo criação do banco...");
                await _context.Database.EnsureCreatedAsync();
                _logger.LogInformation("✅ Database inicializado via EnsureCreated");
            }

            // Verifica integridade do banco
            await ValidateDatabaseIntegrityAsync();

            _logger.LogInformation("🎯 Inicialização do banco concluída com sucesso");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro durante inicialização do banco de dados");
            throw;
        }
    }

    public async Task SeedDataAsync()
    {
        try
        {
            _logger.LogInformation("🌱 Verificando dados iniciais...");

            // Em desenvolvimento, adiciona alguns dados de exemplo
            if (_environment.IsDevelopment())
            {
                await SeedDevelopmentDataAsync();
            }

            // Sempre verifica se há agendamentos básicos
            await EnsureBasicDataAsync();

            _logger.LogInformation("✅ Dados iniciais verificados");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao inserir dados iniciais");
            throw;
        }
    }

    private async Task ValidateDatabaseIntegrityAsync()
    {
        try
        {
            // Testa uma query simples
            var count = await _context.Agendamentos.CountAsync();
            _logger.LogInformation("📊 Banco contém {Count} agendamentos", count);

            // Verifica se as tabelas essenciais existem
            var tableNames = new[] { "Agendamentos" };
            
            foreach (var tableName in tableNames)
            {
                var exists = await TableExistsAsync(tableName);
                if (!exists)
                {
                    throw new InvalidOperationException($"Tabela essencial '{tableName}' não encontrada");
                }
            }

            _logger.LogInformation("✅ Integridade do banco verificada");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Falha na verificação de integridade do banco");
            throw;
        }
    }

    private async Task<bool> TableExistsAsync(string tableName)
    {
        try
        {
            // Para bancos InMemory (testes), assume que as tabelas existem se o contexto foi criado
            if (!_context.Database.IsRelational())
            {
                return true;
            }

            // Para SQLite, verifica na tabela sqlite_master
            var sql = "SELECT name FROM sqlite_master WHERE type='table' AND name=@tableName;";
            
            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText = sql;
            
            var parameter = command.CreateParameter();
            parameter.ParameterName = "@tableName";
            parameter.Value = tableName;
            command.Parameters.Add(parameter);

            if (_context.Database.IsRelational())
            {
                await _context.Database.OpenConnectionAsync();
            }
            
            var result = await command.ExecuteScalarAsync();
            return result != null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Não foi possível verificar se tabela {TableName} existe", tableName);
            return false;
        }
        finally
        {
            if (_context.Database.IsRelational())
            {
                await _context.Database.CloseConnectionAsync();
            }
        }
    }

    private async Task SeedDevelopmentDataAsync()
    {
        // Só adiciona se não existir nenhum agendamento
        var existingCount = await _context.Agendamentos.CountAsync();
        if (existingCount > 0)
        {
            _logger.LogInformation("Dados de desenvolvimento já existem. Pulando seed.");
            return;
        }

        _logger.LogInformation("🧪 Adicionando dados de exemplo para desenvolvimento...");

        var exemplos = new[]
        {
            new Agendamento
            {
                NomeResponsavel = "João Silva (Exemplo)",
                Contato = "(98) 99999-1111",
                CidadeBairro = "Centro",
                DataHoraInicio = DateTime.Today.AddDays(1).AddHours(19), // Amanhã às 19h
                DataHoraFim = DateTime.Today.AddDays(1).AddHours(20),     // Amanhã às 20h
                Cor = "#3788d8"
            },
            new Agendamento
            {
                NomeResponsavel = "Maria Santos (Exemplo)",
                Contato = "(98) 99999-2222", 
                CidadeBairro = "Liberdade",
                DataHoraInicio = DateTime.Today.AddDays(2).AddHours(20), // Depois de amanhã às 20h
                DataHoraFim = DateTime.Today.AddDays(2).AddHours(21),     // Depois de amanhã às 21h
                Cor = "#28a745"
            }
        };

        _context.Agendamentos.AddRange(exemplos);
        await _context.SaveChangesAsync();

        _logger.LogInformation("✅ {Count} agendamentos de exemplo adicionados", exemplos.Length);
    }

    private async Task EnsureBasicDataAsync()
    {
        // Aqui você pode adicionar dados que sempre devem existir
        // Por exemplo, configurações padrão, usuários administrativos, etc.
        
        // Para este projeto, não há dados obrigatórios além das tabelas
        // Mas deixamos o método preparado para futuras necessidades
        
        await Task.CompletedTask;
        _logger.LogInformation("✅ Dados básicos verificados");
    }
}