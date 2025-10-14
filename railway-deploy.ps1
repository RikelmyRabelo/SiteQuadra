# Script de Deploy para Railway - SiteQuadra
Write-Host "DEPLOY RAILWAY - SiteQuadra" -ForegroundColor Cyan
Write-Host "==============================" -ForegroundColor Cyan

# 1. Verificacao pre-deploy
Write-Host "`nVerificando requisitos..." -ForegroundColor Yellow

# Verifica se Git esta configurado
try {
    $gitStatus = git status 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  Git configurado" -ForegroundColor Green
    } else {
        Write-Host "  Inicializando repositorio Git..." -ForegroundColor Yellow
        git init
        Write-Host "  Git inicializado" -ForegroundColor Green
    }
} catch {
    Write-Host "  Git nao esta disponivel" -ForegroundColor Red
    exit 1
}

# Verifica se Railway CLI esta instalado
Write-Host "`nVerificando Railway CLI..." -ForegroundColor Yellow
$railwayInstalled = $false
try {
    $railwayVersion = railway --version 2>$null
    if ($railwayVersion) {
        Write-Host "  Railway CLI: $railwayVersion" -ForegroundColor Green
        $railwayInstalled = $true
    }
} catch {
    Write-Host "  Railway CLI nao encontrado" -ForegroundColor Yellow
}

if (-not $railwayInstalled) {
    Write-Host "  Tentando instalar Railway CLI..." -ForegroundColor Yellow
    try {
        npm install -g @railway/cli
        Write-Host "  Railway CLI instalado!" -ForegroundColor Green
    } catch {
        Write-Host "  Nao foi possivel instalar Railway CLI automaticamente" -ForegroundColor Red
        Write-Host "  Instale manualmente: npm install -g @railway/cli" -ForegroundColor Yellow
    }
}

# 2. Prepara arquivos para deploy
Write-Host "`nPreparando arquivos..." -ForegroundColor Yellow

# Copia Dockerfile otimizado se existir
if (Test-Path "Dockerfile.railway") {
    Copy-Item "Dockerfile.railway" "Dockerfile" -Force
    Write-Host "  Dockerfile Railway configurado" -ForegroundColor Green
} else {
    Write-Host "  Usando Dockerfile existente" -ForegroundColor Green
}

# 3. Adiciona arquivos ao Git
Write-Host "`nAdicionando arquivos ao Git..." -ForegroundColor Yellow
git add .
$commitResult = git commit -m "Preparando deploy Railway" 2>$null
if ($LASTEXITCODE -eq 0) {
    Write-Host "  Arquivos commitados" -ForegroundColor Green
} else {
    Write-Host "  Nenhuma alteracao para commitar" -ForegroundColor Yellow
}

# 4. Instrucoes de deploy
Write-Host "`nPROXIMOS PASSOS:" -ForegroundColor Cyan
Write-Host "=================" -ForegroundColor Cyan

Write-Host "`n1. Faca login no Railway:" -ForegroundColor Yellow
Write-Host "   railway login" -ForegroundColor White

Write-Host "`n2. Crie um novo projeto:" -ForegroundColor Yellow
Write-Host "   railway new" -ForegroundColor White

Write-Host "`n3. Configure as variaveis de ambiente:" -ForegroundColor Yellow
Write-Host "   railway variables set ASPNETCORE_ENVIRONMENT=Production" -ForegroundColor White

Write-Host "`n4. Faca o deploy:" -ForegroundColor Yellow
Write-Host "   railway up" -ForegroundColor White

Write-Host "`n5. Abra no navegador:" -ForegroundColor Yellow
Write-Host "   railway open" -ForegroundColor White

Write-Host "`nCOMANDOS UTEIS:" -ForegroundColor Cyan
Write-Host "   railway logs       # Ver logs" -ForegroundColor Gray
Write-Host "   railway status     # Status do projeto" -ForegroundColor Gray
Write-Host "   railway variables  # Gerenciar variaveis" -ForegroundColor Gray
Write-Host "   railway domain     # Configurar dominio" -ForegroundColor Gray

Write-Host "`nTUDO PRONTO PARA RAILWAY!" -ForegroundColor Green
Write-Host "Execute os comandos acima na ordem para fazer deploy." -ForegroundColor White