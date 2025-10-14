# WARP.md

Este arquivo fornece orientações ao WARP (warp.dev) ao trabalhar com código neste repositório.

## 🏟️ Sobre o Projeto

SiteQuadra é um sistema web de agendamento de quadras esportivas desenvolvido em ASP.NET Core 8.0 com frontend HTML/CSS/JavaScript puro. O sistema permite que usuários façam agendamentos de quadra através de uma interface visual de calendário e inclui um painel administrativo para gestão.

## ⚡ Comandos de Desenvolvimento

### Comandos Principais

```bash
# Executar aplicação em desenvolvimento
dotnet run --project Src/SiteQuadra.csproj

# Compilar o projeto
dotnet build Src/SiteQuadra.csproj

# Restaurar dependências
dotnet restore Src/SiteQuadra.csproj

# Limpar artefatos de build
dotnet clean
```

### Comandos de Banco de Dados

```bash
# Criar nova migração
dotnet ef migrations add NomeDaMigracao --project Src --startup-project Src

# Aplicar migrações
dotnet ef database update --project Src --startup-project Src

# Reverter última migração
dotnet ef database update NomeMigracaoAnterior --project Src --startup-project Src

# Resetar banco (desenvolvimento)
Remove-Item Src/quadra.db -Force
dotnet ef database update --project Src --startup-project Src
```

### Deploy e Produção

```bash
# Deploy Docker local
docker-compose up --build -d

# Deploy Railway
.\deploy-railway.ps1
railway login
railway new
railway up

# Verificar logs
docker logs sitequadra-app
railway logs

# Parar aplicação
docker-compose down
```

## 🏗️ Arquitetura do Sistema

### Estrutura Backend (.NET Core)

**Controllers**
- `AgendamentosController`: API REST para gerenciar agendamentos (CRUD)
- `AdminController`: Endpoints administrativos protegidos por autenticação
- `HealthController`: Health checks para monitoring

**Models**
- `Agendamento`: Entidade principal com ID, nome, contato, cidade/bairro, data/hora início/fim, cor

**Data Layer**
- `QuadraContext`: DbContext do Entity Framework para SQLite
- `DataContextFactory`: Factory para criação de contexto em design-time
- Migrations: Controle de versão do esquema do banco

**Services**
- `AdminSecurityService`: Gerencia autenticação administrativa via tokens Bearer
- `BackupService`: Backup automático diário do banco SQLite
- `ConfigurationValidationService`: Valida configurações na inicialização
- `DatabaseInitializationService`: Inicializa e popula banco com dados padrão

**Middleware**
- `AdminAuthMiddleware`: Protege rotas `/api/admin/*` com validação de token
- `SecurityLoggingMiddleware`: Log de tentativas de acesso para auditoria

### Estrutura Frontend (HTML/CSS/JS)

**Páginas Principais**
- `FronEnd/index.html`: Página inicial com calendário interativo
- `FronEnd/Calendar/`: Componente de calendário visual (FullCalendar.js)
- `FronEnd/Booking/`: Modal de agendamento com validações
- `FronEnd/Admin/`: Painel administrativo com dashboard e gestão

**Organização**
- Cada módulo (Calendar, Booking, Admin) tem seus próprios HTML, CSS e JS
- CSS com design responsivo e gradientes modernos
- JavaScript puro (sem frameworks), usando Fetch API para comunicação

### Regras de Negócio Importantes

**Agendamentos**
- Duração fixa de 1 hora por agendamento
- Não permite agendamentos além da semana corrente
- Não permite agendamentos em datas passadas
- Detecção automática de conflitos de horário
- Cor padrão azul (#3788d8) aplicada automaticamente

**Segurança**
- Senha administrativa configurável (padrão: admin123)
- Tokens Bearer com expiração para sessões admin
- CORS restritivo em produção, permissivo em desenvolvimento
- Headers de segurança aplicados automaticamente

**Dados**
- SQLite como banco de dados com backup automático
- Migrações controladas pelo Entity Framework
- Dados persistidos em volume Docker para produção

## 🔧 Configurações Específicas

### Variáveis de Ambiente
- `ASPNETCORE_ENVIRONMENT`: Development/Production
- `RAILWAY_ENVIRONMENT`: Detecta deploy Railway
- `AdminPassword`: Senha do painel administrativo

### Arquivos de Configuração
- `appsettings.json`: Configurações base
- `appsettings.Development.json`: Configurações de desenvolvimento
- `appsettings.Railway.json`: Otimizações para Railway

### Deploy
- **Local**: Docker Compose com volumes persistentes
- **Railway**: Dockerfile.railway otimizado com Nixpacks
- **Produção**: Suporte a SSL, backup automático, monitoring

## 💡 Padrões de Desenvolvimento

### Convenções de Código
- Usar `async/await` para operações de banco
- Validações no controller com retornos HTTP semânticos
- Logs estruturados com níveis apropriados
- Middleware para concerns transversais (auth, logging, security)

### Estrutura de Arquivos
- Controllers organizados por domínio
- Services para lógica de negócio complexa
- Middleware para interceptação de requisições
- Frontend modularizado por funcionalidade

### Banco de Dados
- Entity Framework Code-First com migrações
- Sempre usar transações para operações críticas
- Backup automático diário em produção
- Índices apropriados para consultas frequentes

### Deploy e DevOps
- Containerização com Docker multistage build
- Health checks configurados
- Volumes persistentes para dados
- Scripts PowerShell para automação de deploy

## 🚨 Pontos de Atenção

- O projeto usa PowerShell para scripts de automação (Windows-friendly)
- Configurações de CORS diferem entre desenvolvimento e produção
- Railway detecta automaticamente via variável de ambiente
- Backup automático ativado apenas em produção
- Migrations devem ser aplicadas automaticamente na inicialização
- Validação de datas passadas ativa em produção
