// Função utilitária para calcular período permitido para agendamentos

// Função utilitária para calcular período permitido para agendamentos
function calcularPeriodoPermitido() {
    const hoje = new Date();
    const diaDaSemana = hoje.getDay(); // 0=Domingo, 6=Sábado
    
    console.log('🔍 DEBUG:');
    console.log('  - Data atual:', hoje.toLocaleDateString('pt-BR'));
    console.log('  - Dia da semana:', diaDaSemana, '(0=Dom, 6=Sáb)');
    
    // Data de hoje sem horário
    const hojeSemHoras = new Date(hoje.getFullYear(), hoje.getMonth(), hoje.getDate());
    
    let ultimoDiaPermitido;
    
    if (diaDaSemana === 6) {
        // REGRA ESPECIAL PARA SÁBADO: Permite agendar até o próximo sábado
        ultimoDiaPermitido = new Date(hojeSemHoras);
        ultimoDiaPermitido.setDate(hojeSemHoras.getDate() + 7);
        console.log('🎯 É SÁBADO! Liberando até próximo sábado:', ultimoDiaPermitido.toLocaleDateString('pt-BR'));
    } else {
        // Regra normal: até o sábado desta semana
        ultimoDiaPermitido = new Date(hojeSemHoras);
        const diasRestantesNaSemana = 6 - diaDaSemana;
        ultimoDiaPermitido.setDate(hojeSemHoras.getDate() + diasRestantesNaSemana);
        console.log('📅 Dia normal. Permitindo até sábado desta semana:', ultimoDiaPermitido.toLocaleDateString('pt-BR'));
    }
    
    console.log('  - Período: de', hojeSemHoras.toLocaleDateString('pt-BR'), 'até', ultimoDiaPermitido.toLocaleDateString('pt-BR'));
    
    return { hojeSemHoras, ultimoDiaPermitido };
}

// Função para verificar e mostrar modal de sábado
function verificarESexibirModalSabado() {
    const hoje = new Date();
    const diaDaSemana = hoje.getDay(); // 0=Domingo, 6=Sábado
    
    if (diaDaSemana === 6) { // Se hoje é sábado
        // Espera um pouco para o DOM carregar completamente
        setTimeout(() => {
            const sabadoModal = document.getElementById('sabado-overlay');
            if (sabadoModal) {
                sabadoModal.style.display = 'flex';
            }
        }, 1500); // 1.5 segundos após carregar a página
    }
}

// Funções do modal de sábado
function fecharModalSabado() {
    const sabadoModal = document.getElementById('sabado-overlay');
    if (sabadoModal) {
        sabadoModal.style.display = 'none';
    }
}

