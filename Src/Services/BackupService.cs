using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace SiteQuadra.Services;

public interface IBackupService
{
    Task CreateBackupAsync();
    Task<string[]> ListBackupsAsync();
}

public class BackupService : BackgroundService, IBackupService
{
    private readonly ILogger<BackupService> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _databasePath;
    private readonly string _backupDirectory;
    private readonly Timer _backupTimer;

    public BackupService(ILogger<BackupService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        _databasePath = "quadra.db"; // Path do banco SQLite
        _backupDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Backups");
        
        // Cria diretório de backup se não existir
        if (!Directory.Exists(_backupDirectory))
        {
            Directory.CreateDirectory(_backupDirectory);
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Backup a cada 24 horas
        var timer = new PeriodicTimer(TimeSpan.FromHours(24));
        
        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await CreateBackupAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erro ao criar backup automático");
            }
        }
    }

    public async Task CreateBackupAsync()
    {
        try
        {
            if (!File.Exists(_databasePath))
            {
                _logger.LogWarning("⚠️ Arquivo de banco de dados não encontrado para backup: {DatabasePath}", _databasePath);
                return;
            }

            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var backupFileName = $"quadra_backup_{timestamp}.db";
            var backupPath = Path.Combine(_backupDirectory, backupFileName);

            // Cria cópia do banco
            await Task.Run(() => File.Copy(_databasePath, backupPath, false));

            _logger.LogInformation("✅ Backup criado com sucesso: {BackupPath}", backupPath);

            // Limpa backups antigos (mantém apenas os últimos 7)
            await CleanOldBackupsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao criar backup do banco de dados");
            throw;
        }
    }

    public async Task<string[]> ListBackupsAsync()
    {
        try
        {
            var backupFiles = Directory.GetFiles(_backupDirectory, "quadra_backup_*.db")
                .OrderByDescending(f => File.GetCreationTime(f))
                .Select(f => Path.GetFileName(f))
                .ToArray();

            return await Task.FromResult(backupFiles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao listar backups");
            return Array.Empty<string>();
        }
    }

    private async Task CleanOldBackupsAsync()
    {
        try
        {
            var backupFiles = Directory.GetFiles(_backupDirectory, "quadra_backup_*.db")
                .OrderByDescending(f => File.GetCreationTime(f))
                .Skip(7) // Mantém apenas os 7 mais recentes
                .ToArray();

            foreach (var oldBackup in backupFiles)
            {
                await Task.Run(() => File.Delete(oldBackup));
                _logger.LogInformation("🗑️ Backup antigo removido: {BackupFile}", Path.GetFileName(oldBackup));
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ Erro ao limpar backups antigos");
        }
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("🚀 Serviço de backup iniciado");
        
        // Cria um backup inicial
        try
        {
            await CreateBackupAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao criar backup inicial");
        }

        await base.StartAsync(cancellationToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("🛑 Serviço de backup parado");
        await base.StopAsync(cancellationToken);
    }
}