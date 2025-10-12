document.addEventListener('DOMContentLoaded', function() {
    // Seus comentários e código existente para obter os elementos...
    const form = document.getElementById('form-agendamento');
    const dataInput = document.getElementById('data');
    const horaInput = document.getElementById('hora');
    const infoHorarioFim = document.getElementById('info-horario-fim');
    
    // Elementos do Modal
    const modalOverlay = document.getElementById('modal-overlay');
    const modalContent = document.getElementById('modal-content');
    const modalTitle = document.getElementById('modal-title');
    const modalMessage = document.getElementById('modal-message');
    const modalCloseBtn = document.getElementById('modal-close-btn');

    let shouldResetOnClose = false;

    // Funções para controlar o Modal
    function showModal(title, message, type) {
        modalTitle.textContent = title;
        modalMessage.textContent = message;
        
        modalContent.className = 'modal-content';
        modalCloseBtn.className = '';

        if (type === 'loading') {
            modalContent.classList.add('modal-loading');
        } else if (type === 'success') {
            modalContent.classList.add('modal-success');
            modalCloseBtn.classList.add('success');
            modalCloseBtn.textContent = 'Fechar';
            shouldResetOnClose = true;
        } else if (type === 'error') {
            modalContent.classList.add('modal-error');
            modalCloseBtn.textContent = 'Tentar Novamente';
            shouldResetOnClose = false;
        }
        
        modalOverlay.style.display = 'flex';
    }

    function hideModal() {
        if (shouldResetOnClose) {
            form.reset();
            infoHorarioFim.textContent = '';
            shouldResetOnClose = false;
        }
        modalOverlay.style.display = 'none';
    }

    modalCloseBtn.addEventListener('click', hideModal);
    modalOverlay.addEventListener('click', (e) => {
        if (e.target === modalOverlay) {
            hideModal();
        }
    });


    // Define a data mínima para hoje
    const hoje = new Date();
    // Ajuste para o fuso horário local para evitar erros de um dia para o outro
    hoje.setMinutes(hoje.getMinutes() - hoje.getTimezoneOffset());
    const dataMinima = hoje.toISOString().split('T')[0];
    dataInput.setAttribute('min', dataMinima);

    dataInput.parentElement.addEventListener('click', (e) => { if (e.target !== dataInput) dataInput.showPicker(); });
    horaInput.parentElement.addEventListener('click', (e) => { if (e.target !== horaInput) horaInput.showPicker(); });

    function atualizarInfoHorario() {
        const horaValue = horaInput.value;
        if (horaValue) {
            const [hora, minuto] = horaValue.split(':');
            const dataInicio = new Date();
            dataInicio.setHours(parseInt(hora), parseInt(minuto));
            dataInicio.setHours(dataInicio.getHours() + 1);
            const horaFimFormatada = dataInicio.getHours().toString().padStart(2, '0') + ':' + dataInicio.getMinutes().toString().padStart(2, '0');
            infoHorarioFim.textContent = `O jogo poderá ser jogado até as ${horaFimFormatada}.`;
        } else { infoHorarioFim.textContent = ''; }
    }
    horaInput.addEventListener('change', atualizarInfoHorario);


    // --- LÓGICA DE ENVIO DO FORMULÁRIO ATUALIZADA ---
    form.addEventListener('submit', async function(event) {
        event.preventDefault();

        const dataSelecionada = dataInput.value;
        const horaSelecionada = horaInput.value;

        // --- VALIDAÇÃO DA DATA NO PASSADO ---
        if (dataSelecionada < dataMinima) {
            showModal('Data Inválida', 'Não é possível agendar um horário em uma data passada.', 'error');
            return; // Interrompe a execução aqui
        }

        // Validação de campos vazios (já que novalidate foi adicionado)
        if (!document.getElementById('nome').value || !dataSelecionada || !horaSelecionada) {
            // A validação padrão do navegador para campos 'required' ainda funciona bem aqui.
            // Se quisermos um pop-up para isso, podemos adicionar. Por agora, mantemos o padrão.
            form.reportValidity();
            return;
        }

        showModal('', '', 'loading');

        const agendamento = {
            id: 0,
            nomeResponsavel: document.getElementById('nome').value,
            dataHoraInicio: `${dataSelecionada}T${horaSelecionada}`,
            dataHoraFim: `${dataSelecionada}T${horaSelecionada}`
        };

        try {
            const response = await fetch('http://localhost:5201/api/agendamentos', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(agendamento),
            });

            setTimeout(async () => {
                if (response.ok) {
                    showModal('Sucesso!', 'O horário foi agendado com sucesso.', 'success');
                } else if (response.status === 409) {
                    const errorMessage = await response.text();
                    showModal('Erro de Agendamento', errorMessage, 'error');
                } else {
                    showModal('Erro Inesperado', 'Ocorreu uma falha ao salvar.', 'error');
                }
            }, 100);

        } catch (error) {
            setTimeout(() => {
                showModal('Erro de Conexão', 'Não foi possível se comunicar com o servidor.', 'error');
            }, 100);
        }
    });
});