document.addEventListener('DOMContentLoaded', function() {
    
    var calendarEl = document.getElementById('calendario');

    var calendar = new FullCalendar.Calendar(calendarEl, {
        initialView: 'dayGridMonth',
        locale: 'pt-br',
        headerToolbar: {
            left: 'prev,next today agendarButton',
            center: 'title',
            right: 'changeViewButton'
        },
        
        customButtons: {
            changeViewButton: {
                click: function() {
                    if (calendar.view.type === 'dayGridMonth') {
                        calendar.changeView('timeGridDay');
                    } else {
                        calendar.changeView('dayGridMonth');
                    }
                }
            },
            agendarButton: {
                click: function() {
                    const { hojeSemHoras, ultimoDiaPermitido } = calcularPeriodoPermitido();
                    
                    const dataView = calendar.view.currentStart;
                    const dataAtualDaViewSemHoras = new Date(dataView.getFullYear(), dataView.getMonth(), dataView.getDate());

                    if (calendar.view.type === 'dayGridMonth') {
                        showTermosModal('../Booking/booking.html');
                    } else if (calendar.view.type === 'timeGridDay') {
                        console.log('🔍 TENTATIVA DE AGENDAMENTO:');
                        console.log('  - Data da view:', dataAtualDaViewSemHoras.toDateString());
                        console.log('  - Hoje:', hojeSemHoras.toDateString());
                        console.log('  - Último dia permitido:', ultimoDiaPermitido.toDateString());
                        console.log('  - Está dentro do período?', dataAtualDaViewSemHoras >= hojeSemHoras && dataAtualDaViewSemHoras <= ultimoDiaPermitido);
                        
                        if (dataAtualDaViewSemHoras >= hojeSemHoras && dataAtualDaViewSemHoras <= ultimoDiaPermitido) {
                            const dataFormatada = dataAtualDaViewSemHoras.toISOString().split('T')[0];
                            console.log('✅ PERMITIDO - Redirecionando para agendamento');
                            showTermosModal(`../Booking/booking.html?data=${dataFormatada}`);
                        } else {
                            console.log('❌ BLOQUEADO - Mostrando modal de erro');
                            showModal();
                        }
                    }
                }
            }
        },

        datesSet: function(info) {
            const changeViewButton = document.querySelector('.fc-changeViewButton-button');
            if (changeViewButton) {
                changeViewButton.textContent = info.view.type === 'dayGridMonth' ? 'dia' : 'mês';
            }

            const agendarButton = document.querySelector('.fc-agendarButton-button');
            if (agendarButton) {
                agendarButton.style.display = 'inline-block';
                
                if (info.view.type === 'dayGridMonth') {
                    agendarButton.textContent = 'Novo Agendamento';
                    agendarButton.classList.remove('fc-button-disabled');
                } else if (info.view.type === 'timeGridDay') {
                    agendarButton.textContent = 'Agendar neste dia';
                    
                    const { hojeSemHoras, ultimoDiaPermitido } = calcularPeriodoPermitido();
                    
                    const dataView = info.view.currentStart;
                    const dataAtualDaViewSemHoras = new Date(dataView.getFullYear(), dataView.getMonth(), dataView.getDate());

                    console.log('🔍 ATUALIZAÇÃO DO BOTÃO:');
                    console.log('  - Data da view:', dataAtualDaViewSemHoras.toDateString());
                    console.log('  - Hoje:', hojeSemHoras.toDateString());
                    console.log('  - Último dia permitido:', ultimoDiaPermitido.toDateString());
                    console.log('  - Botão deve estar habilitado?', dataAtualDaViewSemHoras >= hojeSemHoras && dataAtualDaViewSemHoras <= ultimoDiaPermitido);
                    
                    if (dataAtualDaViewSemHoras >= hojeSemHoras && dataAtualDaViewSemHoras <= ultimoDiaPermitido) {
                        agendarButton.classList.remove('fc-button-disabled');
                        console.log('✅ BOTÃO HABILITADO');
                    } else {
                        agendarButton.classList.add('fc-button-disabled');
                        console.log('❌ BOTÃO DESABILITADO');
                    }
                }
            }
            
            // Aplica correção mobile após mudança de view
            applyMobileFix();
        },
        
        buttonText: {
            today: 'hoje',
        },
        
        allDaySlot: false,
        slotMinTime: '08:00:00',
        slotMaxTime: '22:00:00',
        height: 'auto',
        contentHeight: 'auto',

        slotLabelFormat: {
            hour: '2-digit',
            minute: '2-digit',
            hour12: false,
            template: function(info) {
                return `${info.text} horas`;
            }
        },

        nowIndicator: true,
        displayEventTime: true,
        events: window.location.origin.includes('localhost') || window.location.origin.includes('127.0.0.1') ? 
            'http://localhost:5201/api/agendamentos' : '/api/agendamentos',
  
        eventDataTransform: function(eventData) {
            return {
                id: eventData.id,
                title: eventData.nomeResponsavel,
                start: eventData.dataHoraInicio,
                end: eventData.dataHoraFim,
                color: eventData.cor || '#3788d8'
            };
        },

        dateClick: function(info) {
            console.log('💆 CLIQUE NO DIA:', info.dateStr);
            const { hojeSemHoras, ultimoDiaPermitido } = calcularPeriodoPermitido();
            const dataClicada = new Date(info.dateStr + 'T00:00:00');
            console.log('  - Data clicada:', dataClicada.toDateString());
            console.log('  - Permitido agendar?', dataClicada >= hojeSemHoras && dataClicada <= ultimoDiaPermitido);
            calendar.changeView('timeGridDay', info.dateStr);
        },

        eventClick: function(info) {
            calendar.changeView('timeGridDay', info.event.start);
        },
        
    });

    calendar.render();
    
    // Fix mobile layout after render
    fixMobileLayout();
    
    // Verificar se é sábado e mostrar modal
    verificarESexibirModalSabado();
    
    // Aplica bordas para mobile - ESTRATÉGIA ANTI-CACHE
    if (window.innerWidth <= 480) {
        applySimpleBorders(); // Imediatamente
        setTimeout(applySimpleBorders, 50);
        setTimeout(applySimpleBorders, 100);
        setTimeout(applySimpleBorders, 300);
        setTimeout(applySimpleBorders, 500);
        setTimeout(applySimpleBorders, 800);
        setTimeout(applySimpleBorders, 1000);
        setTimeout(applySimpleBorders, 1500);
        setTimeout(applySimpleBorders, 2000);
        
        // Aplica novamente a cada 3 segundos por 15 segundos
        let attempts = 0;
        const interval = setInterval(() => {
            applySimpleBorders();
            attempts++;
            if (attempts >= 5) clearInterval(interval);
        }, 3000);
    }
});

