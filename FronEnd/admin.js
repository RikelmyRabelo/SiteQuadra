document.addEventListener('DOMContentLoaded', function() {

    const form = document.getElementById('form-agendamento');
    const dataInput = document.getElementById('data');
    const horaInput = document.getElementById('hora');
    const infoHorarioFim = document.getElementById('info-horario-fim');
    const statusMensagem = document.getElementById('mensagem-status');

    // --- NOVO BLOCO: Impede a seleção de datas passadas ---
    const hoje = new Date();
    const ano = hoje.getFullYear();
    const mes = String(hoje.getMonth() + 1).padStart(2, '0'); // Adiciona um zero à esquerda se necessário
    const dia = String(hoje.getDate()).padStart(2, '0'); // Adiciona um zero à esquerda se necessário
    
    // Formata a data para "AAAA-MM-DD", que é o formato que o input aceita
    const dataMinima = `${ano}-${mes}-${dia}`;
    
    // Define o atributo 'min' no campo de data
    dataInput.setAttribute('min', dataMinima);
    // --- FIM DO NOVO BLOCO ---


    // Funcionalidade de clique nos labels
    dataInput.parentElement.addEventListener('click', function(e) {
        if (e.target !== dataInput) dataInput.showPicker();
    });
    horaInput.parentElement.addEventListener('click', function(e) {
        if (e.target !== horaInput) horaInput.showPicker();
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