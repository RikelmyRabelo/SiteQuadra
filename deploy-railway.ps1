# Script de Deploy para Railway
Write-Host "🚂 DEPLOY RAILWAY - SiteQuadra" -ForegroundColor Cyan
Write-Host "===============================" -ForegroundColor Cyan

# 1. Verificação pré-deploy
Write-Host "`n🔍 Verificando requisitos..." -ForegroundColor Yellow

# Verifica se Railway CLI está instalado
try {
    $railwayVersion = railway --version 2>$null
    if ($railwayVersion) {
        Write-Host "  ✅ Railway CLI: $railwayVersion" -ForegroundColor Green
    } else {
        Write-Host "  ❌ Railway CLI não encontrado" -ForegroundColor Red
        Write-Host "`n📥 Instalando Railway CLI..." -ForegroundColor Yellow
        
        # Instala Railway CLI
        npm install -g @railway/cli
        
        Write-Host "  ✅ Railway CLI instalado!" -ForegroundColor Green
    }
} catch {
    Write-Host "  ⚠️  Tentando instalar Railway CLI..." -ForegroundColor Yellow
    try {
        npm install -g @railway/cli
        Write-Host "  ✅ Railway CLI instalado!" -ForegroundColor Green
    } catch {
        Write-Host "  ❌ Não foi possível instalar Railway CLI" -ForegroundColor Red
        Write-Host "    Instale manualmente: npm install -g @railway/cli" -ForegroundColor Red
        exit 1
    }
}

# Verifica se Git está configurado
try {
    $gitStatus = git status 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  ✅ Repositório Git configurado" -ForegroundColor Green
    } else {
        Write-Host "  ⚠️  Inicializando repositório Git..." -ForegroundColor Yellow
        git init
        Write-Host "  ✅ Git inicializado" -ForegroundColor Green
    }
} catch {
    Write-Host "  ❌ Git não está disponível" -ForegroundColor Red
    exit 1
}

# 2. Prepara arquivos para deploy
Write-Host "`n📁 Preparando arquivos..." -ForegroundColor Yellow

# Copia Dockerfile otimizado
Copy-Item "Dockerfile.railway" "Dockerfile" -Force
Write-Host "  ✅ Dockerfile Railway configurado" -ForegroundColor Green

# Verifica se .gitignore está correto
if (-not (Test-Path ".gitignore")) {
    Write-Host "  ⚠️  Criando .gitignore..." -ForegroundColor Yellow
    Copy-Item ".gitignore" ".gitignore.bak" -ErrorAction SilentlyContinue
}

# Adiciona arquivos ao Git
Write-Host "`n📤 Adicionando arquivos ao Git..." -ForegroundColor Yellow
git add .
git commit -m "🚂 Preparando deploy Railway" 2>$null

Write-Host "  ✅ Arquivos preparados para deploy" -ForegroundColor Green

# 3. Instrucões de deploy
Write-Host "`n🚀 PRÓXIMOS PASSOS:" -ForegroundColor Cyan
Write-Host "===================" -ForegroundColor Cyan

Write-Host "`n1️⃣  Faça login no Railway:" -ForegroundColor Yellow
Write-Host "   railway login" -ForegroundColor White

Write-Host "`n2️⃣  Crie um novo projeto:" -ForegroundColor Yellow
Write-Host "   railway new" -ForegroundColor White

Write-Host "`n3️⃣  Faça o deploy:" -ForegroundColor Yellow
Write-Host "   railway up" -ForegroundColor White

Write-Host "`n4️⃣  Abra no navegador:" -ForegroundColor Yellow
Write-Host "   railway open" -ForegroundColor White

Write-Host "`n📋 COMANDOS ÚTEIS:" -ForegroundColor Cyan
Write-Host "   railway logs       # Ver logs" -ForegroundColor Gray
Write-Host "   railway status     # Status do projeto" -ForegroundColor Gray
Write-Host "   railway variables  # Gerenciar variáveis" -ForegroundColor Gray
Write-Host "   railway domain     # Configurar domínio" -ForegroundColor Gray

Write-Host "`n🎉 TUDO PRONTO PARA RAILWAY!" -ForegroundColor Green
Write-Host "Execute os comandos acima na ordem para fazer deploy." -ForegroundColor White