// Função para corrigir layout mobile
function fixMobileLayout() {
    // Só aplicar em telas pequenas
    if (window.innerWidth <= 480) {
        setTimeout(() => {
            const toolbar = document.querySelector('.fc-toolbar');
            if (toolbar) {
                console.log('Corrigindo layout mobile...');
                
                // Força estilo da toolbar
                toolbar.style.cssText = `
                    display: flex !important;
                    flex-direction: column !important;
                    align-items: center !important;
                    gap: 8px !important;
                    margin-bottom: 15px !important;
                    padding: 8px !important;
                    background: rgba(255,255,255,0.95) !important;
                    border-radius: 8px !important;
                    box-shadow: 0 2px 4px rgba(0,0,0,0.1) !important;
                `;
                
                // Reorganiza os chunks
                const chunks = toolbar.querySelectorAll('.fc-toolbar-chunk');
                if (chunks.length >= 2) {
                    // Título primeiro (chunk do meio)
                    if (chunks[1]) {
                        chunks[1].style.order = '1';
                        chunks[1].style.margin = '0';
                    }
                    
                    // Navegação segundo (chunk da esquerda)
                    if (chunks[0]) {
                        chunks[0].style.order = '2';
                        chunks[0].style.display = 'flex';
                        chunks[0].style.gap = '4px';
                        chunks[0].style.margin = '4px 0';
                    }
                    
                    // Botões extras terceiro (chunk da direita)
                    if (chunks[2]) {
                        chunks[2].style.order = '3';
                        chunks[2].style.display = 'flex';
                        chunks[2].style.gap = '4px';
                        chunks[2].style.margin = '4px 0';
                    }
                }
                
                // Ajusta todos os botões
                const buttons = toolbar.querySelectorAll('.fc-button');
                buttons.forEach(button => {
                    button.style.cssText += `
                        padding: 4px 8px !important;
                        font-size: 10px !important;
                        margin: 1px !important;
                        min-width: 45px !important;
                        height: 28px !important;
                        line-height: 1.2 !important;
                        white-space: nowrap !important;
                        border-radius: 4px !important;
                    `;
                });
                
                // Ajusta título
                const title = toolbar.querySelector('.fc-toolbar-title');
                if (title) {
                    title.style.cssText = `
                        font-size: 1rem !important;
                        margin: 0 !important;
                        padding: 5px 10px !important;
                        text-align: center !important;
                        color: #2c3e50 !important;
                        font-weight: 600 !important;
                    `;
                }
                
                // Aplica bordas simples
                setTimeout(applySimpleBorders, 100);
            }
        }, 100);
    }
}

