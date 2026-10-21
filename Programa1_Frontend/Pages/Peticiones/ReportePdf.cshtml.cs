using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Programa1_Frontend.Modelos;
using Programa1_Frontend.Servicios;

namespace Programa1_Frontend.Pages.Peticiones;

// Esta página es para el reporte completo en PDF: junta en un solo archivo todas las peticiones
// (reporte de facturas, consulta de contribuyente, resumen de IVA y resumen por rango).
// Se llenan los filtros, se piden los datos a la API y el PDF se arma en el navegador con reportes.js.
public class ReportePdfModel : PageModel
{
    private readonly ClienteApi _api;

    public ReportePdfModel(ClienteApi api) => _api = api;

    // Rango de fechas que usan el reporte de facturas, la consulta de contribuyente y el resumen por rango.
    [BindProperty(SupportsGet = true)] public string? Inicio { get; set; }
    [BindProperty(SupportsGet = true)] public string? Fin { get; set; }

    // Para la consulta de contribuyente (si no se escribe un NIT, esa parte no entra al PDF).
    [BindProperty(SupportsGet = true)] public string? Nit { get; set; }
    [BindProperty(SupportsGet = true)] public string Tipo { get; set; } = "emitidas";

    // Para el resumen de IVA (si no se elige fecha, esa parte no entra al PDF).
    [BindProperty(SupportsGet = true)] public string? FechaIva { get; set; }

    // Para el resumen por rango.
    [BindProperty(SupportsGet = true)] public string Modo { get; set; } = "total";

    public List<ReporteFecha>? Reporte { get; private set; }
    public ConsultaContribuyente? Contribuyente { get; private set; }
    public ResumenIva? Iva { get; private set; }
    public ResumenRango? Rango { get; private set; }
    public List<string> Errores { get; } = new();
    public bool Generado { get; private set; }

    public string RangoTexto => Formato.Rango(Inicio, Fin);

    public string DatosJson => JsonSerializer.Serialize(new
    {
        rangoTexto = RangoTexto,
        reporte = Reporte,
        contribuyente = Contribuyente,
        iva = Iva,
        rango = Rango
    });

    public async Task OnGetAsync()
    {
        // Si todavía no se ha llenado el formulario (primera vez que se abre la página), no se consulta nada.
        if (string.IsNullOrWhiteSpace(Request.QueryString.Value)) return;
        Generado = true;

        var reporte = await _api.ReporteFacturas(Inicio, Fin);
        if (reporte.Ok) Reporte = reporte.Datos; else Errores.Add("Reporte de facturas: " + reporte.Error);

        if (!string.IsNullOrWhiteSpace(Nit))
        {
            var contribuyente = await _api.ConsultarContribuyente(Nit, Tipo, Inicio, Fin);
            if (contribuyente.Ok) Contribuyente = contribuyente.Datos; else Errores.Add("Consulta de contribuyente: " + contribuyente.Error);
        }

        if (!string.IsNullOrWhiteSpace(FechaIva))
        {
            var iva = await _api.ResumenIva(FechaIva);
            if (iva.Ok) Iva = iva.Datos; else Errores.Add("Resumen de IVA: " + iva.Error);
        }

        var rango = await _api.ResumenRango(Inicio, Fin, Modo);
        if (rango.Ok) Rango = rango.Datos; else Errores.Add("Resumen por rango: " + rango.Error);
    }
}
