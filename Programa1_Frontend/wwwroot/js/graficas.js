// Este archivo dibuja las gráficas de los dos resúmenes (IVA por NIT y valor por fecha).
// Usa Chart.js, que está guardado en wwwroot/lib/chartjs, así no hace falta internet.

const Graficas = (() => {
    const AZUL = '#2563eb';
    const NARANJA = '#ea580c';

    // Este plugin pinta el fondo de blanco. Sin él, la gráfica sale transparente cuando se mete en el PDF.
    const fondoBlanco = {
        id: 'fondoBlanco',
        beforeDraw(grafica) {
            const c = grafica.ctx;
            c.save();
            c.globalCompositeOperation = 'destination-over';
            c.fillStyle = '#ffffff';
            c.fillRect(0, 0, grafica.width, grafica.height);
            c.restore();
        }
    };

    // Opciones que comparten las dos gráficas. "enPdf" es true cuando se dibuja escondida para meterla al PDF.
    function opciones(titulo, tituloY, tituloX, enPdf) {
        return {
            responsive: !enPdf,
            maintainAspectRatio: false,
            animation: enPdf ? false : { duration: 400 },
            devicePixelRatio: enPdf ? 1.5 : undefined,
            plugins: {
                title: { display: true, text: titulo, font: { size: 15 } },
                legend: { position: 'bottom' }
            },
            scales: {
                y: { beginAtZero: true, title: { display: true, text: tituloY } },
                x: { title: { display: true, text: tituloX } }
            }
        };
    }

    // Gráfica 1: por cada NIT, el IVA emitido y el IVA recibido en la fecha elegida.
    function configIva(datos, enPdf) {
        return {
            type: 'bar',
            data: {
                labels: datos.nits.map(n => n.nit),
                datasets: [
                    { label: 'IVA emitido', data: datos.nits.map(n => n.ivaEmitido), backgroundColor: AZUL },
                    { label: 'IVA recibido', data: datos.nits.map(n => n.ivaRecibido), backgroundColor: NARANJA }
                ]
            },
            options: opciones('IVA emitido y recibido por NIT - ' + datos.fecha, 'IVA (Q)', 'NIT', enPdf),
            plugins: enPdf ? [fondoBlanco] : []
        };
    }

    // Gráfica 2: por cada fecha, el valor autorizado (con IVA o sin IVA, según el modo).
    function configRango(datos, enPdf) {
        const etiqueta = datos.modo === 'sinIva' ? 'Valor sin IVA' : 'Valor total (con IVA)';
        return {
            type: 'bar',
            data: {
                labels: datos.detalle.map(d => d.fecha),
                datasets: [{ label: etiqueta, data: datos.detalle.map(d => d.valor), backgroundColor: AZUL }]
            },
            options: opciones('Valor autorizado por fecha - ' + etiqueta, 'Valor (Q)', 'Fecha', enPdf),
            plugins: enPdf ? [fondoBlanco] : []
        };
    }

    // Dibuja la gráfica en un canvas de la página.
    function iva(canvas, datos) { return new Chart(canvas, configIva(datos, false)); }
    function rango(canvas, datos) { return new Chart(canvas, configRango(datos, false)); }

    // Dibuja la gráfica en un canvas escondido y devuelve la imagen (para ponerla en el PDF).
    function aImagen(config) {
        const canvas = document.createElement('canvas');
        canvas.width = 1000;
        canvas.height = 380;
        const grafica = new Chart(canvas, config);
        const imagen = canvas.toDataURL('image/png');
        grafica.destroy();
        return imagen;
    }
    function imagenIva(datos) { return aImagen(configIva(datos, true)); }
    function imagenRango(datos) { return aImagen(configRango(datos, true)); }

    return { iva, rango, imagenIva, imagenRango };
})();
