# 🚀 Guia de Deploy - SiteQuadra

## 📋 Pré-requisitos

### Sistema Operacional
- ✅ **Linux** (Ubuntu 20.04+ recomendado)
- ✅ **Windows** (com Docker Desktop)
- ✅ **macOS** (com Docker Desktop)

### Software Necessário
- 🐳 **Docker** (20.10+)
- 🔧 **Docker Compose** (2.0+)
- 🌐 **Nginx** (opcional, para proxy reverso)
- 🔒 **Certificado SSL** (para HTTPS)

## ⚡ Deploy Rápido

### 1. Preparação
```bash
# Clone o repositório
git clone <seu-repositorio>
cd SiteQuadra

# Configure variáveis de ambiente
cp .env.example .env
nano .env  # Configure seus valores
```

### 2. Deploy para Desenvolvimento
```bash
./deploy.sh development
```

### 3. Deploy para Produção
```bash
./deploy.sh production
```

## 🔧 Configuração Detalhada

### Arquivo `.env`
Configure as seguintes variáveis:

```env
# Domínio da aplicação
ALLOWED_ORIGINS=https://seudominio.com,https://www.seudominio.com

# Certificado SSL
CERTIFICATE_PASSWORD=sua_senha_segura_aqui

# Portas (padrão: 80 e 443)
HTTP_PORT=80
HTTPS_PORT=443
```

### Certificado SSL
1. **Obtenha um certificado SSL**:
   ```bash
   # Let's Encrypt (gratuito)
   certbot certonly --standalone -d seudominio.com
   
   # Ou use seu certificado existente
   ```

2. **Converta para formato .pfx** (se necessário):
   ```bash
   openssl pkcs12 -export -out sitequadra.pfx \
     -inkey privkey.pem -in cert.pem \
     -password pass:sua_senha_aqui
   ```

3. **Coloque o certificado** em `./certificates/sitequadra.pfx`

## 🐳 Comandos Docker

### Comandos Básicos
```bash
# Iniciar aplicação
./deploy.sh production

# Ver logs
./deploy.sh logs

# Ver status
./deploy.sh status

# Parar aplicação
./deploy.sh stop
```

### Comandos Avançados
```bash
# Deploy com monitoramento
./deploy.sh monitoring

# Acessar container
docker exec -it sitequadra-app bash

# Backup manual do banco
docker exec sitequadra-app sqlite3 /app/data/quadra.db ".backup /app/Backups/manual_backup.db"
```

## 🏥 Monitoramento e Saúde

### Health Checks
- **Endpoint**: `/health`
- **Verificação automática** a cada 30 segundos
- **Timeout**: 10 segundos
- **Retry**: 3 tentativas

### Logs
```bash
# Logs em tempo real
docker logs -f sitequadra-app

# Logs com timestamp
docker logs -t sitequadra-app

# Últimas 100 linhas
docker logs --tail=100 sitequadra-app
```

### Métricas (com perfil monitoring)
- **Grafana**: http://localhost:3000 (admin/admin)
- **Prometheus**: http://localhost:9090

## 🔒 Segurança em Produção

### Configurações Aplicadas Automaticamente
- ✅ **Headers de segurança**: XSS Protection, CSRF, etc.
- ✅ **CORS restritivo**: Apenas domínios configurados
- ✅ **HTTPS forçado**: Redirecionamento automático
- ✅ **Usuário não-root**: Container roda como appuser
- ✅ **Logs de segurança**: Monitoramento de tentativas suspeitas

### Recomendações Adicionais
- 🔐 **Firewall**: Feche portas desnecessárias
- 🛡️ **Fail2Ban**: Proteja contra força bruta
- 📊 **Monitoramento**: Configure alertas
- 🔄 **Backup**: Automatizado diariamente

## 📁 Estrutura de Arquivos

```
SiteQuadra/
├── data/              # Banco de dados SQLite
├── logs/              # Logs da aplicação  
├── backups/           # Backups automáticos
├── certificates/      # Certificados SSL
├── monitoring/        # Configurações do Prometheus/Grafana
├── Dockerfile         # Container da aplicação
├── docker-compose.yml # Orquestração
├── deploy.sh         # Script de deploy
└── .env              # Variáveis de ambiente
```

## 🚨 Troubleshooting

### Problemas Comuns

#### Container não inicia
```bash
# Verificar logs
docker logs sitequadra-app

# Verificar configuração
./deploy.sh status

# Recriar container
docker-compose down && docker-compose up --build -d
```

#### Erro de certificado SSL
```bash
# Verificar se arquivo existe
ls -la certificates/

# Testar certificado
openssl pkcs12 -info -in certificates/sitequadra.pfx -noout
```

#### Banco de dados corrompido
```bash
# Restaurar backup mais recente
docker exec sitequadra-app ls /app/Backups/
docker cp sitequadra-app:/app/Backups/backup_mais_recente.db ./
docker cp ./backup_mais_recente.db sitequadra-app:/app/data/quadra.db
docker restart sitequadra-app
```

#### Performance ruim
```bash
# Verificar recursos
docker stats sitequadra-app

# Ajustar limites no docker-compose.yml
resources:
  limits:
    memory: 1G
    cpus: '2.0'
```

## 🔄 Atualizações

### Processo de Atualização
```bash
# 1. Backup automático é criado
./deploy.sh production

# 2. Zero downtime com rolling update
docker-compose up --build -d --no-deps sitequadra

# 3. Verificar se está funcionando
curl -f http://localhost/health
```

## 📞 Suporte

### Logs Importantes
- **Aplicação**: `docker logs sitequadra-app`
- **Sistema**: `./logs/sitequadra-*.log`
- **Segurança**: Filtrar por "🔐" e "🚨" nos logs

### Comandos de Diagnóstico
```bash
# Status completo
./deploy.sh status

# Uso de recursos
docker stats --no-stream

# Conexões de rede
netstat -tulpn | grep :80
netstat -tulpn | grep :443

# Espaço em disco
df -h
du -sh data/ logs/ backups/
```

---

**Sistema preparado para produção com segurança enterprise! 🛡️**