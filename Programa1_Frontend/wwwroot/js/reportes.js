// Este archivo arma los reportes en PDF.
// Usa jsPDF (para crear el PDF) y su plugin autoTable (para las tablas).
// Los dos están en wwwroot/lib/jspdf, así no hace falta internet.
//
// Se usa así: Reportes.descargar({ iva: datos, rangoTexto: '...' }, 'archivo.pdf')
// El objeto puede traer una o varias secciones: reporte, contribuyente, iva, rango.
// Cada página manda solo la suya, y la página "Reporte PDF" las manda todas juntas.

const Reportes = (() => {
    const AZUL = [31, 78, 156];
    const MARGEN = 14;

    // Pone el dinero como "Q 1,234.50".
    function dinero(n) {
        return 'Q ' + Number(n).toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    }

    // La fecha y hora de ahorita, como dd/mm/aaaa hh:mm.
    function ahora() {
        const d = new Date();
        const dos = x => String(x).padStart(2, '0');
        return dos(d.getDate()) + '/' + dos(d.getMonth() + 1) + '/' + d.getFullYear() + ' ' + dos(d.getHours()) + ':' + dos(d.getMinutes());
    }

    // Crea el PDF en hoja horizontal y escribe el título arriba.
    // "ctx" guarda el documento y la posición vertical (y) donde se va a escribir lo siguiente.
    function nuevoDocumento(titulo, rangoTexto) {
        const { jsPDF } = window.jspdf;
        const doc = new jsPDF({ orientation: 'landscape', unit: 'mm', format: 'letter' });
        doc.setFont('helvetica', 'bold');
        doc.setFontSize(18);
        doc.setTextColor(...AZUL);
        doc.text(titulo, MARGEN, 18);
        doc.setFont('helvetica', 'normal');
        doc.setFontSize(9);
        doc.setTextColor(110);
        doc.text('Generado el ' + ahora() + (rangoTexto ? '   |   Rango de fechas: ' + rangoTexto : ''), MARGEN, 24);
        doc.setTextColor(0);
        return { doc, y: 32 };
    }

    // Si ya no cabe lo que sigue en la página, pasa a una página nueva.
    function haySitio(ctx, alto) {
        const altoPagina = ctx.doc.internal.pageSize.getHeight();
        if (ctx.y + alto > altoPagina - 16) {
            ctx.doc.addPage();
            ctx.y = 18;
        }
    }

    function subtitulo(ctx, texto) {
        haySitio(ctx, 24);
        ctx.doc.setFont('helvetica', 'bold');
        ctx.doc.setFontSize(13);
        ctx.doc.setTextColor(...AZUL);
        ctx.doc.text(texto, MARGEN, ctx.y);
        ctx.doc.setTextColor(0);
        ctx.y += 7;
    }

    function linea(ctx, texto) {
        haySitio(ctx, 8);
        ctx.doc.setFont('helvetica', 'normal');
        ctx.doc.setFontSize(10);
        ctx.doc.text(texto, MARGEN, ctx.y);
        ctx.y += 5.5;
    }

    function sinDatos(ctx) {
        linea(ctx, 'No hay datos para este filtro.');
        ctx.y += 4;
    }

    function tabla(ctx, columnas, filas) {
        haySitio(ctx, 24);
        ctx.doc.autoTable({
            startY: ctx.y,
            head: [columnas],
            body: filas,
            theme: 'grid',
            styles: { fontSize: 9, cellPadding: 2 },
            headStyles: { fillColor: AZUL, textColor: 255 },
            margin: { left: MARGEN, right: MARGEN }
        });
        ctx.y = ctx.doc.lastAutoTable.finalY + 8;
    }

    function imagen(ctx, url) {
        const ancho = 200, alto = 76;
        haySitio(ctx, alto + 4);
        ctx.doc.addImage(url, 'PNG', MARGEN, ctx.y, ancho, alto);
        ctx.y += alto + 8;
    }

    // ---------------------------------------------------------------- las secciones

    // El reporte por fecha, con los errores (el cuadro del enunciado).
    function seccionReporte(ctx, lista) {
        subtitulo(ctx, 'Reporte de facturas por fecha');
        if (!lista || lista.length === 0) { sinDatos(ctx); return; }
        tabla(ctx,
            ['Fecha', 'Recibidas', 'Emisor inválido', 'Receptor inválido', 'Emisor inexistente', 'Receptor inexistente',
             'IVA mal', 'TOTAL mal', 'Ref. duplicada', 'Sin errores', 'Emisores', 'Receptores'],
            lista.map(r => [r.fecha, r.cantidadFacturasRecibidas, r.errores.nitEmisorInvalido, r.errores.nitReceptorInvalido,
                r.errores.nitEmisorInexistente, r.errores.nitReceptorInexistente, r.errores.ivaMalCalculado,
                r.errores.totalMalCalculado, r.errores.referenciaDuplicada, r.cantidadFacturasSinErrores,
                r.cantidadEmisores, r.cantidadReceptores]));
    }

    function seccionContribuyente(ctx, d) {
        subtitulo(ctx, 'Consulta de contribuyente');
        linea(ctx, 'NIT: ' + d.nit + (d.nombre ? '  -  ' + d.nombre : ''));
        linea(ctx, 'Facturas ' + d.tipo + ' (solo las autorizadas)');
        ctx.y += 2;
        if (d.detalle.length === 0) { sinDatos(ctx); return; }
        tabla(ctx, ['Fecha', 'Cantidad de facturas', 'Monto total'],
            d.detalle.map(l => [l.fecha, l.cantidadFacturas, dinero(l.montoTotal)]));
        linea(ctx, 'Total facturado en todo el rango: ' + dinero(d.totalFacturado));
        ctx.y += 4;
    }

    function seccionIva(ctx, d) {
        subtitulo(ctx, 'Resumen de IVA por fecha y NIT - ' + d.fecha);
        if (d.nits.length === 0) { sinDatos(ctx); return; }
        tabla(ctx, ['NIT', 'Nombre', 'IVA emitido', 'IVA recibido'],
            d.nits.map(n => [n.nit, n.nombre || '', dinero(n.ivaEmitido), dinero(n.ivaRecibido)]));
        imagen(ctx, Graficas.imagenIva(d));
    }

    function seccionRango(ctx, d) {
        const modo = d.modo === 'sinIva' ? 'valores sin IVA' : 'valores totales (con IVA)';
        subtitulo(ctx, 'Resumen por rango de fechas - ' + modo);
        if (d.detalle.length === 0) { sinDatos(ctx); return; }
        tabla(ctx, ['Fecha', 'Valor autorizado'], d.detalle.map(l => [l.fecha, dinero(l.valor)]));
        linea(ctx, 'Total del rango: ' + dinero(d.totalGeneral));
        ctx.y += 4;
        imagen(ctx, Graficas.imagenRango(d));
    }

    // ---------------------------------------------------------------- armar y descargar

    function descargar(secciones, archivo) {
        const cuantas = ['reporte', 'contribuyente', 'iva', 'rango'].filter(k => secciones[k]).length;
        const titulo = cuantas > 1 ? 'Reporte de facturas - SAT' : 'Reporte - ' + (
            secciones.reporte ? 'Facturas por fecha' :
            secciones.contribuyente ? 'Consulta de contribuyente' :
            secciones.iva ? 'Resumen de IVA' : 'Resumen por rango');

        const ctx = nuevoDocumento(titulo, secciones.rangoTexto);
        if (secciones.reporte) seccionReporte(ctx, secciones.reporte);
        if (secciones.contribuyente) seccionContribuyente(ctx, secciones.contribuyente);
        if (secciones.iva) seccionIva(ctx, secciones.iva);
        if (secciones.rango) seccionRango(ctx, secciones.rango);

        // Al final se numeran las páginas.
        const total = ctx.doc.getNumberOfPages();
        const ancho = ctx.doc.internal.pageSize.getWidth();
        const alto = ctx.doc.internal.pageSize.getHeight();
        for (let i = 1; i <= total; i++) {
            ctx.doc.setPage(i);
            ctx.doc.setFont('helvetica', 'normal');
            ctx.doc.setFontSize(8);
            ctx.doc.setTextColor(120);
            ctx.doc.text('Página ' + i + ' de ' + total, ancho / 2, alto - 8, { align: 'center' });
        }
        ctx.doc.save(archivo);
    }

    return { descargar };
})();
