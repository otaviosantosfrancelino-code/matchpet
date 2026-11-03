// Funções JavaScript chamadas pelas páginas Blazor (C#)

// Liga as animações de entrada (sem JavaScript, tudo aparece normalmente)
document.documentElement.classList.add("anim");

// Volta ao topo ao trocar de página
window.mpRolarTopo = () => window.scrollTo({ top: 0, behavior: 'instant' });

// Copiar texto (Pix etc.); devolve false se o navegador bloquear
window.mpCopiar = async (texto) => {
    try { await navigator.clipboard.writeText(texto); return true; } catch { return false; }
};

// Leva a lista de mensagens do chat até a última
window.mpRolarFim = (id) => {
    const el = document.getElementById(id);
    if (el) el.scrollTop = el.scrollHeight;
};

// Abre a janela de impressão (lá o usuário escolhe "Salvar como PDF")
window.mpImprimir = () => window.print();

// ----- MAPA (Leaflet + OpenStreetMap), carregado só quando alguma página precisa -----
let leafletCarregando = null;
function carregarLeaflet() {
    if (window.L) return Promise.resolve();
    if (leafletCarregando) return leafletCarregando;
    leafletCarregando = new Promise((ok, erro) => {
        const css = document.createElement('link');
        css.rel = 'stylesheet';
        css.href = 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.4/leaflet.min.css';
        document.head.appendChild(css);
        const js = document.createElement('script');
        js.src = 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.4/leaflet.min.js';
        js.onload = ok;
        js.onerror = erro;
        document.head.appendChild(js);
    });
    return leafletCarregando;
}

// Mostra um círculo (área aproximada, por privacidade) em vez do endereço exato
window.mpMapa = async (id, lat, lng, raioMetros, titulo) => {
    await carregarLeaflet();
    const el = document.getElementById(id);
    if (!el) return;
    if (el._mapa) { el._mapa.remove(); }
    const mapa = L.map(el, { scrollWheelZoom: false, attributionControl: true }).setView([lat, lng], 13);
    L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 18,
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>'
    }).addTo(mapa);
    const area = L.circle([lat, lng], { radius: raioMetros, color: '#2563eb', fillColor: '#3b82f6', fillOpacity: 0.18, weight: 2 }).addTo(mapa);
    area.bindPopup(titulo);
    mapa.fitBounds(area.getBounds(), { padding: [20, 20] });
    el._mapa = mapa;
    setTimeout(() => mapa.invalidateSize(), 200);
};

// ----- VÍDEOS (aba de vídeos da Comunidade) -----
// Toca o vídeo que está visível na tela e pausa os outros
let observadorVideos = null;
window.mpObservarVideos = () => {
    if (observadorVideos) observadorVideos.disconnect();
    observadorVideos = new IntersectionObserver((itens) => {
        itens.forEach(i => {
            const v = i.target;
            if (i.isIntersecting && i.intersectionRatio > 0.6) { v.play().catch(() => { }); }
            else { v.pause(); }
        });
    }, { threshold: [0, 0.6, 1] });
    document.querySelectorAll('video[data-auto]').forEach(v => observadorVideos.observe(v));
};

// Liga/desliga o som do vídeo que está tocando
window.mpAlternarSom = () => {
    document.querySelectorAll('video[data-auto]').forEach(v => { if (!v.paused) v.muted = !v.muted; });
};

// Liga/desliga o som de todos os vídeos da tela cheia
window.mpDefinirSom = (mudo) => {
    document.querySelectorAll('video[data-auto]').forEach(v => { v.muted = mudo; });
    const tocando = [...document.querySelectorAll('video[data-auto]')].find(v => !v.paused);
    if (tocando && !mudo) tocando.play().catch(() => { });
};

// ----- ANIMAÇÕES AO ROLAR (todas as páginas) -----
// Elementos com data-revelar aparecem suavemente quando entram na tela.
// Elementos com data-contar="123" contam de 0 até o número.
(function () {
    const reduzir = window.matchMedia('(prefers-reduced-motion: reduce)').matches;

    const contar = (el) => {
        const alvo = parseInt(el.dataset.contar || '0', 10);
        if (reduzir || !alvo) { el.textContent = alvo.toLocaleString('pt-BR'); return; }
        const inicio = performance.now(), duracao = 1200;
        const passo = (t) => {
            const p = Math.min(1, (t - inicio) / duracao);
            const suave = 1 - Math.pow(1 - p, 3);
            el.textContent = Math.round(alvo * suave).toLocaleString('pt-BR');
            if (p < 1) requestAnimationFrame(passo);
        };
        requestAnimationFrame(passo);
    };

    const observador = new IntersectionObserver((itens) => {
        itens.forEach(i => {
            if (!i.isIntersecting) return;
            const el = i.target;
            if (el.hasAttribute('data-revelar')) el.classList.add('revelado');
            if (el.hasAttribute('data-contar') && !el.dataset.contado) { el.dataset.contado = '1'; contar(el); }
            observador.unobserve(el);
        });
    }, { threshold: 0.12, rootMargin: '0px 0px -40px 0px' });

    const observarNovos = (raiz) => {
        raiz.querySelectorAll?.('[data-revelar]:not(.revelado), [data-contar]:not([data-contado])').forEach(el => observador.observe(el));
    };

    // O Blazor troca o conteúdo da página sem recarregar: observa o que for aparecendo
    new MutationObserver((mudancas) => {
        mudancas.forEach(m => m.addedNodes.forEach(n => { if (n.nodeType === 1) { observarNovos(n); if (n.matches?.('[data-revelar],[data-contar]')) observador.observe(n); } }));
    }).observe(document.documentElement, { childList: true, subtree: true });

    document.addEventListener('DOMContentLoaded', () => observarNovos(document));
})();
