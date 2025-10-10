document.addEventListener('DOMContentLoaded', function() {

    const form = document.getElementById('form-agendamento');
    const dataInput = document.getElementById('data');
    const horaInput = document.getElementById('hora');
    const infoHorarioFim = document.getElementById('info-horario-fim');
    const statusMensagem = document.getElementById('mensagem-status');

    function atualizarInfoHorario() {
        const horaValue = horaInput.value;
        if (horaValue) {
            const hora = parseInt(horaValue.split(':')[0]);
            const horaFim = (hora + 1) % 24;
            const horaFimFormatada = horaFim.toString().padStart(2, '0') + ':00';
            infoHorarioFim.textContent = `Você podera ficar na quadra até às: ${horaFimFormatada}.`;
        } else {
            infoHorarioFim.textContent = '';
        }
    }

    horaInput.addEventListener('change', atualizarInfoHorario);

    form.addEventListener('submit', function(event) {
        event.preventDefault();

        const nome = document.getElementById('nome').value;
        const data = dataInput.value;
        const hora = horaInput.value;
        
        const ano = new Date(data).getFullYear();
        if (ano.toString().length > 4) {
            statusMensagem.textContent = 'O ano não pode ter mais que 4 dígitos.';
            statusMensagem.style.color = 'red'; 
            return; 
        }

        const dataHoraInicio = `${data}T${hora}`;

        const agendamento = {
            id: 0,
            nomeResponsavel: nome,
            dataHoraInicio: dataHoraInicio,
            dataHoraFim: dataHoraInicio 
        };

        statusMensagem.textContent = 'Salvando...';
        statusMensagem.style.color = 'blue';

        fetch('http://localhost:5201/api/agendamentos', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(agendamento),
        })
        .then(response => {
            if (response.ok) {
                statusMensagem.textContent = 'Agendamento salvo com sucesso!';
                statusMensagem.style.color = 'green';
                form.reset();
                infoHorarioFim.textContent = '';
            } else {
                statusMensagem.textContent = 'Falha ao salvar. Verifique o console (F12).';
                statusMensagem.style.color = 'red'; 
            }
        })
        .catch(error => {
            console.error('Erro na requisição:', error);
            statusMensagem.textContent = 'Erro de conexão com a API.';
            statusMensagem.style.color = 'red'; 
        });
    });
});