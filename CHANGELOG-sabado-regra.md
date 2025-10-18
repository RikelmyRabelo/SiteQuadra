# 🗓️ Nova Regra Especial para Sábados

## 📋 Resumo da Mudança

Implementada uma regra especial que quando o dia atual é **sábado**, o sistema permite agendamentos até o **sábado seguinte** (próximos 7 dias), em vez de bloquear apenas até o final da semana atual.

## 🎯 Problema Resolvido

- **Antes**: Se hoje fosse sábado, o sistema bloqueava agendamentos para a semana seguinte
- **Agora**: Se hoje for sábado, o sistema permite agendamentos até o sábado seguinte

## ⚙️ Implementação Técnica

### Função Utilitária Criada
```javascript
function calcularPeriodoPermitido() {
    const hoje = new Date();
    const hojeSemHoras = new Date(hoje.getFullYear(), hoje.getMonth(), hoje.getDate());
    
    let ultimoDiaPermitido;
    if (hoje.getDay() === 6) { // Se hoje é sábado (6)
        // Permite até o sábado seguinte (próximos 7 dias)
        ultimoDiaPermitido = new Date(hojeSemHoras);
        ultimoDiaPermitido.setDate(hojeSemHoras.getDate() + 7);
    } else {
        // Regra normal: até o final da semana atual
        ultimoDiaPermitido = new Date(hojeSemHoras);
        ultimoDiaPermitido.setDate(hojeSemHoras.getDate() + (6 - hojeSemHoras.getDay()));
    }
    
    return { hojeSemHoras, ultimoDiaPermitido };
}
```

### Arquivos Modificados
- `FronEnd/Calendar/calendar.js`
  - Adicionada função `calcularPeriodoPermitido()`
  - Refatorado `agendarButton.click()`
  - Refatorado lógica no `datesSet()`

## 🔄 Lógica de Funcionamento

| Dia da Semana | Permite Agendamento Até |
|---------------|------------------------|
| **Domingo (0)** | Sábado atual |
| **Segunda (1)** | Sábado atual |
| **Terça (2)** | Sábado atual |
| **Quarta (3)** | Sábado atual |
| **Quinta (4)** | Sábado atual |
| **Sexta (5)** | Sábado atual |
| **🎯 Sábado (6)** | **Sábado seguinte** |

## ✅ Benefícios

1. **Flexibilidade**: Usuários podem agendar com uma semana de antecedência quando for sábado
2. **Experiência melhorada**: Não há mais bloqueios desnecessários aos sábados
3. **Código limpo**: Função utilitária evita duplicação
4. **Manutenibilidade**: Regra centralizada e fácil de modificar

## 🧪 Como Testar

1. **Simular um sábado**:
   - Altere a data do sistema para um sábado
   - Acesse o calendário
   - Verifique se consegue agendar para a semana seguinte

2. **Testar outros dias**:
   - Confirme que a regra normal continua funcionando
   - Apenas sábados devem ter a regra especial

## 📅 Data da Implementação
18 de outubro de 2025

---
*Esta funcionalidade melhora significativamente a experiência do usuário ao usar o sistema de agendamento aos sábados.*