#!/usr/bin/env pwsh

# Script para executar todos os testes do SiteQuadra
# Executa: .\run-tests.ps1

Write-Host "🧪 EXECUTANDO TESTES DO SITEQUADRA" -ForegroundColor Green
Write-Host "=================================" -ForegroundColor Green
Write-Host ""

# Verificar se o projeto de testes existe
if (-not (Test-Path "Tests\SiteQuadra.Tests.csproj")) {
    Write-Host "❌ Projeto de testes não encontrado!" -ForegroundColor Red
    Write-Host "   Esperado em: Tests\SiteQuadra.Tests.csproj" -ForegroundColor Yellow
    exit 1
}

# Limpar e restaurar dependências
Write-Host "🔄 Limpando e restaurando dependências..." -ForegroundColor Yellow
dotnet clean Tests\SiteQuadra.Tests.csproj --verbosity quiet
dotnet restore Tests\SiteQuadra.Tests.csproj --verbosity quiet

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Falha ao restaurar dependências!" -ForegroundColor Red
    exit 1
}

# Compilar o projeto
Write-Host "🔨 Compilando projeto de testes..." -ForegroundColor Yellow
dotnet build Tests\SiteQuadra.Tests.csproj --no-restore --verbosity quiet

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Falha na compilação!" -ForegroundColor Red
    exit 1
}

# Executar os testes
Write-Host "🏃‍♂️ Executando testes..." -ForegroundColor Yellow
Write-Host ""

# Executar com detalhes e cobertura
dotnet test Tests\SiteQuadra.Tests.csproj `
    --no-build `
    --verbosity normal `
    --logger "console;verbosity=detailed" `
    --collect:"XPlat Code Coverage" `
    --results-directory TestResults

$testExitCode = $LASTEXITCODE

Write-Host ""
Write-Host "=================================" -ForegroundColor Green

# Resultados
if ($testExitCode -eq 0) {
    Write-Host "✅ TODOS OS TESTES PASSARAM!" -ForegroundColor Green
    Write-Host "🎉 Sistema de agendamento funcionando corretamente!" -ForegroundColor Green
} else {
    Write-Host "❌ ALGUNS TESTES FALHARAM!" -ForegroundColor Red
    Write-Host "🔍 Verifique os logs acima para mais detalhes." -ForegroundColor Yellow
}

Write-Host ""
Write-Host "📊 Relatórios salvos em: TestResults\" -ForegroundColor Cyan

# Resumo dos testes implementados
Write-Host ""
Write-Host "🧪 TESTES IMPLEMENTADOS:" -ForegroundColor Magenta
Write-Host "  • Testes de Integração (HTTP/API):" -ForegroundColor White
Write-Host "    ✓ Agendamento com dados válidos" -ForegroundColor Gray
Write-Host "    ✓ Detecção de conflitos de horário" -ForegroundColor Gray
Write-Host "    ✓ Conflitos parciais" -ForegroundColor Gray
Write-Host "    ✓ Horários consecutivos (permitidos)" -ForegroundColor Gray
Write-Host "    ✓ Listagem de agendamentos" -ForegroundColor Gray
Write-Host "    ✓ Validações de campos obrigatórios" -ForegroundColor Gray
Write-Host ""
Write-Host "  • Testes Unitários (Controller):" -ForegroundColor White
Write-Host "    ✓ Lógica de negócio isolada" -ForegroundColor Gray
Write-Host "    ✓ Cálculo correto de DataHoraFim" -ForegroundColor Gray
Write-Host "    ✓ Cor padrão aplicada" -ForegroundColor Gray
Write-Host "    ✓ Diferentes cenários de conflito" -ForegroundColor Gray
Write-Host "    ✓ Validação de horários de funcionamento" -ForegroundColor Gray

Write-Host ""
exit $testExitCode