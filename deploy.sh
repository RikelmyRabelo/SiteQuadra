#!/bin/bash
# Script de deploy para SiteQuadra
# Uso: ./deploy.sh [development|production]

set -e  # Para execução em caso de erro

ENVIRONMENT=${1:-production}
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

echo "🚀 Iniciando deploy do SiteQuadra - Ambiente: $ENVIRONMENT"

# Função para log colorido
log_info() {
    echo -e "\e[34m[INFO]\e[0m $1"
}

log_success() {
    echo -e "\e[32m[SUCESSO]\e[0m $1"
}

log_warning() {
    echo -e "\e[33m[AVISO]\e[0m $1"
}

log_error() {
    echo -e "\e[31m[ERRO]\e[0m $1"
}

# Verificações pré-deploy
check_requirements() {
    log_info "Verificando requisitos..."
    
    # Verifica se Docker está instalado
    if ! command -v docker &> /dev/null; then
        log_error "Docker não está instalado!"
        exit 1
    fi
    
    # Verifica se Docker Compose está instalado
    if ! command -v docker-compose &> /dev/null; then
        log_error "Docker Compose não está instalado!"
        exit 1
    fi
    
    # Verifica se o arquivo .env existe
    if [[ ! -f .env ]]; then
        log_warning "Arquivo .env não encontrado!"
        if [[ -f .env.example ]]; then
            log_info "Copiando .env.example para .env"
            cp .env.example .env
            log_warning "IMPORTANTE: Configure o arquivo .env com seus valores reais!"
            read -p "Pressione Enter após configurar o .env..."
        else
            log_error "Arquivo .env.example também não encontrado!"
            exit 1
        fi
    fi
    
    log_success "Todos os requisitos verificados"
}

# Backup do banco atual (se existir)
backup_database() {
    if [[ -f "data/quadra.db" ]]; then
        log_info "Fazendo backup do banco de dados..."
        backup_file="data/quadra_pre_deploy_$(date +%Y%m%d_%H%M%S).db"
        cp "data/quadra.db" "$backup_file"
        log_success "Backup criado: $backup_file"
    fi
}

# Deploy para desenvolvimento
deploy_development() {
    log_info "Deploy para desenvolvimento..."
    
    # Para containers existentes
    docker-compose down 2>/dev/null || true
    
    # Build e start
    docker-compose up --build -d
    
    log_success "Deploy de desenvolvimento concluído!"
    log_info "Aplicação disponível em:"
    log_info "  HTTP:  http://localhost"
    log_info "  HTTPS: https://localhost (se certificado configurado)"
}

# Deploy para produção
deploy_production() {
    log_info "Deploy para produção..."
    
    # Verificações adicionais para produção
    if grep -q "seudominio.com" .env; then
        log_error "Configure o domínio real no arquivo .env!"
        exit 1
    fi
    
    if grep -q "sua_senha" .env; then
        log_error "Configure senhas reais no arquivo .env!"
        exit 1
    fi
    
    # Cria diretórios necessários
    mkdir -p data logs backups certificates
    
    # Para containers existentes
    docker-compose down 2>/dev/null || true
    
    # Remove imagens antigas
    log_info "Removendo imagens antigas..."
    docker image prune -f
    
    # Build e start
    log_info "Construindo e iniciando containers..."
    docker-compose up --build -d
    
    # Aguarda aplicação estar pronta
    log_info "Aguardando aplicação ficar pronta..."
    sleep 30
    
    # Verifica se está funcionando
    if curl -f http://localhost/health &> /dev/null; then
        log_success "Health check passou!"
    else
        log_warning "Health check falhou - verifique os logs"
        show_logs
    fi
    
    log_success "Deploy de produção concluído!"
    log_info "Aplicação disponível em:"
    log_info "  HTTP:  http://seu-servidor"
    log_info "  HTTPS: https://seu-servidor (se certificado configurado)"
}

# Deploy com monitoramento
deploy_with_monitoring() {
    log_info "Deploy com monitoramento..."
    
    mkdir -p monitoring/grafana/dashboards monitoring/grafana/datasources
    
    # Configura Prometheus
    cat > monitoring/prometheus.yml <<EOF
global:
  scrape_interval: 15s

scrape_configs:
  - job_name: 'sitequadra'
    static_configs:
      - targets: ['sitequadra:5000']
    metrics_path: '/metrics'
    scrape_interval: 10s
EOF
    
    docker-compose --profile monitoring up --build -d
    
    log_success "Deploy com monitoramento concluído!"
    log_info "Monitoramento disponível em:"
    log_info "  Grafana:    http://localhost:3000 (admin/admin)"
    log_info "  Prometheus: http://localhost:9090"
}

# Mostra logs
show_logs() {
    log_info "Últimos logs da aplicação:"
    docker-compose logs --tail=50 sitequadra
}

# Função de status
show_status() {
    log_info "Status dos containers:"
    docker-compose ps
    
    log_info "Uso de recursos:"
    docker stats --no-stream sitequadra-app 2>/dev/null || log_warning "Container não está rodando"
}

# Função de parada
stop_application() {
    log_info "Parando aplicação..."
    docker-compose down
    log_success "Aplicação parada"
}

# Menu principal
case "$ENVIRONMENT" in
    "development"|"dev")
        check_requirements
        backup_database
        deploy_development
        ;;
    "production"|"prod")
        check_requirements
        backup_database
        deploy_production
        ;;
    "monitoring"|"mon")
        check_requirements
        backup_database
        deploy_with_monitoring
        ;;
    "logs")
        show_logs
        ;;
    "status")
        show_status
        ;;
    "stop")
        stop_application
        ;;
    *)
        echo "Uso: $0 [development|production|monitoring|logs|status|stop]"
        echo ""
        echo "Comandos:"
        echo "  development  - Deploy para desenvolvimento"
        echo "  production   - Deploy para produção"
        echo "  monitoring   - Deploy com Prometheus/Grafana"
        echo "  logs         - Mostra logs da aplicação"
        echo "  status       - Mostra status dos containers"
        echo "  stop         - Para a aplicação"
        exit 1
        ;;
esac