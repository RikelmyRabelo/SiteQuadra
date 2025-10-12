document.addEventListener('DOMContentLoaded', function() {

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

    // Lógica do formulário
    const hoje = new Date().toISOString().split('T')[0];
    dataInput.setAttribute('min', hoje);

    dataInput.parentElement.addEventListener('click', (e) => { if (e.target !== dataInput) dataInput.showPicker(); });
    horaInput.parentElement.addEventListener('click', (e) => { if (e.target !== horaInput) horaInput.showPicker(); });

    function atualizarInfoHorario() {
        const horaValue = horaInput.value;
        if (horaValue) {
            const [hora, minuto] = horaValue.split(':');
            const horaInt = parseInt(hora);
            if (horaInt >= 8 && horaInt <= 21) {
                const dataInicio = new Date();
                dataInicio.setHours(horaInt, parseInt(minuto) || 0);
                dataInicio.setHours(dataInicio.getHours() + 1);
                const horaFimFormatada = dataInicio.getHours().toString().padStart(2, '0') + ':' + dataInicio.getMinutes().toString().padStart(2, '0');
                infoHorarioFim.textContent = `Você poderá permanecer na quadra até às ${horaFimFormatada}.`;
            } else {
                infoHorarioFim.textContent = '';
            }
        } else { infoHorarioFim.textContent = ''; }
    }
    horaInput.addEventListener('change', atualizarInfoHorario);

    // Lógica de Envio do Formulário
    form.addEventListener('submit', async function(event) {
        event.preventDefault();

        const horaSelecionada = horaInput.value;
        const hora = parseInt(horaSelecionada.split(':')[0]);
        if (hora < 8 || hora > 21) {
            showModal('Horário Inválido', 'O horário de funcionamento da quadra é das 08:00 às 21:00.', 'error');
            return;
        }
        
        showModal('', '', 'loading');

        // Calcula a hora de fim (1 hora depois)
        const [horaInicio, minutoInicio] = horaInput.value.split(':');
        const dataHoraInicioObj = new Date(`${dataInput.value}T${horaInput.value}`);
        const dataHoraFimObj = new Date(dataHoraInicioObj.getTime() + 60 * 60 * 1000); 
        
        const agendamento = {
            id: 0,
            nomeResponsavel: document.getElementById('nome').value,
            contato: document.getElementById('contato').value,          
            cidadeBairro: document.getElementById('cidade-bairro').value, 
            dataHoraInicio: `${dataInput.value}T${horaInput.value}`,
            dataHoraFim: dataHoraFimObj.toISOString().slice(0, 16)
        };

        try {
            const response = await fetch('http://localhost:5201/api/agendamentos', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(agendamento),
            });

            if (response.ok) {
                showModal('Sucesso!', 'O horário foi agendado com sucesso.', 'success');
            } else if (response.status === 409) {
                const errorMessage = await response.text();
                showModal('Erro de Agendamento', errorMessage, 'error');
            } else {
                showModal('Erro Inesperado', 'Ocorreu uma falha ao salvar. Por favor, tente novamente.', 'error');
            }

        } catch (error) {
            console.error('Erro de conexão:', error);
            showModal('Erro de Conexão', 'Não foi possível se comunicar com o servidor.', 'error');
        }
    });
});