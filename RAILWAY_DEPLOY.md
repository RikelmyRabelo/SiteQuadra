# 🚂 Deploy Railway - SiteQuadra

## 🚀 Deploy em 5 Minutos

### **Pré-requisitos**
- ✅ Node.js instalado
- ✅ Git configurado  
- ✅ Conta no Railway (gratuita)

### **Passo a Passo**

#### **1. 🔧 Preparar o projeto**
```bash
# Execute o script de preparação
.\deploy-railway.ps1
```

#### **2. 🌐 Criar conta Railway**
1. Acesse: [railway.app](https://railway.app)
2. Clique em **"Start a New Project"**
3. Faça login com GitHub (recomendado)

#### **3. 📱 Instalar Railway CLI**
```bash
# Via NPM
npm install -g @railway/cli

# Verificar instalação
railway --version
```

#### **4. 🔑 Fazer login**
```bash
railway login
```
- Abrirá o navegador para autenticação
- Autorize o acesso

#### **5. 🆕 Criar novo projeto**
```bash
railway new
```
- Escolha **"Empty Project"**
- Dê um nome ao projeto (ex: "sitequadra")

#### **6. 🚀 Fazer deploy**
```bash
railway up
```
- Railway detectará automaticamente o Dockerfile
- Aguarde o build e deploy (2-5 minutos)

#### **7. 🌐 Acessar aplicação**
```bash
railway open
```
- Abrirá seu site no navegador
- URL será algo como: `https://sitequadra-production.up.railway.app`

---

## 📋 **Comandos Úteis**

### **Monitoramento**
```bash
# Ver logs em tempo real
railway logs

# Status do projeto
railway status

# Informações do deployment
railway info
```

### **Configurações**
```bash
# Ver variáveis de ambiente
railway variables

# Adicionar variável
railway variables set KEY=value

# Configurar domínio customizado
railway domain
```

### **Desenvolvimento**
```bash
# Deploy nova versão
git add .
git commit -m "Update"
railway up

# Rollback para versão anterior
railway rollback
```

---

## 🔧 **Configurações Específicas**

### **Variáveis de Ambiente**
Railway detectará automaticamente:
- `PORT=8080`
- `ASPNETCORE_ENVIRONMENT=Production`

### **Domínio Gratuito**
Você receberá automaticamente:
- `https://[projeto].up.railway.app`
- SSL gratuito incluído

### **Banco de Dados**
- SQLite será persistente na Railway
- Backups automáticos inclusos
- Volume de 1GB gratuito

---

## 🔍 **Troubleshooting**

### **Build Fails**
```bash
# Ver logs detalhados
railway logs --deployment

# Verificar Dockerfile
railway shell
```

### **App não carrega**
```bash
# Verificar health check
curl https://[seu-app].up.railway.app/health

# Ver logs de runtime
railway logs --tail 100
```

### **Dados perdidos**
- Railway mantém volumes persistentes
- SQLite fica salvo em `/app/data/`
- Backups automáticos disponíveis

---

## 💰 **Limites do Plano Gratuito**

- ✅ **500 horas/mês** (suficiente para desenvolvimento)
- ✅ **512MB RAM**
- ✅ **1GB storage**
- ✅ **100GB bandwidth**
- ✅ **SSL gratuito**

### **Sleep Mode**
- App "dorme" após 15min de inatividade
- Acordar leva 10-30 segundos
- Para apps always-on, upgrade para plano pago ($5/mês)

---

## 🎯 **URLs Importantes**

### **Sua Aplicação**
- **Frontend**: `https://[projeto].up.railway.app/`
- **Admin**: `https://[projeto].up.railway.app/Admin/login.html`
- **API**: `https://[projeto].up.railway.app/api/`
- **Health**: `https://[projeto].up.railway.app/health`

### **Dashboard Railway**
- **Projeto**: `https://railway.app/project/[id]`
- **Logs**: `https://railway.app/project/[id]/deployments`
- **Metrics**: `https://railway.app/project/[id]/metrics`

---

## 🎉 **Pronto!**

Após o deploy, seu **SiteQuadra** estará disponível na internet!

**Compartilhe o link**: `https://[seu-projeto].up.railway.app`

### **Next Steps:**
1. 🔑 Acesse `/Admin/login.html` e use a senha gerada
2. 📝 Configure seu domínio customizado (opcional)
3. 📊 Monitore via Railway dashboard
4. 🚀 Compartilhe com os usuários!

**Problemas?** Verifique os logs com `railway logs` 🔍