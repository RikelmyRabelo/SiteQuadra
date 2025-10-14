using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SiteQuadra.Data;
using SiteQuadra.Services;
using System.Diagnostics;

namespace SiteQuadra.Controllers;

[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    private readonly QuadraContext _context;
    private readonly IAdminSecurityService _adminSecurity;
    private readonly IBackupService _backupService;
    private readonly ILogger<HealthController> _logger;

    public HealthController(
        QuadraContext context,
        IAdminSecurityService adminSecurity,
        IBackupService backupService,
        ILogger<HealthController> logger)
    {
        _context = context;
        _adminSecurity = adminSecurity;
        _backupService = backupService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var healthStatus = new
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            Version = GetApplicationVersion(),
            Checks = await PerformHealthChecksAsync()
        };

        var hasUnhealthyChecks = healthStatus.Checks.Any(c => GetStatusFromObject(c) != "Healthy");
        
        if (hasUnhealthyChecks)
        {
            _logger.LogWarning("Health check falhou: {UnhealthyChecks}", 
                string.Join(", ", healthStatus.Checks.Where(c => GetStatusFromObject(c) != "Healthy").Select(c => GetNameFromObject(c))));
            return StatusCode(503, healthStatus);
        }

        return Ok(healthStatus);
    }

    [HttpGet("detailed")]
    public async Task<IActionResult> GetDetailed()
    {
        var startTime = DateTime.UtcNow;
        var checks = await PerformHealthChecksAsync();
        var endTime = DateTime.UtcNow;

        var detailedHealth = new
        {
            Status = checks.All(c => c.Status == "Healthy") ? "Healthy" : "Unhealthy",
            Timestamp = startTime,
            Duration = (endTime - startTime).TotalMilliseconds,
            Version = GetApplicationVersion(),
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
            MachineName = Environment.MachineName,
            ProcessId = Environment.ProcessId,
            WorkingSet = GC.GetTotalMemory(false),
            Uptime = GetUptime(),
            Checks = checks,
            SystemInfo = GetSystemInfo()
        };

        var hasUnhealthyChecks = checks.Any(c => GetStatusFromObject(c) != "Healthy");
        return hasUnhealthyChecks ? StatusCode(503, detailedHealth) : Ok(detailedHealth);
    }

    [HttpGet("ready")]
    public async Task<IActionResult> Ready()
    {
        try
        {
            // Verifica se pode conectar no banco
            await _context.Database.CanConnectAsync();
            
            // Verifica se sistema de segurança está inicializado
            var passwordHash = await _adminSecurity.GetStoredPasswordHashAsync();
            if (string.IsNullOrEmpty(passwordHash))
            {
                return StatusCode(503, new { Status = "NotReady", Reason = "Admin security not initialized" });
            }

            return Ok(new { Status = "Ready", Timestamp = DateTime.UtcNow });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Readiness check failed");
            return StatusCode(503, new { Status = "NotReady", Reason = ex.Message });
        }
    }

    [HttpGet("live")]
    public IActionResult Live()
    {
        return Ok(new { Status = "Alive", Timestamp = DateTime.UtcNow });
    }

    private async Task<List<object>> PerformHealthChecksAsync()
    {
        var checks = new List<object>();

        // 1. Database Health
        checks.Add(await CheckDatabaseHealthAsync());

        // 2. Admin Security Health
        checks.Add(await CheckAdminSecurityHealthAsync());

        // 3. Backup Service Health
        checks.Add(await CheckBackupServiceHealthAsync());

        // 4. File System Health
        checks.Add(CheckFileSystemHealth());

        // 5. Memory Health
        checks.Add(CheckMemoryHealth());

        return checks;
    }

    private async Task<object> CheckDatabaseHealthAsync()
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();
            var canConnect = await _context.Database.CanConnectAsync();
            stopwatch.Stop();

            if (!canConnect)
            {
                return new
                {
                    Name = "Database",
                    Status = "Unhealthy",
                    Description = "Cannot connect to database",
                    Duration = stopwatch.ElapsedMilliseconds
                };
            }

            var agendamentosCount = await _context.Agendamentos.CountAsync();

            return new
            {
                Name = "Database",
                Status = "Healthy",
                Description = $"Connected successfully. {agendamentosCount} agendamentos",
                Duration = stopwatch.ElapsedMilliseconds
            };
        }
        catch (Exception ex)
        {
            return new
            {
                Name = "Database",
                Status = "Unhealthy",
                Description = ex.Message,
                Duration = 0
            };
        }
    }

    private async Task<object> CheckAdminSecurityHealthAsync()
    {
        try
        {
            var passwordHash = await _adminSecurity.GetStoredPasswordHashAsync();
            var isInitialized = !string.IsNullOrEmpty(passwordHash);

            return new
            {
                Name = "AdminSecurity",
                Status = isInitialized ? "Healthy" : "Unhealthy",
                Description = isInitialized ? "Admin security initialized" : "Admin security not initialized"
            };
        }
        catch (Exception ex)
        {
            return new
            {
                Name = "AdminSecurity",
                Status = "Unhealthy",
                Description = ex.Message
            };
        }
    }

    private async Task<object> CheckBackupServiceHealthAsync()
    {
        try
        {
            var backups = await _backupService.ListBackupsAsync();

            return new
            {
                Name = "BackupService",
                Status = "Healthy",
                Description = $"Service running. {backups.Length} backups available"
            };
        }
        catch (Exception ex)
        {
            return new
            {
                Name = "BackupService",
                Status = "Unhealthy",
                Description = ex.Message
            };
        }
    }

    private object CheckFileSystemHealth()
    {
        try
        {
            var requiredDirectories = new[] { "data", "logs", "Backups" };
            var missingDirectories = new List<string>();

            foreach (var dir in requiredDirectories)
            {
                var fullPath = Path.Combine(Directory.GetCurrentDirectory(), dir);
                if (!Directory.Exists(fullPath))
                {
                    missingDirectories.Add(dir);
                }
            }

            if (missingDirectories.Any())
            {
                return new
                {
                    Name = "FileSystem",
                    Status = "Unhealthy",
                    Description = $"Missing directories: {string.Join(", ", missingDirectories)}"
                };
            }

            // Testa escrita
            var testFile = Path.Combine(Directory.GetCurrentDirectory(), "health_test.tmp");
            File.WriteAllText(testFile, "test");
            File.Delete(testFile);

            return new
            {
                Name = "FileSystem",
                Status = "Healthy",
                Description = "All directories exist and writable"
            };
        }
        catch (Exception ex)
        {
            return new
            {
                Name = "FileSystem",
                Status = "Unhealthy",
                Description = ex.Message
            };
        }
    }

    private object CheckMemoryHealth()
    {
        try
        {
            var totalMemory = GC.GetTotalMemory(false);
            var workingSet = Environment.WorkingSet;
            
            // Alerta se usando mais de 400MB
            var isHealthy = totalMemory < 400 * 1024 * 1024;

            return new
            {
                Name = "Memory",
                Status = isHealthy ? "Healthy" : "Warning",
                Description = $"Total: {totalMemory / (1024 * 1024)}MB, Working Set: {workingSet / (1024 * 1024)}MB"
            };
        }
        catch (Exception ex)
        {
            return new
            {
                Name = "Memory",
                Status = "Unhealthy",
                Description = ex.Message
            };
        }
    }

    private string GetApplicationVersion()
    {
        try
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            return assembly.GetName().Version?.ToString() ?? "Unknown";
        }
        catch
        {
            return "Unknown";
        }
    }

    private TimeSpan GetUptime()
    {
        try
        {
            using var process = Process.GetCurrentProcess();
            return DateTime.Now - process.StartTime;
        }
        catch
        {
            return TimeSpan.Zero;
        }
    }

    private object GetSystemInfo()
    {
        return new
        {
            OS = Environment.OSVersion.ToString(),
            Framework = Environment.Version.ToString(),
            ProcessorCount = Environment.ProcessorCount,
            MachineName = Environment.MachineName,
            UserName = Environment.UserName,
            CurrentDirectory = Directory.GetCurrentDirectory()
        };
    }
    
    private string GetStatusFromObject(object obj)
    {
        var statusProperty = obj.GetType().GetProperty("Status");
        return statusProperty?.GetValue(obj)?.ToString() ?? "Unknown";
    }
    
    private string GetNameFromObject(object obj)
    {
        var nameProperty = obj.GetType().GetProperty("Name");
        return nameProperty?.GetValue(obj)?.ToString() ?? "Unknown";
    }
}
