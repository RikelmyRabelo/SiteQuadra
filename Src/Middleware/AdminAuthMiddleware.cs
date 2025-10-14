using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace SiteQuadra.Middleware;

public class AdminAuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _adminPassword;

    public AdminAuthMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _adminPassword = configuration["AdminPassword"] ?? "admin123"; // Senha padrão para desenvolvimento
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLower() ?? "";
        
        // Só aplica autenticação para rotas administrativas, exceto login
        if (path.StartsWith("/api/admin/") && !path.Equals("/api/admin/login"))
        {
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
            
            if (authHeader == null || !authHeader.StartsWith("Bearer "))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Token de autenticação necessário");
                return;
            }
            
            var token = authHeader.Substring("Bearer ".Length).Trim();
            
            // Validação simples do token (em produção, use JWT ou similar)
            if (token != _adminPassword)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Token de autenticação inválido");
                return;
            }
        }
        
        await _next(context);
    }
}