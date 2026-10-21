using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Programa1_Frontend.Modelos;
using Programa1_Frontend.Servicios;

namespace Programa1_Frontend.Pages.Peticiones;

// Esta página muestra el reporte del enunciado: por cada fecha, cuántas facturas llegaron,
// cuántos errores hubo de cada tipo, cuántas salieron sin errores y cuántos emisores y receptores distintos hubo.
public class ReporteFacturasModel : PageModel
{
    private readonly ClienteApi _api;

    public ReporteFacturasModel(ClienteApi api) => _api = api;

    [BindProperty(SupportsGet = true)] public string? Inicio { get; set; }
    [BindProperty(SupportsGet = true)] public string? Fin { get; set; }

    public List<ReporteFecha>? Reporte { get; private set; }
    public string? Error { get; private set; }

    public string RangoTexto => Formato.Rango(Inicio, Fin);

    public string DatosJson => JsonSerializer.Serialize(new { rangoTexto = RangoTexto, reporte = Reporte });

    // Esta página consulta apenas se abre (sin fechas muestra todas las fechas que haya).
    public async Task OnGetAsync()
    {
        var resultado = await _api.ReporteFacturas(Inicio, Fin);
        if (resultado.Ok) Reporte = resultado.Datos;
        else Error = resultado.Error;
    }
}
