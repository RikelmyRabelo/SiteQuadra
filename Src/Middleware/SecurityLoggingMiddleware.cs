using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace SiteQuadra.Middleware;

public class SecurityLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SecurityLoggingMiddleware> _logger;

    public SecurityLoggingMiddleware(RequestDelegate next, ILogger<SecurityLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var request = context.Request;
        var clientIp = GetClientIpAddress(context);
        var userAgent = request.Headers["User-Agent"].ToString();
        var path = request.Path.Value?.ToLower() ?? "";

        // Log tentativas de acesso administrativo
        if (path.StartsWith("/api/admin/"))
        {
            var method = request.Method;
            
            if (path.Equals("/api/admin/login"))
            {
                _logger.LogInformation("🔐 Tentativa de login administrativo - IP: {ClientIp}, User-Agent: {UserAgent}", 
                    clientIp, userAgent);
            }
            else
            {
                _logger.LogInformation("🛡️ Acesso a rota administrativa - {Method} {Path} - IP: {ClientIp}", 
                    method, path, clientIp);
            }
        }

        // Log tentativas suspeitas
        LogSuspiciousActivity(request, clientIp, userAgent);

        var originalStatusCode = context.Response.StatusCode;
        
        await _next(context);
        
        // Log falhas de autenticação
        if (context.Response.StatusCode == 401 && path.StartsWith("/api/admin/"))
        {
            _logger.LogWarning("❌ Falha de autenticação administrativa - IP: {ClientIp}, Path: {Path}, User-Agent: {UserAgent}", 
                clientIp, path, userAgent);
        }
        
        // Log sucessos de login
        if (context.Response.StatusCode == 200 && path.Equals("/api/admin/login"))
        {
            _logger.LogInformation("✅ Login administrativo bem-sucedido - IP: {ClientIp}", clientIp);
        }
    }

    private void LogSuspiciousActivity(HttpRequest request, string clientIp, string userAgent)
    {
        var path = request.Path.Value?.ToLower() ?? "";
        var queryString = request.QueryString.Value?.ToLower() ?? "";
        
        // Lista de padrões suspeitos
        string[] suspiciousPatterns = {
            "script", "alert", "javascript:", "vbscript:", "onload", "onerror",
            "union", "select", "drop", "delete", "insert", "update",
            "../", "..\\", "/etc/passwd", "/windows/system32",
            "cmd.exe", "powershell", "bash", "/bin/sh"
        };
        
        foreach (var pattern in suspiciousPatterns)
        {
            if (path.Contains(pattern) || queryString.Contains(pattern))
            {
                _logger.LogWarning("🚨 Atividade suspeita detectada - Padrão: {Pattern}, Path: {Path}, Query: {QueryString}, IP: {ClientIp}, User-Agent: {UserAgent}",
                    pattern, path, queryString, clientIp, userAgent);
                break;
            }
        }
        
        // Log User-Agents suspeitos
        if (string.IsNullOrWhiteSpace(userAgent) || 
            userAgent.Contains("bot", StringComparison.OrdinalIgnoreCase) ||
            userAgent.Contains("crawler", StringComparison.OrdinalIgnoreCase) ||
            userAgent.Contains("scanner", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation("🤖 User-Agent suspeito/bot detectado - IP: {ClientIp}, User-Agent: {UserAgent}",
                clientIp, userAgent);
        }
    }

    private string GetClientIpAddress(HttpContext context)
    {
        var request = context.Request;
        
        // Verifica headers de proxy primeiro
        var forwardedFor = request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(forwardedFor))
        {
            var ips = forwardedFor.Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (ips.Length > 0)
            {
                return ips[0].Trim();
            }
        }
        
        var realIp = request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(realIp))
        {
            return realIp;
        }
        
        // Fallback para IP de conexão direta
        return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }
}