// Função para corrigir bordas do calendário
function fixCalendarBorders() {
    setTimeout(() => {
        console.log('Aplicando bordas uniformes...');
        
        // ESTRATÉGIA: Criar um container com borda única em volta de tudo
        const calendarTable = document.querySelector('.fc-daygrid');
        if (calendarTable && window.innerWidth <= 480) {
            // Remove todas as bordas existentes primeiro
            const allCells = document.querySelectorAll('.fc-daygrid-day, .fc-col-header-cell');
            allCells.forEach(cell => {
                cell.style.border = 'none !important';
                cell.style.borderRight = '1px solid #ddd !important';
                cell.style.borderBottom = '1px solid #ddd !important';
            });
            
            // Remove borda direita das últimas colunas
            const lastColCells = document.querySelectorAll('.fc-col-header-cell:nth-child(7n), .fc-daygrid-day:nth-child(7n)');
            lastColCells.forEach(cell => {
                cell.style.borderRight = 'none !important';
            });
            
            // Aplica borda vermelha externa no container principal
            const dayGrid = document.querySelector('.fc-daygrid');
            if (dayGrid) {
                dayGrid.style.cssText = `
                    border: 2px solid #dc3545 !important;
                    border-radius: 8px !important;
                    overflow: hidden !important;
                    box-shadow: 0 2px 4px rgba(220, 53, 69, 0.2) !important;
                `;
            }
            
            // Garante que o container da view também tenha a borda
            const viewHarness = document.querySelector('.fc-view-harness');
            if (viewHarness) {
                viewHarness.style.cssText += `
                    border: 2px solid #dc3545 !important;
                    border-radius: 8px !important;
                    overflow: hidden !important;
                    background: white !important;
                `;
            }
            
            // Cabeçalhos com fundo diferenciado
            const headerCells = document.querySelectorAll('.fc-col-header-cell');
            headerCells.forEach(cell => {
                cell.style.cssText += `
                    background: #f8f9fa !important;
                    font-weight: bold !important;
                    border-bottom: 2px solid #ddd !important;
                `;
            });
            
            console.log('Bordas aplicadas!');
        }
    }, 150);
}

// Reaplica correções quando a view muda
function applyMobileFix() {
    if (window.innerWidth <= 480) {
        fixMobileLayout();
        setTimeout(applySimpleBorders, 100);
    }
}

// Listener para redimensionamento da janela
window.addEventListener('resize', () => {
    applyMobileFix();
});

