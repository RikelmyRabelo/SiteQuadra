# Script de Verificação Pré-Deploy
# Execute antes de fazer deploy para produção

Write-Host "🔍 VERIFICAÇÃO PRÉ-DEPLOY - SiteQuadra" -ForegroundColor Cyan
Write-Host "=======================================" -ForegroundColor Cyan

$errorList = @()

# 1. Verifica se arquivos críticos existem
Write-Host "`n📁 Verificando arquivos críticos..." -ForegroundColor Yellow

$criticalFiles = @(
    "Src/Program.cs",
    "Src/Controllers/HealthController.cs", 
    "Src/Controllers/AdminController.cs",
    "Src/Controllers/AgendamentosController.cs",
    "Src/Services/AdminSecurityService.cs",
    "Src/Services/DatabaseInitializationService.cs",
    "Src/wwwroot/index.html",
    "Dockerfile",
    "docker-compose.yml",
    ".env.example"
)

foreach ($file in $criticalFiles) {
    if (Test-Path $file) {
        Write-Host "  ✅ $file" -ForegroundColor Green
    } else {
        Write-Host "  ❌ $file" -ForegroundColor Red
        $errorList += "Arquivo crítico ausente: $file"
    }
}

# 2. Verifica estrutura do frontend
Write-Host "`n🎨 Verificando estrutura do frontend..." -ForegroundColor Yellow

$frontendDirs = @(
    "Src/wwwroot/Admin",
    "Src/wwwroot/Calendar", 
    "Src/wwwroot/Booking"
)

foreach ($dir in $frontendDirs) {
    if (Test-Path $dir) {
        Write-Host "  ✅ $dir" -ForegroundColor Green
        
        # Verifica se há arquivos HTML na pasta
        $htmlFiles = Get-ChildItem "$dir/*.html" -ErrorAction SilentlyContinue
        if ($htmlFiles.Count -gt 0) {
            Write-Host "    📄 $($htmlFiles.Count) arquivos HTML encontrados" -ForegroundColor Gray
        } else {
            Write-Host "    ⚠️  Nenhum arquivo HTML encontrado" -ForegroundColor Yellow
        }
    } else {
        Write-Host "  ❌ $dir" -ForegroundColor Red
        $errorList += "Pasta do frontend ausente: $dir"
    }
}

# 3. Verifica configurações
Write-Host "`n⚙️ Verificando configurações..." -ForegroundColor Yellow

if (Test-Path ".env") {
    Write-Host "  ✅ Arquivo .env existe" -ForegroundColor Green
} else {
    Write-Host "  ⚠️  Arquivo .env não encontrado (será criado do .env.example)" -ForegroundColor Yellow
}

if (Test-Path "Src/appsettings.Production.json") {
    Write-Host "  ✅ Configuração de produção existe" -ForegroundColor Green
} else {
    Write-Host "  ❌ appsettings.Production.json ausente" -ForegroundColor Red
    $errorList += "Configuração de produção ausente"
}

# 4. Verifica se Docker está disponível
Write-Host "`n🐳 Verificando Docker..." -ForegroundColor Yellow

try {
    $dockerVersion = docker --version 2>$null
    if ($dockerVersion) {
        Write-Host "  ✅ Docker disponível: $dockerVersion" -ForegroundColor Green
    } else {
        Write-Host "  ❌ Docker não encontrado" -ForegroundColor Red
        $errorList += "Docker não está instalado ou não está no PATH"
    }
} catch {
    Write-Host "  ❌ Erro ao verificar Docker" -ForegroundColor Red
    $errorList += "Não foi possível verificar Docker"
}

try {
    $composeVersion = docker-compose --version 2>$null
    if ($composeVersion) {
        Write-Host "  ✅ Docker Compose disponível: $composeVersion" -ForegroundColor Green
    } else {
        Write-Host "  ❌ Docker Compose não encontrado" -ForegroundColor Red
        $errorList += "Docker Compose não está instalado"
    }
} catch {
    Write-Host "  ❌ Erro ao verificar Docker Compose" -ForegroundColor Red
    $errorList += "Não foi possível verificar Docker Compose"
}

# 5. Verifica se pode construir o projeto
Write-Host "`n🔨 Testando build do projeto..." -ForegroundColor Yellow

try {
    Push-Location "Src"
    $buildResult = dotnet build --configuration Release --verbosity quiet 2>&1
    Pop-Location
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  ✅ Build bem-sucedido" -ForegroundColor Green
    } else {
        Write-Host "  ❌ Falha no build" -ForegroundColor Red
        Write-Host "    $buildResult" -ForegroundColor Red
        $errorList += "Projeto não compila"
    }
} catch {
    Pop-Location
    Write-Host "  ❌ Erro durante o build" -ForegroundColor Red
    $errorList += "Erro ao tentar compilar o projeto"
}

# 6. Resultado final
Write-Host "`n📊 RESULTADO DA VERIFICAÇÃO" -ForegroundColor Cyan
Write-Host "===========================" -ForegroundColor Cyan

if ($errorList.Count -eq 0) {
    Write-Host "`n🎉 TUDO PRONTO PARA PRODUÇÃO!" -ForegroundColor Green
    Write-Host "`nPróximos passos:" -ForegroundColor Yellow
    Write-Host "1. Configure o arquivo .env com seus valores reais" -ForegroundColor White
    Write-Host "2. Execute: ./deploy.sh production" -ForegroundColor White
    Write-Host "3. Acesse: http://localhost para testar" -ForegroundColor White
    
    exit 0
} else {
    Write-Host "`n❌ PROBLEMAS ENCONTRADOS:" -ForegroundColor Red
    foreach ($errorItem in $errorList) {
        Write-Host "  • $errorItem" -ForegroundColor Red
    }
    Write-Host "`n⚠️  Corrija os problemas antes de fazer deploy" -ForegroundColor Yellow
    
    exit 1
}