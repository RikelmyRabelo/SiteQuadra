using Microsoft.EntityFrameworkCore;
using SiteQuadra.Data;
using SiteQuadra.Middleware;
using SiteQuadra.Services;

var builder = WebApplication.CreateBuilder(args);


// Adiciona a configuração do DbContext para usar o SQLite
builder.Services.AddDbContext<QuadraContext>(options => 
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Registra serviços
builder.Services.AddSingleton<IAdminSecurityService, AdminSecurityService>();
builder.Services.AddSingleton<IBackupService, BackupService>();
builder.Services.AddHostedService<BackupService>();
builder.Services.AddTransient<IConfigurationValidationService, ConfigurationValidationService>();
builder.Services.AddTransient<IDatabaseInitializationService, DatabaseInitializationService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// Configuração de CORS segura
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? new string[0];

builder.Services.AddCors(options =>
{
    options.AddPolicy("SecurePolicy",
        policy =>
        {
            if (builder.Environment.IsDevelopment())
            {
                // Desenvolvimento: mais permissivo
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            }
            else
            {
                // Produção: restritivo e seguro
                policy.WithOrigins(allowedOrigins)
                      .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
                      .WithHeaders("Content-Type", "Authorization", "Accept")
                      .AllowCredentials();
            }
        });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // Força HTTPS em produção
    app.UseHsts();
}

app.UseHttpsRedirection();

// Servidor de arquivos estáticos (Frontend)
app.UseDefaultFiles(); // Serve index.html automaticamente
app.UseStaticFiles(); // Serve arquivos estáticos

// CORS AQUI - Política segura
app.UseCors("SecurePolicy");

// Headers de segurança para produção
if (!app.Environment.IsDevelopment())
{
    app.Use(async (context, next) =>
    {
        context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Add("X-Frame-Options", "DENY");
        context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
        context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
        context.Response.Headers.Add("Content-Security-Policy", "default-src 'self'; script-src 'self' 'unsafe-inline' cdn.jsdelivr.net; style-src 'self' 'unsafe-inline'; img-src 'self' data:;");
        await next();
    });
}

// Middleware de logging de segurança
app.UseMiddleware<SecurityLoggingMiddleware>();

// Middleware de autenticação administrativa
app.UseMiddleware<AdminAuthMiddleware>();

app.UseAuthorization();

app.MapControllers();

// Rota padrão para o frontend (SPA fallback)
app.MapFallbackToFile("index.html");

// Inicialização do sistema
using (var scope = app.Services.CreateScope())
{
    var configValidation = scope.ServiceProvider.GetRequiredService<IConfigurationValidationService>();
    await configValidation.ValidateConfigurationAsync();
    
    var dbInitialization = scope.ServiceProvider.GetRequiredService<IDatabaseInitializationService>();
    await dbInitialization.InitializeAsync();
    await dbInitialization.SeedDataAsync();
    
    var adminSecurity = scope.ServiceProvider.GetRequiredService<IAdminSecurityService>();
    await adminSecurity.InitializeAdminPasswordAsync();
}

app.Run();

// Tornar Program acessível para testes
public partial class Program { }
