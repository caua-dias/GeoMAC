// Login page logic
let isLogin = true;

function toggleForm() {
  isLogin = !isLogin;
  const loginForm = document.getElementById('loginForm');
  const registerForm = document.getElementById('registerForm');
  const title = document.getElementById('formTitle');
  const subtitle = document.getElementById('formSubtitle');
  const switchText = document.getElementById('switchText');
  const switchBtn = document.getElementById('switchBtn');

  if (isLogin) {
    loginForm.style.display = 'flex';
    registerForm.style.display = 'none';
    title.textContent = 'Bem-vindo de volta';
    subtitle.textContent = 'Faça login para acessar o painel de análises';
    switchText.textContent = 'Não tem uma conta?';
    switchBtn.textContent = 'Criar conta';
  } else {
    loginForm.style.display = 'none';
    registerForm.style.display = 'flex';
    title.textContent = 'Crie sua conta';
    subtitle.textContent = 'Registre-se para começar a usar o GeoMAC';
    switchText.textContent = 'Já possui uma conta?';
    switchBtn.textContent = 'Fazer login';
  }
  // Clear errors
  document.getElementById('loginError').style.display = 'none';
  document.getElementById('registerError').style.display = 'none';
  document.getElementById('registerSuccess').style.display = 'none';
}

function togglePassword(inputId, btn) {
  const input = document.getElementById(inputId);
  const icon = btn.querySelector('i');
  if (input.type === 'password') {
    input.type = 'text';
    icon.classList.replace('fa-eye', 'fa-eye-slash');
  } else {
    input.type = 'password';
    icon.classList.replace('fa-eye-slash', 'fa-eye');
  }
}

function showError(elementId, message) {
  const el = document.getElementById(elementId);
  el.textContent = message;
  el.style.display = 'block';
}

function hideError(elementId) {
  document.getElementById(elementId).style.display = 'none';
}

// Login
document.getElementById('loginForm').addEventListener('submit', async (e) => {
  e.preventDefault();
  hideError('loginError');
  const btn = document.getElementById('loginBtn');
  btn.classList.add('loading');

  const email = document.getElementById('loginEmail').value;
  const password = document.getElementById('loginPassword').value;

  try {
    const res = await fetch('/api/auth/login', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email, password })
    });

    if (res.ok) {
      const data = await res.json();
      localStorage.setItem('geomac_token', data.token);
      localStorage.setItem('geomac_user', email);
      window.location.href = '/dashboard.html';
    } else {
      showError('loginError', 'E-mail ou senha incorretos. Verifique suas credenciais.');
    }
  } catch (err) {
    showError('loginError', 'Erro de conexão. Tente novamente.');
  } finally {
    btn.classList.remove('loading');
  }
});

// Register
document.getElementById('registerForm').addEventListener('submit', async (e) => {
  e.preventDefault();
  hideError('registerError');
  document.getElementById('registerSuccess').style.display = 'none';
  const btn = document.getElementById('registerBtn');
  btn.classList.add('loading');

  const email = document.getElementById('regEmail').value;
  const password = document.getElementById('regPassword').value;
  const confirm = document.getElementById('regPasswordConfirm').value;

  if (password !== confirm) {
    showError('registerError', 'As senhas não coincidem.');
    btn.classList.remove('loading');
    return;
  }

  if (password.length < 6) {
    showError('registerError', 'A senha deve ter pelo menos 6 caracteres.');
    btn.classList.remove('loading');
    return;
  }

  try {
    const res = await fetch('/api/auth/register', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email, password })
    });

    if (res.ok) {
      const el = document.getElementById('registerSuccess');
      el.textContent = 'Conta criada com sucesso! Redirecionando para login...';
      el.style.display = 'block';
      setTimeout(() => toggleForm(), 2000);
    } else {
      const data = await res.json().catch(() => null);
      showError('registerError', data?.message || 'Erro ao criar conta. A senha deve conter maiúsculas, minúsculas, número e caractere especial.');
    }
  } catch (err) {
    showError('registerError', 'Erro de conexão. Tente novamente.');
  } finally {
    btn.classList.remove('loading');
  }
});
