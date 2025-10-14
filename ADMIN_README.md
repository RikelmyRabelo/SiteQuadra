# 📊 Painel Administrativo - Sistema de Agendamento da Quadra

## 🔐 Como Acessar o Painel Administrativo

### 1. Acesso via Interface Web
- Na página principal (`index.html`), clique no link **"🔐 Área Administrativa"** no cabeçalho
- Ou acesse diretamente: `admin-login.html`

### 2. Credenciais de Acesso
- **Senha padrão:** `admin123`
- A senha pode ser alterada no arquivo `appsettings.json` na propriedade `AdminPassword`

## 🛠️ Funcionalidades do Painel

### 📈 Dashboard Principal
- **Estatísticas Rápidas:**
  - Total de agendamentos no sistema
  - Agendamentos da semana atual
  - Agendamentos do dia atual
- **Atualização automática** das estatísticas a cada 30 segundos

### 📅 Gestão de Agendamentos
- **Visualizar todos os agendamentos** com informações completas:
  - Nome do responsável
  - Data e horário
  - Contato (telefone)
  - Cidade/Bairro
- **Remover agendamentos** com confirmação de segurança
- **Atualização em tempo real** após modificações

### 🔄 Controles Administrativos
- **Botão Atualizar:** Recarrega todos os dados manualmente
- **Botão Sair:** Encerra a sessão administrativa

## 🔧 Configuração Técnica

### Arquivo de Configuração
```json
{
  "AdminPassword": "sua_senha_aqui"
}
```

### Endpoints da API
- `POST /api/admin/login` - Autenticação
- `GET /api/admin/agendamentos` - Listar todos os agendamentos
- `GET /api/admin/estatisticas` - Obter estatísticas
- `DELETE /api/admin/agendamentos/{id}` - Remover agendamento

### Segurança
- **Autenticação por token Bearer** em todas as rotas administrativas
- **Middleware de segurança** protege automaticamente rotas `/api/admin/*`
- **Verificação automática** de validade do token
- **Redirecionamento automático** para login quando token expira

## 📱 Interface Responsiva

O painel administrativo é totalmente responsivo e funciona bem em:
- 💻 **Desktop** - Experiência completa
- 📱 **Tablet** - Layout adaptado
- 📱 **Mobile** - Interface otimizada para telas pequenas

## 🎨 Recursos Visuais

### Temas e Cores
- **Design moderno** com gradientes e animações suaves
- **Feedback visual** para todas as ações
- **Modais de confirmação** para ações destrutivas
- **Estados de carregamento** com spinners animados

### Animações
- **Transições suaves** em todos os elementos
- **Efeitos hover** nos botões e cards
- **Animações de entrada** nos modais
- **Feedback tátil** nas interações

## 🚀 Como Usar

### Primeiro Acesso
1. Acesse a página principal do sistema
2. Clique em "🔐 Área Administrativa"
3. Digite a senha: `admin123` (ou a configurada)
4. Clique em "Entrar"

### Gerenciando Agendamentos
1. No dashboard, visualize as estatísticas
2. Role para baixo para ver todos os agendamentos
3. Para remover um agendamento:
   - Clique no botão "🗑️ Remover"
   - Confirme a ação no modal de confirmação
   - O agendamento será removido e as estatísticas atualizadas

### Dicas de Uso
- **Use o botão "🔄 Atualizar"** para sincronizar com novos dados
- **As estatísticas se atualizam automaticamente** a cada 30 segundos
- **Clique fora dos modais** para fechá-los rapidamente
- **Use "🚪 Sair"** para encerrar a sessão com segurança

## 🔒 Segurança e Manutenção

### Alterando a Senha
1. Edite o arquivo `Src/appsettings.json`
2. Modifique o valor de `AdminPassword`
3. Reinicie o servidor
4. A nova senha será aplicada imediatamente

### Monitoramento
- Todos os acessos e ações são logados no console do servidor
- Tokens inválidos são automaticamente rejeitados
- Sistema detecta e remove tokens expirados automaticamente

## 📞 Suporte

Em caso de problemas:
1. Verifique se o servidor backend está rodando
2. Confirme se a senha está correta no `appsettings.json`
3. Verifique o console do navegador para erros de JavaScript
4. Reinicie o servidor se necessário

---

**Sistema desenvolvido para gestão eficiente da Quadra Municipal** 🏟️