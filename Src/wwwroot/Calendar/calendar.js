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
                    const hoje = new Date();
                    const hojeSemHoras = new Date(hoje.getFullYear(), hoje.getMonth(), hoje.getDate());
                    const ultimoDiaSemana = new Date(hojeSemHoras);
                    ultimoDiaSemana.setDate(hojeSemHoras.getDate() + (6 - hojeSemHoras.getDay()));
                    
                    const dataView = calendar.view.currentStart;
                    const dataAtualDaViewSemHoras = new Date(dataView.getFullYear(), dataView.getMonth(), dataView.getDate());

                    if (calendar.view.type === 'dayGridMonth') {
                        showTermosModal('../Booking/booking.html');
                    } else if (calendar.view.type === 'timeGridDay') {
                        if (dataAtualDaViewSemHoras >= hojeSemHoras && dataAtualDaViewSemHoras <= ultimoDiaSemana) {
                            const dataFormatada = dataAtualDaViewSemHoras.toISOString().split('T')[0];
                            showTermosModal(`../Booking/booking.html?data=${dataFormatada}`);
                        } else {
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
                    
                    const hoje = new Date();
                    const hojeSemHoras = new Date(hoje.getFullYear(), hoje.getMonth(), hoje.getDate());
                    const ultimoDiaSemana = new Date(hojeSemHoras);
                    ultimoDiaSemana.setDate(hojeSemHoras.getDate() + (6 - hojeSemHoras.getDay()));
                    const dataView = info.view.currentStart;
                    const dataAtualDaViewSemHoras = new Date(dataView.getFullYear(), dataView.getMonth(), dataView.getDate());

                    if (dataAtualDaViewSemHoras >= hojeSemHoras && dataAtualDaViewSemHoras <= ultimoDiaSemana) {
                        agendarButton.classList.remove('fc-button-disabled');
                    } else {
                        agendarButton.classList.add('fc-button-disabled');
                    }
                }
            }
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
        events: '/api/agendamentos',
  
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
            calendar.changeView('timeGridDay', info.dateStr);
        },

        eventClick: function(info) {
            calendar.changeView('timeGridDay', info.event.start);
        }
    });

    calendar.render();
});

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
});
