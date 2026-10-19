const chartInstances = {};
const mapInstances = {};

window.renderMap = function (elementId, lat, lng) {
    if (mapInstances[elementId]) {
        mapInstances[elementId].remove();
        delete mapInstances[elementId];
    }
    const el = document.getElementById(elementId);
    if (!el) return;
    const map = L.map(elementId).setView([lat, lng], 15);
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors',
        maxZoom: 19
    }).addTo(map);
    L.marker([lat, lng]).addTo(map);
    mapInstances[elementId] = map;
};

window.destroyMap = function (elementId) {
    if (mapInstances[elementId]) {
        mapInstances[elementId].remove();
        delete mapInstances[elementId];
    }
};

window.getCurrentPosition = function () {
    return new Promise(function (resolve, reject) {
        if (!navigator.geolocation) { reject('Geolocation not supported'); return; }
        navigator.geolocation.getCurrentPosition(
            function (pos) { resolve({ Latitude: pos.coords.latitude, Longitude: pos.coords.longitude }); },
            function (err) { reject(err.message); },
            { enableHighAccuracy: true, timeout: 10000 }
        );
    });
};

window.renderPieChart = function (canvasId, labels, data, colors) {
    if (chartInstances[canvasId]) {
        chartInstances[canvasId].destroy();
    }
    const ctx = document.getElementById(canvasId);
    if (!ctx) return;
    chartInstances[canvasId] = new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels,
            datasets: [{
                data,
                backgroundColor: colors.length ? colors : labels.map((_, i) => `hsl(${i * 40}, 60%, 50%)`),
                borderWidth: 0
            }]
        },
        options: {
            plugins: {
                legend: { display: false }
            },
            cutout: '65%'
        }
    });
};

window.renderLineChart = function (canvasId, labels, series) {
    if (chartInstances[canvasId]) {
        chartInstances[canvasId].destroy();
    }
    const ctx = document.getElementById(canvasId);
    if (!ctx) return;
    const single = series.length <= 1;
    chartInstances[canvasId] = new Chart(ctx, {
        type: 'line',
        data: {
            labels,
            datasets: series.map(s => ({
                label: s.label,
                data: s.data,
                borderColor: s.color,
                backgroundColor: s.color + '1a',
                borderWidth: 2,
                fill: single,
                tension: 0.3,
                pointRadius: 0
            }))
        },
        options: {
            plugins: { legend: { display: !single, labels: { color: '#8b949e' } } },
            scales: {
                x: {
                    ticks: { color: '#8b949e', maxTicksLimit: 8 },
                    grid: { color: '#21262d' }
                },
                y: {
                    ticks: { color: '#8b949e' },
                    grid: { color: '#21262d' }
                }
            }
        }
    });
};