// Função ULTRA-AGRESSIVA para aplicar bordas (força em produção)
function applySimpleBorders() {
    if (window.innerWidth > 480) return;
    
    const calendario = document.getElementById('calendario');
    if (calendario) {
        // ESTRATÉGIA 1: Remove todos os estilos existentes e reaplica
        calendario.removeAttribute('style');
        
        // ESTRATÉGIA 2: Cria um novo elemento de estilo inline com máxima prioridade
        const forceStyle = `
            border: 3px solid #dc3545 !important;
            border-radius: 10px !important;
            overflow: hidden !important;
            background: white !important;
            box-sizing: border-box !important;
            width: calc(100% - 10px) !important;
            margin: 5px auto !important;
            padding: 0 !important;
            display: block !important;
        `;
        calendario.setAttribute('style', forceStyle);
        
        // ESTRATÉGIA 3: Cria CSS dinâmico com timestamp para burlar cache
        const timestamp = Date.now();
        const styleId = `mobile-borders-${timestamp}`;
        
        // Remove estilos anteriores
        const oldStyle = document.getElementById(styleId.replace(timestamp, ''));
        if (oldStyle) oldStyle.remove();
        
        // Cria novo estilo
        const dynamicStyle = document.createElement('style');
        dynamicStyle.id = styleId;
        dynamicStyle.textContent = `
            @media (max-width: 480px) {
                #calendario {
                    border: 3px solid #dc3545 !important;
                    border-radius: 10px !important;
                    overflow: hidden !important;
                    background: white !important;
                    box-sizing: border-box !important;
                    width: calc(100% - 10px) !important;
                    margin: 5px auto !important;
                    padding: 0 !important;
                    display: block !important;
                }
                #calendario .fc,
                #calendario .fc-view-harness {
                    width: 100% !important;
                    margin: 0 !important;
                    padding: 0 !important;
                    box-sizing: border-box !important;
                }
            }
        `;
        document.head.appendChild(dynamicStyle);
        
        // ESTRATÉGIA 4: Força propriedades nos containers internos
        setTimeout(() => {
            const fcContainer = calendario.querySelector('.fc');
            if (fcContainer) {
                fcContainer.style.cssText += 'width: 100% !important; margin: 0 !important; padding: 0 !important;';
            }
            
            const viewHarness = calendario.querySelector('.fc-view-harness');
            if (viewHarness) {
                viewHarness.style.cssText += 'width: 100% !important; margin: 0 !important; padding: 0 !important;';
            }
        }, 50);
    }
}

// Funções do modal
function showModal() {
    const modal = document.getElementById('modal-overlay');
    modal.style.display = 'flex';
}

function hideModal() {
    const modal = document.getElementById('modal-overlay');
    modal.style.display = 'none';
}

// Funções do modal de termos
function showTermosModal(redirectUrl) {
    const termosModal = document.getElementById('termos-overlay');
    termosModal.style.display = 'flex';
    
    // Armazenar URL de redirecionamento
    termosModal.setAttribute('data-redirect', redirectUrl);
}

function hideTermosModal() {
    const termosModal = document.getElementById('termos-overlay');
    termosModal.style.display = 'none';
}

function aceitarTermos() {
    const termosModal = document.getElementById('termos-overlay');
    const redirectUrl = termosModal.getAttribute('data-redirect');
    
    hideTermosModal();
    
    // Redirecionar para página de agendamento
    if (redirectUrl) {
        window.location.href = redirectUrl;
    }
}

// Event listener para fechar o modal
document.addEventListener('DOMContentLoaded', function() {
    const closeBtn = document.getElementById('modal-close-btn');
    const modal = document.getElementById('modal-overlay');
    
    if (closeBtn) {
        closeBtn.addEventListener('click', hideModal);
    }
    
    if (modal) {
        modal.addEventListener('click', function(event) {
            if (event.target === modal) {
                hideModal();
            }
        });
    }
    
    // Event listeners para modal de termos
    const termosAceitoBtn = document.getElementById('termos-aceito-btn');
    const termosCancelarBtn = document.getElementById('termos-cancelar-btn');
    const termosModal = document.getElementById('termos-overlay');
    
    if (termosAceitoBtn) {
        termosAceitoBtn.addEventListener('click', aceitarTermos);
    }
    
    if (termosCancelarBtn) {
        termosCancelarBtn.addEventListener('click', hideTermosModal);
    }
    
    if (termosModal) {
        termosModal.addEventListener('click', function(event) {
            if (event.target === termosModal) {
                hideTermosModal();
            }
        });
    }
    
    // Event listeners para modal de sábado
    const sabadoFecharBtn = document.getElementById('sabado-fechar-btn');
    const sabadoModal = document.getElementById('sabado-overlay');
    
    if (sabadoFecharBtn) {
        sabadoFecharBtn.addEventListener('click', fecharModalSabado);
    }
    
    if (sabadoModal) {
        sabadoModal.addEventListener('click', function(event) {
            if (event.target === sabadoModal) {
                fecharModalSabado();
            }
        });
    }
});
