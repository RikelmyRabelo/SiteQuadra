using Microsoft.EntityFrameworkCore;
using SiteQuadra.Data;

var builder = WebApplication.CreateBuilder(args);


// Adiciona a configuração do DbContext para usar o SQLite
builder.Services.AddDbContext<QuadraContext>(options => 
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// CORS AQUI
app.UseCors("AllowAllOrigins");

app.UseAuthorization();

app.MapControllers();

app.Run();