// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

let dashboardScale = 1.0;

function applyDashboardZoom() {
    const container = document.getElementById('dashboardContainer');
    if (!container) return;
    container.style.transformOrigin = '0 0';
    container.style.transform = `scale(${dashboardScale})`;
}

function zoomInDashboard() {
    dashboardScale = Math.min(dashboardScale + 0.1, 2);
    applyDashboardZoom();
}

function zoomOutDashboard() {
    dashboardScale = Math.max(dashboardScale - 0.1, 0.5);
    applyDashboardZoom();
}

document.addEventListener('DOMContentLoaded', function () {
    document.getElementById('zoomInBtn')?.addEventListener('click', zoomInDashboard);
    document.getElementById('zoomOutBtn')?.addEventListener('click', zoomOutDashboard);
});
