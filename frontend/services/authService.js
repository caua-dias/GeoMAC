const FarmAuth = (() => {
  const USERS = 'farmlab_users', SESSION = 'farmlab_session', TOKEN = 'farmlab_jwt_token';

  // Detecta se está acessando direto pelo backend ou via Live Server / arquivo local
  const getApiBase = () => {
    if (window.location.protocol === 'file:') return 'http://localhost:5000/api';
    if (['5500', '3000', '5173', '8080'].includes(window.location.port)) {
      return 'http://localhost:5000/api';
    }
    return '/api';
  };

  const read = k => { try { return JSON.parse(localStorage.getItem(k)); } catch { return null; } };
  const write = (k, v) => { try { localStorage.setItem(k, JSON.stringify(v)); } catch {} };
  const sha = async s => [...new Uint8Array(await crypto.subtle.digest('SHA-256', new TextEncoder().encode(s)))]
    .map(b => b.toString(16).padStart(2, '0')).join('');

  async function register({ name, farm, email, password }) {
    email = email.trim().toLowerCase();
    const payload = { name: name.trim(), farm: (farm || '').trim(), email, password };
    const apiBase = getApiBase();

    try {
      const res = await fetch(`${apiBase}/auth/register`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
      });

      if (res.ok) {
        // Registro com sucesso na API C#, faz o login logo em seguida para gerar o JWT
        await login({ email, password, remember: true });
        return;
      }

      // Se a API retornou erro com status (ex: 400 ou 409)
      const data = await res.json().catch(() => ({}));
      throw new Error(data.message || data.Message || 'Erro ao realizar cadastro na API.');
    } catch (err) {
      // Se for erro retornado pelo próprio backend, propaga a mensagem
      if (err.message && !err.message.includes('Failed to fetch') && !err.message.includes('NetworkError')) {
        throw err;
      }

      // Fallback local caso o backend C# esteja desligado no momento (modo demonstração)
      console.warn('[FarmLab] API offline ou inacessível. Usando persistência local de demonstração.', err);
      const users = read(USERS) || [];
      if (users.some(u => u.email === email)) throw new Error('Este e-mail já está cadastrado. Entre ou recupere a senha.');
      users.push({ name, farm, email, hash: await sha(password) });
      write(USERS, users);
      write(SESSION, { name, farm, email });
      write(TOKEN, 'demo-jwt-token-local');
    }
  }

  async function login({ email, password, remember }) {
    email = email.trim().toLowerCase();
    const payload = { email, password };
    const apiBase = getApiBase();

    try {
      const res = await fetch(`${apiBase}/auth/login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
      });

      if (res.ok) {
        const data = await res.json();
        const jwtToken = data.token;
        const userData = data.user || {
          name: email.split('@')[0],
          farm: '',
          email: email
        };

        write(TOKEN, jwtToken);
        write(SESSION, userData);
        try {
          localStorage.setItem('geomac_token', jwtToken);
          localStorage.setItem('geomac_user', JSON.stringify(userData));
        } catch {}
        if (!remember) sessionStorage.setItem('farmlab_tmp', '1');
        return;
      }

      if (res.status === 401 || res.status === 400) {
        const data = await res.json().catch(() => ({}));
        throw new Error(data.message || data.Message || 'E-mail ou senha incorretos. Confira os dados e tente de novo.');
      }

      throw new Error(`Erro no servidor: status ${res.status}`);
    } catch (err) {
      // Se for erro de credencial da API, propaga para a tela
      if (err.message && !err.message.includes('Failed to fetch') && !err.message.includes('NetworkError')) {
        throw err;
      }

      // Fallback local caso a API C# esteja offline
      console.warn('[FarmLab] API offline. Tentando login com credenciais locais simuladas.', err);
      const u = (read(USERS) || []).find(u => u.email === email);
      if (!u || u.hash !== await sha(password)) {
        throw new Error('E-mail ou senha incorretos (ou API C# ainda não inicializada).');
      }
      const s = { name: u.name, farm: u.farm, email: u.email };
      write(SESSION, s);
      write(TOKEN, 'demo-jwt-token-local');
      if (!remember) sessionStorage.setItem('farmlab_tmp', '1');
    }
  }

  async function reset({ email }) {
    return email;
  }

  const user = () => read(SESSION);
  const token = () => {
    try { return JSON.parse(localStorage.getItem(TOKEN)); } catch { return localStorage.getItem(TOKEN); }
  };
  const getAuthHeaders = () => {
    const t = token();
    return t ? { 'Authorization': `Bearer ${t}`, 'Content-Type': 'application/json' } : { 'Content-Type': 'application/json' };
  };

  const fetchWithAuth = async (url, options = {}) => {
    options.headers = { ...getAuthHeaders(), ...(options.headers || {}) };
    return fetch(url, options);
  };

  const logout = () => {
    try {
      localStorage.removeItem(SESSION);
      localStorage.removeItem(TOKEN);
      localStorage.removeItem('geomac_token');
      localStorage.removeItem('geomac_user');
      sessionStorage.removeItem('farmlab_tmp');
    } catch {}
    location.href = 'login.html';
  };

  const requireAuth = () => {
    if (!user()) location.replace('login.html');
  };

  return { register, login, reset, user, token, getAuthHeaders, fetchWithAuth, logout, requireAuth, getApiBase };
})();