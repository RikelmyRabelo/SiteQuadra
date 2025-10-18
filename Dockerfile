# Dockerfile otimizado para Railway
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Railway usa porta 8080 por padrão
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia arquivos de projeto
COPY ["Src/SiteQuadra.csproj", "Src/"]
RUN dotnet restore "Src/SiteQuadra.csproj"

# Copia código fonte
COPY . .
WORKDIR "/src/Src"

# Build da aplicação
RUN dotnet build "SiteQuadra.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SiteQuadra.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

# Cria diretórios necessários
RUN mkdir -p /app/data /app/logs /app/Backups

# Copia aplicação
COPY --from=publish /app/publish .

# Copia frontend do diretório correto
COPY --from=build /src/FronEnd ./wwwroot/

# Health check específico para Railway
HEALTHCHECK --interval=30s --timeout=10s --start-period=30s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "SiteQuadra.dll"]