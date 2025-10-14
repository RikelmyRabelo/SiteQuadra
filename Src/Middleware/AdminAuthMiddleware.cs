using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SiteQuadra.Services;
using System.Threading.Tasks;

namespace SiteQuadra.Middleware;

public class AdminAuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IAdminSecurityService _adminSecurity;

    public AdminAuthMiddleware(RequestDelegate next, IAdminSecurityService adminSecurity)
    {
        _next = next;
        _adminSecurity = adminSecurity;
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
            
            // Validação segura do token
            if (!_adminSecurity.IsValidToken(token))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Token de autenticação inválido ou expirado");
                return;
            }
        }
        
        await _next(context);
    }
}