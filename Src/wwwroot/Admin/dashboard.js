document.addEventListener('DOMContentLoaded', function() {
    // Elementos da página
    const refreshBtn = document.getElementById('refresh-btn');
    const logoutBtn = document.getElementById('logout-btn');
    const loadingDiv = document.getElementById('loading');
    const agendamentosContainer = document.getElementById('agendamentos-container');
    const emptyMessage = document.getElementById('empty-message');
    
    // Elementos das estatísticas
    const totalAgendamentos = document.getElementById('total-agendamentos');
    const agendamentosSemana = document.getElementById('agendamentos-semana');
    const agendamentosHoje = document.getElementById('agendamentos-hoje');
    
    // Elementos dos modais
    const confirmModal = document.getElementById('confirm-modal');
    const confirmDetails = document.getElementById('confirm-details');
    const confirmRemove = document.getElementById('confirm-remove');
    const cancelRemove = document.getElementById('cancel-remove');
    
    const feedbackModal = document.getElementById('feedback-modal');
    const feedbackTitle = document.getElementById('feedback-title');
    const feedbackMessage = document.getElementById('feedback-message');
    const feedbackClose = document.getElementById('feedback-close');
    
    let currentAgendamentoToRemove = null;
    
    // Verifica autenticação na inicialização
    checkAuth();
    
    function checkAuth() {
        const token = localStorage.getItem('admin_token');
        if (!token) {
            redirectToLogin();
            return;
        }
        
        // Carrega os dados iniciais
        loadAllData();
    }
    
    function redirectToLogin() {
        window.location.href = 'login.html';
    }
    
    function getAuthHeaders() {
        const token = localStorage.getItem('admin_token');
        return {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        };
    }
    
    function handleAuthError(response) {
        if (response.status === 401) {
            localStorage.removeItem('admin_token');
            redirectToLogin();
            return true;
        }
        return false;
    }
    
    async function loadAllData() {
        await Promise.all([
            loadEstatisticas(),
            loadAgendamentos()
        ]);
    }
    
    async function loadEstatisticas() {
        try {
            const response = await fetch('/api/admin/estatisticas', {
                method: 'GET',
                headers: getAuthHeaders()
            });
            
            if (handleAuthError(response)) return;
            
            if (response.ok) {
                const stats = await response.json();
                totalAgendamentos.textContent = stats.totalAgendamentos;
                agendamentosSemana.textContent = stats.agendamentosSemana;
                agendamentosHoje.textContent = stats.agendamentosHoje;
            }
        } catch (error) {
            console.error('Erro ao carregar estatísticas:', error);
        }
    }
    
    async function loadAgendamentos() {
        showLoading(true);
        
        try {
            const response = await fetch('/api/admin/agendamentos', {
                method: 'GET',
                headers: getAuthHeaders()
            });
            
            if (handleAuthError(response)) return;
            
            if (response.ok) {
                const agendamentos = await response.json();
                displayAgendamentos(agendamentos);
            } else {
                showFeedback('Erro', 'Não foi possível carregar os agendamentos', 'error');
            }
        } catch (error) {
            console.error('Erro ao carregar agendamentos:', error);
            showFeedback('Erro de Conexão', 'Não foi possível se comunicar com o servidor', 'error');
        }
        
        showLoading(false);
    }
    
    function showLoading(show) {
        loadingDiv.style.display = show ? 'block' : 'none';
        agendamentosContainer.style.display = show ? 'none' : 'block';
    }
    
    function displayAgendamentos(agendamentos) {
        if (agendamentos.length === 0) {
            agendamentosContainer.style.display = 'none';
            emptyMessage.style.display = 'block';
            return;
        }
        
        emptyMessage.style.display = 'none';
        agendamentosContainer.style.display = 'block';
        agendamentosContainer.innerHTML = '';
        
        agendamentos.forEach(agendamento => {
            const card = createAgendamentoCard(agendamento);
            agendamentosContainer.appendChild(card);
        });
    }
    
    function createAgendamentoCard(agendamento) {
        const card = document.createElement('div');
        card.className = 'agendamento-card';
        card.innerHTML = `
            <div class="agendamento-header">
                <h3>👤 ${agendamento.nomeResponsavel}</h3>
                <button class="btn-remove" data-id="${agendamento.id}" data-nome="${agendamento.nomeResponsavel}" data-horario="${agendamento.horarioFormatado || 'N/A'}" data-data="${agendamento.dataFormatada || 'N/A'}">
                    🗑️ Remover
                </button>
            </div>
            <div class="agendamento-details">
                <div class="detail-row">
                    <span class="detail-label">📅 Data:</span>
                    <span class="detail-value">${agendamento.dataFormatada || 'N/A'}</span>
                </div>
                <div class="detail-row">
                    <span class="detail-label">⏰ Horário:</span>
                    <span class="detail-value">${agendamento.horarioFormatado || 'N/A'}</span>
                </div>
                <div class="detail-row">
                    <span class="detail-label">📞 Contato:</span>
                    <span class="detail-value">${agendamento.contato}</span>
                </div>
                <div class="detail-row">
                    <span class="detail-label">🏘️ Cidade/Bairro:</span>
                    <span class="detail-value">${agendamento.cidadeBairro}</span>
                </div>
            </div>
        `;
        
        // Adiciona evento de clique no botão de remover
        const removeBtn = card.querySelector('.btn-remove');
        removeBtn.addEventListener('click', function() {
            showConfirmModal(agendamento);
        });
        
        return card;
    }
    
    function showConfirmModal(agendamento) {
        console.log('💬 Abrindo modal de confirmação para:', agendamento);
        currentAgendamentoToRemove = agendamento;
        
        confirmDetails.innerHTML = `
            <strong>${agendamento.nomeResponsavel}</strong><br>
            ${agendamento.dataFormatada || 'N/A'} às ${agendamento.horarioFormatado || 'N/A'}
        `;
        
        confirmModal.style.display = 'flex';
    }
    
    function hideConfirmModal() {
        confirmModal.style.display = 'none';
        currentAgendamentoToRemove = null;
    }
    
    async function removeAgendamento(id) {
        console.log('🔄 Iniciando remoção do agendamento ID:', id);
        try {
            const response = await fetch(`/api/admin/agendamentos/${id}`, {
                method: 'DELETE',
                headers: getAuthHeaders()
            });
            
            if (handleAuthError(response)) return;
            
            if (response.ok) {
                const result = await response.json();
                console.log('✅ Agendamento removido:', result);
                showFeedback('✅ Sucesso', result.message, 'success');
                
                // Recarrega os dados
                console.log('🔄 Recarregando dados...');
                await loadAllData();
            } else {
                const error = await response.json();
                showFeedback('❌ Erro', error.message || 'Não foi possível remover o agendamento', 'error');
            }
        } catch (error) {
            console.error('❌ Erro ao remover agendamento:', error);
            showFeedback('Erro de Conexão', 'Não foi possível se comunicar com o servidor', 'error');
        }
    }
    
    function showFeedback(title, message, type) {
        feedbackTitle.textContent = title;
        feedbackMessage.textContent = message;
        
        const content = document.getElementById('feedback-content');
        content.className = 'modal-content';
        
        if (type === 'success') {
            content.classList.add('modal-success');
        } else if (type === 'error') {
            content.classList.add('modal-error');
        }
        
        feedbackModal.style.display = 'flex';
    }
    
    function hideFeedbackModal() {
        feedbackModal.style.display = 'none';
    }
    
    // Event Listeners
    refreshBtn.addEventListener('click', loadAllData);
    
    logoutBtn.addEventListener('click', function() {
        localStorage.removeItem('admin_token');
        redirectToLogin();
    });
    
    cancelRemove.addEventListener('click', hideConfirmModal);
    
    confirmRemove.addEventListener('click', async function() {
        console.log('🎯 Botão confirmar clicado. currentAgendamentoToRemove:', currentAgendamentoToRemove);
        if (currentAgendamentoToRemove && currentAgendamentoToRemove.id) {
            const agendamentoId = currentAgendamentoToRemove.id;
            hideConfirmModal();
            await removeAgendamento(agendamentoId);
        } else {
            console.error('❌ currentAgendamentoToRemove é null ou não tem ID');
            hideConfirmModal();
        }
    });
    
    feedbackClose.addEventListener('click', hideFeedbackModal);
    
    // Fechar modais clicando no overlay
    confirmModal.addEventListener('click', function(e) {
        if (e.target === confirmModal) {
            hideConfirmModal();
        }
    });
    
    feedbackModal.addEventListener('click', function(e) {
        if (e.target === feedbackModal) {
            hideFeedbackModal();
        }
    });
    
    // Auto-refresh a cada 30 segundos
    setInterval(loadEstatisticas, 30000);
});