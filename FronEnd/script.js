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
 
        events: 'http://localhost:5201/api/agendamentos',

    
        eventDataTransform: function(eventData) {
            return {
                id: eventData.id,
                title: eventData.nomeResponsavel,
                start: eventData.dataHoraInicio,
                end: eventData.dataHoraFim
            };
        }
    });

    calendar.render();
});