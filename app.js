// ==========================================
// FARMLAB DASHBOARD — APP LOGIC & DATA
// ==========================================

// --- CONFIG & UTILS ---
Chart.defaults.color = '#94a3b8';
Chart.defaults.font.family = "'Inter', sans-serif";
Chart.defaults.scale.grid.color = 'rgba(100, 116, 139, 0.1)';

// Animação de Scroll (Reveal)
document.addEventListener('DOMContentLoaded', () => {
  const observer = new IntersectionObserver((entries) => {
    entries.forEach(entry => {
      if (entry.isIntersecting) {
        entry.target.classList.add('visible');
      }
    });
  }, { threshold: 0.1 });

  document.querySelectorAll('.reveal').forEach(el => observer.observe(el));

  // Efeito de nav no scroll
  window.addEventListener('scroll', () => {
    const nav = document.querySelector('.nav');
    if (window.scrollY > 50) {
      nav.classList.add('scrolled');
    } else {
      nav.classList.remove('scrolled');
    }
  });
});

// --- DADOS REAVALIADOS (Safra Real: Nov 2025 - Fev 2026) ---

// Dados NDVI (Foco no ciclo real Pós-Plantio)
const ndviData = {
  labels: ['04/Nov (Plantio)', '24/Nov', '17/Dez', '27/Dez (Pendoamento)', '01/Jan', '16/Jan (Pico)', '31/Jan (Colapso)', '03/Fev (Colheita)'],
  grao40: [0.369, 0.521, 0.612, 0.624, 0.696, 0.827, 0.507, 0.485],
  graoConvencional: [0.365, 0.530, 0.620, 0.650, 0.720, 0.840, 0.710, 0.650] // Suportou melhor o estresse
};

// Dados de Solo
const soilData = {
  labels: ['pH (CaCl2)', 'Sat. Bases (%)', 'Fósforo (mg/dm³)', 'Alumínio (cmol)', 'Matéria Orgânica (%)', 'Argila (%)'],
  amostraNormal: [6.3, 79, 13.2, 0, 8.5, 12],
  amostraProblema: [4.6, 47, 18.9, 1.5, 5.0, 14]
};

// Dados Meteorológicos (Semanas do ciclo)
const meteoData = {
  labels: ['Sem 3 (Nov)', 'Sem 4 (Dez)', 'Sem 5 (Dez)', 'Sem 6 (Dez)', 'Sem 7 (Dez-Crítico)', 'Sem 8 (Dez-Crítico)', 'Sem 10 (Jan)'],
  precipitacao: [18.0, 0.0, 20.4, 271.8, 0.0, 16.8, 29.2], // mm
  tempMax: [34.0, 35.0, 34.2, 29.8, 32.9, 35.9, 32.5] // °C
};

// --- GRÁFICOS ---

window.addEventListener('load', () => {
  // 1. Gráfico de Evolução NDVI
  const ctxNdvi = document.getElementById('chartNdvi').getContext('2d');
  new Chart(ctxNdvi, {
    type: 'line',
    data: {
      labels: ndviData.labels,
      datasets: [
        {
          label: 'Grão 4.0 (Raiz Superficial - Sofreu Estresse)',
          data: ndviData.grao40,
          borderColor: '#ef4444',
          backgroundColor: 'rgba(239, 68, 68, 0.1)',
          borderWidth: 3,
          tension: 0.4,
          fill: true,
          pointBackgroundColor: '#ef4444'
        },
        {
          label: 'Grão Convencional',
          data: ndviData.graoConvencional,
          borderColor: '#3b82f6',
          borderWidth: 2,
          borderDash: [5, 5],
          tension: 0.4,
          pointBackgroundColor: '#3b82f6'
        }
      ]
    },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: { position: 'bottom' },
        tooltip: {
          mode: 'index',
          intersect: false,
          backgroundColor: 'rgba(15, 23, 42, 0.9)',
          titleColor: '#fff',
          bodyColor: '#e2e8f0',
          borderColor: 'rgba(34, 197, 94, 0.3)',
          borderWidth: 1
        },
        annotation: {
          annotations: {
            line1: {
              type: 'line',
              xMin: '27/Dez (Pendoamento)',
              xMax: '27/Dez (Pendoamento)',
              borderColor: 'rgba(245, 158, 11, 0.5)',
              borderWidth: 2,
              label: { content: 'Onda de Calor', enabled: true, position: 'top' }
            }
          }
        }
      },
      scales: {
        y: {
          min: 0.2,
          max: 1.0,
          title: { display: true, text: 'Valor NDVI' }
        }
      },
      interaction: {
        mode: 'nearest',
        axis: 'x',
        intersect: false
      }
    }
  });

  // 2. Gráfico de Diagnóstico de Solo (Radar)
  const ctxSoil = document.getElementById('chartSoil').getContext('2d');
  new Chart(ctxSoil, {
    type: 'radar',
    data: {
      labels: soilData.labels,
      datasets: [
        {
          label: 'Zona Problemática (Alumínio Tóxico)',
          data: [4.6, 47, 18.9, 80, 5.0, 14], // Alumínio escalado para visualização
          backgroundColor: 'rgba(239, 68, 68, 0.2)',
          borderColor: '#ef4444',
          pointBackgroundColor: '#ef4444',
          pointBorderColor: '#fff',
          pointHoverBackgroundColor: '#fff',
          pointHoverBorderColor: '#ef4444'
        },
        {
          label: 'Zona Saudável',
          data: [6.3, 79, 10.6, 0, 8.5, 12],
          backgroundColor: 'rgba(34, 197, 94, 0.2)',
          borderColor: '#22c55e',
          pointBackgroundColor: '#22c55e',
          pointBorderColor: '#fff',
          pointHoverBackgroundColor: '#fff',
          pointHoverBorderColor: '#22c55e'
        }
      ]
    },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      scales: {
        r: {
          angleLines: { color: 'rgba(255, 255, 255, 0.1)' },
          grid: { color: 'rgba(255, 255, 255, 0.1)' },
          pointLabels: { color: '#94a3b8', font: { size: 11 } },
          ticks: { display: false }
        }
      },
      plugins: {
        legend: { position: 'bottom' }
      }
    }
  });

  // 3. Gráfico Meteorológico (Bar + Line) - REAVALIADO
  const ctxMeteo = document.getElementById('chartMeteo').getContext('2d');
  new Chart(ctxMeteo, {
    type: 'bar',
    data: {
      labels: meteoData.labels,
      datasets: [
        {
          type: 'bar',
          label: 'Precipitação (mm)',
          data: meteoData.precipitacao,
          backgroundColor: 'rgba(59, 130, 246, 0.6)',
          borderColor: '#3b82f6',
          borderWidth: 1,
          yAxisID: 'y'
        },
        {
          type: 'line',
          label: 'Temperatura Máxima (°C)',
          data: meteoData.tempMax,
          borderColor: '#ef4444',
          backgroundColor: '#ef4444',
          borderWidth: 3,
          tension: 0.4,
          yAxisID: 'y1'
        }
      ]
    },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      interaction: { mode: 'index', intersect: false },
      plugins: { legend: { position: 'bottom' } },
      scales: {
        y: {
          type: 'linear',
          display: true,
          position: 'left',
          title: { display: true, text: 'Chuva (mm)' },
          grid: { color: 'rgba(255, 255, 255, 0.05)' }
        },
        y1: {
          type: 'linear',
          display: true,
          position: 'right',
          title: { display: true, text: 'Temp Máx (°C)' },
          min: 25,
          max: 40,
          grid: { drawOnChartArea: false }
        }
      }
    }
  });
});
