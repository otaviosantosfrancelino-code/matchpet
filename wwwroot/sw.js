// Service worker do Match-Pet (aplicativo instalável).
// O site é Blazor Server (precisa de conexão em tempo real), então o service worker NÃO guarda páginas:
// ele só mostra uma tela amigável quando a pessoa abre o app sem internet.
const CACHE = 'matchpet-offline-v2';
const OFFLINE = '/offline.html';

self.addEventListener('install', (e) => {
    e.waitUntil(caches.open(CACHE).then((c) => c.addAll([OFFLINE, '/icones/app-192.png'])).then(() => self.skipWaiting()));
});

self.addEventListener('activate', (e) => {
    e.waitUntil(caches.keys()
        .then((chaves) => Promise.all(chaves.filter((k) => k !== CACHE).map((k) => caches.delete(k))))
        .then(() => self.clients.claim()));
});

self.addEventListener('fetch', (e) => {
    // Só navegação de página; todo o resto (fotos, _blazor, API) vai direto para a rede
    if (e.request.mode !== 'navigate') return;
    e.respondWith(fetch(e.request).catch(() => caches.match(OFFLINE)));
});
