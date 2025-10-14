FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

# Copy project file and restore
COPY Src/SiteQuadra.csproj Src/
RUN dotnet restore Src/SiteQuadra.csproj

# Copy source and publish
COPY . .
RUN dotnet publish Src/SiteQuadra.csproj -c Release -o /app

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

ENV ASPNETCORE_URLS=http://*:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Create directories
RUN mkdir -p data logs Backups

# Copy published app
COPY --from=build /app .

EXPOSE 8080
ENTRYPOINT ["dotnet", "SiteQuadra.dll"]


ENTRYPOINT ["dotnet", "SiteQuadra.dll"]