document.addEventListener('DOMContentLoaded', function() {
    
    var calendarEl = document.getElementById('calendario');

    var calendar = new FullCalendar.Calendar(calendarEl, {
        initialView: 'dayGridMonth',
        locale: 'pt-br',
        headerToolbar: {
            left: 'prev,next today',
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
            }
        },

        viewDidMount: function(info) {
            const button = document.querySelector('.fc-changeViewButton-button');
            if (info.view.type === 'dayGridMonth') {
                button.textContent = 'dia';
            } else {
                button.textContent = 'mês';
            }
        },
        
        buttonText: {
            today: 'hoje',
        },
        
        allDaySlot: false, // Remove a linha "dia todo" da visualização de dia

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

        events: 'http://localhost:5201/api/agendamentos',
  
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
        }
    });

    calendar.render();
});