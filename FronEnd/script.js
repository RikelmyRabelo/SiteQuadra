document.addEventListener('DOMContentLoaded', function() {
    
    var calendarEl = document.getElementById('calendario');

    var calendar = new FullCalendar.Calendar(calendarEl, {
        initialView: 'dayGridMonth',
        locale: 'pt-br',
        headerToolbar: {
            left: 'prev,next today',
            center: 'title',
            right: 'dayGridMonth,timeGridWeek,timeGridDay'
        },
        
        buttonText: {
            today: 'hoje',
            month: 'mês',
            week: 'semana',
            day: 'dia'
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
        }
    });

    calendar.render();
});