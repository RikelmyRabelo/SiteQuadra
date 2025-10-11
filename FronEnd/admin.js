document.addEventListener('DOMContentLoaded', function() {

    const form = document.getElementById('form-agendamento');
    const dataInput = document.getElementById('data');
    const horaInput = document.getElementById('hora');
    const infoHorarioFim = document.getElementById('info-horario-fim');
    const statusMensagem = document.getElementById('mensagem-status');

    // Funcionalidade de clique nos labels para abrir os seletores
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

    // Define a data mínima para hoje
    const hoje = new Date().toISOString().split('T')[0];
    dataInput.setAttribute('min', hoje);

    form.addEventListener('submit', function(event) {
        event.preventDefault();

        const nome = document.getElementById('nome').value;
        const data = dataInput.value;
        const hora = horaInput.value;
        
        // --- VALIDAÇÕES DE SEGURANÇA (JavaScript) ---

        // PONTO 1: Validação do Ano
        const ano = new Date(data).getFullYear();
        if (ano > 9999) { // Verifica se o ano tem mais de 4 dígitos
            showModal('Erro de Validação', 'O ano não pode ter mais que 4 dígitos.', 'error');
            return; // Impede o envio
        }

        // PONTO 2: Validação da Hora
        const horaSelecionada = parseInt(hora.split(':')[0]);
        if (horaSelecionada < 8 || horaSelecionada > 21) {
            showModal('Erro de Validação', 'O horário de início deve ser entre 08:00 e 21:00.', 'error');
            return; // Impede o envio
        }
        // --- FIM DAS VALIDAÇÕES ---

        const dataHoraInicio = `${data}T${hora}`;

        const agendamento = {
            id: 0,
            nomeResponsavel: nome,
            dataHoraInicio: dataHoraInicio,
            dataHoraFim: dataHoraInicio 
        };
        
        // (O restante do seu código para showModal e fetch permanece o mesmo)
        // Se você já implementou a lógica de pop-ups, ela será chamada aqui.
        // Se não, a lógica antiga de mensagem de texto será usada.
        // Por segurança, vou colocar a lógica de pop-ups aqui.
        
        showModal('Processando...', '', 'loading');

        fetch('http://localhost:5201/api/agendamentos', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(agendamento),
        })
        .then(async response => { // Adicionado 'async' para ler o corpo da resposta de erro
            if (response.ok) {
                showModal('Sucesso!', 'Agendamento salvo com sucesso.', 'success');
                form.reset();
                infoHorarioFim.textContent = '';
            } else if (response.status === 409) {
                const errorMessage = await response.text();
                showModal('Horário Ocupado', errorMessage, 'error');
            } else {
                showModal('Erro', 'Ocorreu uma falha ao salvar. Verifique os dados inseridos.', 'error');
            }
        })
        .catch(error => {
            console.error('Erro de conexão:', error);
            showModal('Erro de Conexão', 'Não foi possível se comunicar com o servidor.', 'error');
        });
    });

    // Funções do Modal (Pop-up)
    const modalOverlay = document.getElementById('modal-overlay');
    const modalContent = document.getElementById('modal-content');
    const modalTitle = document.getElementById('modal-title');
    const modalMessage = document.getElementById('modal-message');
    const modalCloseBtn = document.getElementById('modal-close-btn');

    function showModal(title, message, type = 'info') {
        modalTitle.textContent = title;
        modalMessage.textContent = message;
        
        modalContent.className = 'modal-content'; 
        if (type === 'loading') {
            modalContent.classList.add('modal-loading');
        } else if (type === 'success') {
            modalContent.classList.add('modal-success');
        } else if (type === 'error') {
            modalContent.classList.add('modal-error');
        }

        modalOverlay.style.display = 'flex';
    }

    function hideModal() {
        modalOverlay.style.display = 'none';
    }

    modalCloseBtn.addEventListener('click', hideModal);
    modalOverlay.addEventListener('click', function(e) {
        if (e.target === modalOverlay) {
            hideModal();
        }
    });
});