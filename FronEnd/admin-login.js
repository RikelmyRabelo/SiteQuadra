document.addEventListener('DOMContentLoaded', function() {
    const form = document.getElementById('login-form');
    const passwordInput = document.getElementById('password');
    const loginBtn = document.getElementById('login-btn');
    const loginText = document.getElementById('login-text');
    const loginLoader = document.getElementById('login-loader');
    const errorMessage = document.getElementById('error-message');

    // Verifica se já está logado
    checkExistingAuth();

    function showError(message) {
        errorMessage.textContent = message;
        errorMessage.style.display = 'block';
        setTimeout(() => {
            errorMessage.style.display = 'none';
        }, 5000);
    }

    function setLoading(loading) {
        if (loading) {
            loginText.style.display = 'none';
            loginLoader.style.display = 'inline-block';
            loginBtn.disabled = true;
        } else {
            loginText.style.display = 'inline';
            loginLoader.style.display = 'none';
            loginBtn.disabled = false;
        }
    }

    function checkExistingAuth() {
        const token = localStorage.getItem('admin_token');
        if (token) {
            // Verifica se o token ainda é válido
            validateTokenAndRedirect(token);
        }
    }

    async function validateTokenAndRedirect(token) {
        try {
            const response = await fetch('http://localhost:5201/api/admin/agendamentos', {
                method: 'GET',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                }
            });

            if (response.ok) {
                // Token válido, redireciona para o dashboard
                window.location.href = 'admin-dashboard.html';
            } else {
                // Token inválido, remove do localStorage
                localStorage.removeItem('admin_token');
            }
        } catch (error) {
            console.error('Erro ao validar token:', error);
            localStorage.removeItem('admin_token');
        }
    }

    form.addEventListener('submit', async function(event) {
        event.preventDefault();
        
        const password = passwordInput.value.trim();
        
        if (!password) {
            showError('Por favor, digite a senha');
            passwordInput.focus();
            return;
        }

        setLoading(true);
        errorMessage.style.display = 'none';

        try {
            const response = await fetch('http://localhost:5201/api/admin/login', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ password: password })
            });

            const data = await response.json();

            if (response.ok) {
                // Login bem-sucedido
                localStorage.setItem('admin_token', data.token);
                
                // Pequeno delay para mostrar sucesso
                loginText.textContent = '✅ Sucesso!';
                setTimeout(() => {
                    window.location.href = 'admin-dashboard.html';
                }, 500);
                
            } else {
                // Login falhou
                showError(data.message || 'Senha incorreta');
                passwordInput.select();
                setLoading(false);
            }
        } catch (error) {
            console.error('Erro de conexão:', error);
            showError('Erro de conexão com o servidor');
            setLoading(false);
        }
    });

    // Foco automático no campo de senha
    passwordInput.focus();

    // Enter para submeter o formulário
    passwordInput.addEventListener('keypress', function(event) {
        if (event.key === 'Enter') {
            form.dispatchEvent(new Event('submit'));
        }
    });
});