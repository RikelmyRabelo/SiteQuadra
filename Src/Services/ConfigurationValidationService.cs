using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SiteQuadra.Services;

public interface IConfigurationValidationService
{
    Task ValidateConfigurationAsync();
    bool IsProductionEnvironment { get; }
}

public class ConfigurationValidationService : IConfigurationValidationService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ConfigurationValidationService> _logger;
    private readonly IWebHostEnvironment _environment;

    public ConfigurationValidationService(
        IConfiguration configuration, 
        ILogger<ConfigurationValidationService> logger,
        IWebHostEnvironment environment)
    {
        _configuration = configuration;
        _logger = logger;
        _environment = environment;
    }

    public bool IsProductionEnvironment => _environment.IsProduction();

    public async Task ValidateConfigurationAsync()
    {
        _logger.LogInformation("🔍 Validando configurações do sistema...");

        var validationErrors = new List<string>();

        // Validação de Connection String
        ValidateConnectionString(validationErrors);

        // Validação de CORS em produção
        if (IsProductionEnvironment)
        {
            ValidateProductionCors(validationErrors);
            ValidateProductionSecurity(validationErrors);
        }

        // Validação de diretórios necessários
        ValidateDirectories(validationErrors);

        // Validação de permissões de arquivo
        await ValidateFilePermissionsAsync(validationErrors);

        if (validationErrors.Any())
        {
            var errorMessage = "❌ Erros de configuração encontrados:\n" + 
                              string.Join("\n", validationErrors.Select(e => $"  • {e}"));
            
            _logger.LogCritical(errorMessage);
            throw new InvalidOperationException("Configuração do sistema inválida. Verifique os logs para detalhes.");
        }

        _logger.LogInformation("✅ Todas as configurações validadas com sucesso");
    }

    private void ValidateConnectionString(List<string> errors)
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            errors.Add("Connection string 'DefaultConnection' não configurada");
            return;
        }

        // Valida se o diretório do banco existe (para SQLite)
        if (connectionString.Contains("Data Source="))
        {
            var dbPath = ExtractSqlitePathFromConnectionString(connectionString);
            var dbDirectory = Path.GetDirectoryName(dbPath);
            
            if (!string.IsNullOrEmpty(dbDirectory) && !Directory.Exists(dbDirectory))
            {
                try
                {
                    Directory.CreateDirectory(dbDirectory);
                    _logger.LogInformation("📁 Diretório do banco criado: {Directory}", dbDirectory);
                }
                catch (Exception ex)
                {
                    errors.Add($"Não foi possível criar diretório do banco: {dbDirectory}. Erro: {ex.Message}");
                }
            }
        }
    }

    private void ValidateProductionCors(List<string> errors)
    {
        var allowedOrigins = _configuration.GetSection("AllowedOrigins").Get<string[]>();
        
        if (allowedOrigins == null || !allowedOrigins.Any())
        {
            errors.Add("AllowedOrigins deve ser configurado em produção");
            return;
        }

        foreach (var origin in allowedOrigins)
        {
            if (string.IsNullOrWhiteSpace(origin) || origin.Contains("#{") || origin == "*")
            {
                errors.Add($"Origem inválida ou placeholder não substituído: {origin}");
            }
            
            if (!origin.StartsWith("https://") && !_environment.IsDevelopment())
            {
                _logger.LogWarning("⚠️ Origem não HTTPS detectada em produção: {Origin}", origin);
            }
        }
    }

    private void ValidateProductionSecurity(List<string> errors)
    {
        // Validação de certificado HTTPS em produção
        var httpsUrl = _configuration["Kestrel:Endpoints:Https:Url"];
        var certPath = _configuration["Kestrel:Endpoints:Https:Certificate:Path"];
        var certPassword = _configuration["Kestrel:Endpoints:Https:Certificate:Password"];

        if (!string.IsNullOrEmpty(httpsUrl))
        {
            if (string.IsNullOrWhiteSpace(certPath) || certPath.Contains("#{"))
            {
                _logger.LogWarning("⚠️ Certificado HTTPS não configurado corretamente");
            }
            else if (!File.Exists(certPath))
            {
                errors.Add($"Arquivo de certificado não encontrado: {certPath}");
            }

            if (string.IsNullOrWhiteSpace(certPassword) || certPassword.Contains("#{"))
            {
                errors.Add("Senha do certificado HTTPS não configurada");
            }
        }
    }

    private void ValidateDirectories(List<string> errors)
    {
        var requiredDirectories = new[]
        {
            "logs",
            "Backups",
            "data"
        };

        foreach (var dir in requiredDirectories)
        {
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), dir);
            
            if (!Directory.Exists(fullPath))
            {
                try
                {
                    Directory.CreateDirectory(fullPath);
                    _logger.LogInformation("📁 Diretório criado: {Directory}", fullPath);
                }
                catch (Exception ex)
                {
                    errors.Add($"Não foi possível criar diretório '{dir}': {ex.Message}");
                }
            }
        }
    }

    private async Task ValidateFilePermissionsAsync(List<string> errors)
    {
        try
        {
            // Testa escrita no diretório atual
            var testFile = Path.Combine(Directory.GetCurrentDirectory(), "write_test.tmp");
            
            await File.WriteAllTextAsync(testFile, "test");
            File.Delete(testFile);
            
            _logger.LogInformation("✅ Permissões de escrita validadas");
        }
        catch (Exception ex)
        {
            errors.Add($"Sem permissões de escrita no diretório da aplicação: {ex.Message}");
        }
    }

    private string ExtractSqlitePathFromConnectionString(string connectionString)
    {
        // Extrai o caminho do arquivo SQLite da connection string
        var dataSourceIndex = connectionString.IndexOf("Data Source=", StringComparison.OrdinalIgnoreCase);
        if (dataSourceIndex == -1) return "";

        var pathStart = dataSourceIndex + "Data Source=".Length;
        var pathEnd = connectionString.IndexOf(';', pathStart);
        
        if (pathEnd == -1)
            pathEnd = connectionString.Length;

        return connectionString.Substring(pathStart, pathEnd - pathStart).Trim();
    }
}