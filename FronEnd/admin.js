document.addEventListener('DOMContentLoaded', function() {

    const form = document.getElementById('form-agendamento');
    const dataInput = document.getElementById('data');
    const horaInput = document.getElementById('hora');
    const infoHorarioFim = document.getElementById('info-horario-fim');
    const statusMensagem = document.getElementById('mensagem-status');

    // --- CORREÇÃO: Restaurando o clique nos labels ---
    // Pega os elementos 'pai' dos inputs de data e hora
    const dateGroup = dataInput.parentElement;
    const timeGroup = horaInput.parentElement;

    // Adiciona um evento de clique para o grupo da data
    dateGroup.addEventListener('click', function() {
        dataInput.showPicker(); // Força a abertura do seletor de data
    });

    // Adiciona um evento de clique para o grupo da hora
    timeGroup.addEventListener('click', function() {
        horaInput.showPicker(); // Força a abertura do seletor de hora
    });
    
    // Função para atualizar o horário de término
    function atualizarInfoHorario() {
        const horaValue = horaInput.value;
        if (horaValue) {
            const hora = parseInt(horaValue.split(':')[0]);
            const horaFim = (hora + 1) % 24;
            const horaFimFormatada = horaFim.toString().padStart(2, '0') + ':00';
            infoHorarioFim.textContent = `O jogo poderá ser jogado até as ${horaFimFormatada}.`;
        } else {
            infoHorarioFim.textContent = '';
        }
    }

    // Chama a função no início para mostrar o horário do valor padrão "08:00"
    atualizarInfoHorario(); 
    
    horaInput.addEventListener('change', atualizarInfoHorario);

    form.addEventListener('submit', function(event) {
        event.preventDefault();

        const nome = document.getElementById('nome').value;
        const data = dataInput.value;
        const hora = horaInput.value;

        // Validação da Hora (não pode ser maior que 23)
        const horaSelecionada = parseInt(hora.split(':')[0]);
        if (horaSelecionada > 23) {
            statusMensagem.textContent = 'A hora de início não pode ser maior que 23.';
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
                horaInput.value = '08:00';
                atualizarInfoHorario();
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