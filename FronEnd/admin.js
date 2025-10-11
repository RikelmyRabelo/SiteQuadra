document.addEventListener('DOMContentLoaded', function() {

    const form = document.getElementById('form-agendamento');
    const dataInput = document.getElementById('data');
    const horaInput = document.getElementById('hora');
    const infoHorarioFim = document.getElementById('info-horario-fim');
    const statusMensagem = document.getElementById('mensagem-status');


    // Adiciona um listener no 'pai' do input de data
    const dateGroup = dataInput.parentElement;
    dateGroup.addEventListener('click', function(e) {
        // Evita que o clique no input dispare o evento duas vezes
        if (e.target !== dataInput) {
            dataInput.showPicker();
        }
    });

    // Adiciona um evento de clique para o grupo da hora
    const timeGroup = horaInput.parentElement;
    timeGroup.addEventListener('click', function(e) {
        // Evita que o clique no input dispare o evento duas vezes
        if (e.target !== horaInput) {
            horaInput.showPicker();
        }
    });
    
    // Função para atualizar o horário de término
    function atualizarInfoHorario() {
        const horaValue = horaInput.value;
        if (horaValue) {
            const [hora, minuto] = horaValue.split(':');
            const dataInicio = new Date();
            dataInicio.setHours(parseInt(hora), parseInt(minuto));
            
            dataInicio.setHours(dataInicio.getHours() + 1);

            const horaFimFormatada = dataInicio.getHours().toString().padStart(2, '0') + ':' + dataInicio.getMinutes().toString().padStart(2, '0');
            infoHorarioFim.textContent = `O jogo poderá ser jogado até as ${horaFimFormatada}.`;
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