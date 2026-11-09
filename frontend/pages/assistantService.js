// ==========================================================================
// GEOMAC / FARMLAB — ASSISTANT SERVICE (assistantService.js)
// Gerenciamento de rotas de IA (Diagnóstico & Chat) com suporte a:
// 1. Dados Estáticos (Demonstração / Offline)
// 2. LM Studio em Tempo Real (POST /api/assistant/...)
// ==========================================================================

const AssistantService = (() => {
  const STORAGE_KEY = 'geomac_llm_mode'; // 'static' | 'live'

  // Detecta URL base da API (reaproveitando lógica padrão do FarmLab)
  const getApiBase = () => {
    if (typeof window !== 'undefined') {
      if (window.location.protocol === 'file:') return 'http://localhost:5235/api';
      if (['5500', '3000', '5173', '8080'].includes(window.location.port)) {
        return 'http://localhost:5235/api';
      }
    }
    return '/api';
  };

  // Obtém o modo atual (padrão inicial: 'static' para garantir apresentação imediata)
  const getMode = () => {
    try {
      return localStorage.getItem(STORAGE_KEY) || 'static';
    } catch {
      return 'static';
    }
  };

  // Define o modo atual
  const setMode = (mode) => {
    try {
      localStorage.setItem(STORAGE_KEY, mode);
    } catch {}
  };

  // Amostra real de dados do Talhão Grão 4.0 (Safra 2025/2026)
  const SAMPLE_GRAO_40 = {
    fieldName: "Grão 4.0",
    cropName: "Milho DKB358PRO4",
    peakNdvi: 0.827,
    finalNdvi: 0.507,
    minSoilPh: 4.65,
    exchangeableAluminum: 1.48,
    reproductiveStagePrecipitationMm: 0.0,
    maxTemperatureCelsius: 35.9,
    additionalNotes: "Solo arenoso (>84% areia), veranico severo de 14 dias no estádio VT/R1 e danos por capivaras nas bordaduras."
  };

  // Resposta estruturada estática realista baseada no estudo de caso
  const STATIC_DIAGNOSIS_DATA = {
    executiveSummary: "O Talhão Grão 4.0 apresentou uma quebra acentuada de produtividade provocada pela interação destrutiva entre atributos químicos restritivos do solo (pH 4.65 com 1.48 cmol de Alumínio trocável) e o veranico severo durante o florescimento e pendoamento (0.0 mm de chuva e 35.9°C). O sistema radicular confinado aos primeiros 15 cm foi incapaz de mitigar o déficit hídrico.",
    yieldLossProbability: 85.0,
    overallRiskLevel: "Crítico",
    limitingFactors: [
      {
        category: "Restrição Química do Solo",
        description: "Presença de alumínio tóxico (1.48 cmol/dm³) e saturação por bases baixa (47%), induzindo rasamento radicular severo.",
        severity: "Crítica"
      },
      {
        category: "Estresse Termo-Hídrico",
        description: "Zero precipitação acumulada nas semanas reprodutivas (VT/R1) concomitante a pico térmico de 35.9°C, ocasionando abortamento foliar.",
        severity: "Crítica"
      },
      {
        category: "Retenção Hídrica",
        description: "Textura arenosa com drenagem excessiva e esgotamento da água disponível do solo em menos de 4 dias de estiagem.",
        severity: "Alta"
      },
      {
        category: "Ataque de Fauna",
        description: "Desfolha e quebra mecânica de plantas nas faixas limítrofes provocadas por incursão de capivaras.",
        severity: "Média"
      }
    ],
    recommendations: [
      "Realização prioritária de gessagem agrícola profunda para neutralizar o Al3+ subsuperficial e carrear cálcio.",
      "Calagem com calcário dolomítico com PRNT superior a 80%, visando elevar a saturação por bases (V%) para 70%.",
      "Plantio de espécies de cobertura de sistema radicular agressivo (ex: Braquiária ruziziensis) para geração de palhada e retenção de umidade.",
      "Instalação de barreiras físicas e eletrificação móvel nas bordaduras do talhão para contenção de capivaras."
    ]
  };

  // Respostas estáticas inteligentes para o chat livre
  const getStaticChatReply = (message, talhaoContext) => {
    const msg = (message || '').toLowerCase();

    if (msg.includes('aluminio') || msg.includes('alumínio') || msg.includes('acidez') || msg.includes('ph')) {
      return {
        reply: `### Diagnóstico & Manejo de Acidez e Alumínio Tóxico\n\nNo **${talhaoContext || 'Talhão Grão 4.0'}**, identificamos **pH de 4.65** e **1.48 cmol/dm³ de Al³⁺**.\n\n**Ações Recomendadas:**\n1. **Gessagem Agrícola:** Aplicar gesso fosfatado para carrear sulfato e cálcio em profundidade (>20 cm), desintoxicando o perfil subsuperficial.\n2. **Calagem Corretiva:** Aplicar calcário dolomítico visando elevar o V% para 70% e o pH para a faixa ideal (5.8 a 6.2).\n3. **Benefício Esperado:** Permite que as raízes do milho ultrapassem os 40 cm de profundidade, alcançando reservas hídricas em anos secos.`,
        model: "Simulador Estático FarmLab"
      };
    }

    if (msg.includes('clima') || msg.includes('chuva') || msg.includes('temperatura') || msg.includes('veranico')) {
      return {
        reply: `### Análise de Estresse Termo-Hídrico\n\nDurante o ciclo reprodutivo do milho (estádios VT/R1), as variáveis meteorológicas apontaram:\n- **Precipitação:** 0.0 mm na semana crítica 7.\n- **Temperatura Máxima:** 35.9°C.\n\nEssa condição provocou inviabilidade do pólen e colapso de evapotranspiração, acelerando a quebra do índice NDVI de **0.827** para **0.507**.`,
        model: "Simulador Estático FarmLab"
      };
    }

    if (msg.includes('capivara') || msg.includes('fauna') || msg.includes('praga')) {
      return {
        reply: `### Manejo de Danos por Capivaras e Fauna Silvestre\n\nOs danos nas faixas limítrofes do talhão causaram dessecação prematura e perda de estande.\n\n**Medidas Imediatas:**\n- Instalação de cerca elétrica de baixa amperagem com fio condutor a 20 e 40 cm de altura.\n- Monitoramento ativo com registros nas Ordens de Serviço (OS) do sistema de gestão.`,
        model: "Simulador Estático FarmLab"
      };
    }

    return {
      reply: `Olá! Sou o **Engenheiro Agrônomo Virtual do GeoMAC**.\n\nAnalisando o **${talhaoContext || 'Talhão Grão 4.0'}**, observo que os principais gargalos são a **toxidez por alumínio (1.48 cmol)**, **pH ácido (4.65)** e o **veranico no estádio reprodutivo**.\n\nVocê pode me perguntar detalhes sobre:\n- Dosagem e estratégias de calagem e gessagem\n- Escolha de híbridos tolerantes para a próxima safra\n- Técnicas de semeadura direta e retenção hídrica em solos arenosos`,
      model: "Simulador Estático FarmLab"
    };
  };

  // 1. Gera Diagnóstico Agronômico (Static ou LM Studio)
  async function generateDiagnosis(payload) {
    const mode = getMode();

    if (mode === 'static') {
      // Simulação instantânea com leve delay para UX natural
      await new Promise(r => setTimeout(r, 600));
      return {
        ...STATIC_DIAGNOSIS_DATA,
        _mode: 'static'
      };
    }

    // Modo LM Studio em tempo real
    const apiBase = getApiBase();
    const res = await fetch(`${apiBase}/assistant/diagnose`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });

    if (!res.ok) {
      const errData = await res.json().catch(() => ({}));
      const msg = errData.message || errData.detail || `Erro na API: status ${res.status}`;
      throw new Error(msg);
    }

    const data = await res.json();
    data._mode = 'live';
    return data;
  }

  // 2. Chat com Agrônomo (Static ou LM Studio)
  async function sendChatMessage(message, talhaoContext) {
    const mode = getMode();

    if (mode === 'static') {
      await new Promise(r => setTimeout(r, 500));
      const res = getStaticChatReply(message, talhaoContext);
      return {
        ...res,
        _mode: 'static'
      };
    }

    // Modo LM Studio em tempo real
    const apiBase = getApiBase();
    const res = await fetch(`${apiBase}/assistant/chat`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ message, talhaoContext })
    });

    if (!res.ok) {
      const errData = await res.json().catch(() => ({}));
      const msg = errData.message || errData.detail || `Falha no LM Studio: status ${res.status}`;
      throw new Error(msg);
    }

    const data = await res.json();
    data._mode = 'live';
    return data;
  }

  // 3. Monta e gerencia o Componente Visual do Botão Toggle (exatamente como solicitado)
  function setupDataSourceToggle(containerEl, onModeChange) {
    if (!containerEl) return;

    const initialMode = getMode();
    const isLive = initialMode === 'live';

    containerEl.innerHTML = `
      <div class="data-source-control">
        <div class="data-source-title">
          <i class="fa-solid fa-sliders"></i> Tipo de Dados
        </div>
        <div class="data-source-wrapper">
          <span class="toggle-side-label left ${!isLive ? 'active static-mode' : ''}" id="labelStatic" title="Execução sem depender do LM Studio">
            <i class="fa-solid fa-database"></i> Dados Estáticos (Demonstração / Offline)
          </span>

          <label class="ios-switch" title="Alternar entre Dados Estáticos e LM Studio">
            <input type="checkbox" id="dataSourceToggleCheckbox" ${isLive ? 'checked' : ''}>
            <span class="ios-slider"></span>
          </label>

          <span class="toggle-side-label right ${isLive ? 'active' : ''}" id="labelLive" title="Comunicação em tempo real com Qwen 2.5 7B">
            <i class="fa-solid fa-brain"></i> LM Studio (IA em Tempo Real)
          </span>
        </div>
        <div class="data-source-status-bar ${isLive ? 'live' : 'static'}" id="dataSourceStatus">
          <span class="status-dot"></span>
          <span id="dataSourceStatusText">${
            isLive
              ? 'Conectando ao modelo Qwen 2.5 7B via LM Studio (localhost:1235 / API .NET)'
              : 'Modo Demonstração ativo: dados analíticos pré-calculados do Talhão Grão 4.0'
          }</span>
        </div>
      </div>
    `;

    const checkbox = containerEl.querySelector('#dataSourceToggleCheckbox');
    const labelStatic = containerEl.querySelector('#labelStatic');
    const labelLive = containerEl.querySelector('#labelLive');
    const statusBar = containerEl.querySelector('#dataSourceStatus');
    const statusText = containerEl.querySelector('#dataSourceStatusText');

    function updateUi(live) {
      checkbox.checked = live;
      setMode(live ? 'live' : 'static');

      if (live) {
        labelLive.classList.add('active');
        labelStatic.classList.remove('active', 'static-mode');
        statusBar.className = 'data-source-status-bar live';
        statusText.textContent = 'Conectando ao modelo Qwen 2.5 7B via LM Studio (localhost:1235 / API .NET)';
      } else {
        labelStatic.classList.add('active', 'static-mode');
        labelLive.classList.remove('active');
        statusBar.className = 'data-source-status-bar static';
        statusText.textContent = 'Modo Demonstração ativo: dados analíticos pré-calculados do Talhão Grão 4.0';
      }

      if (typeof onModeChange === 'function') {
        onModeChange(live ? 'live' : 'static');
      }
    }

    checkbox.addEventListener('change', (e) => {
      updateUi(e.target.checked);
    });

    labelStatic.addEventListener('click', () => {
      if (checkbox.checked) updateUi(false);
    });

    labelLive.addEventListener('click', () => {
      if (!checkbox.checked) updateUi(true);
    });
  }

  return {
    getMode,
    setMode,
    SAMPLE_GRAO_40,
    generateDiagnosis,
    sendChatMessage,
    setupDataSourceToggle
  };
})();

