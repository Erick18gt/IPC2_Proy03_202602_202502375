using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Programa1_Frontend.Modelos;
using Programa1_Frontend.Servicios;

namespace Programa1_Frontend.Pages.Peticiones;

// Esta página es para el "Resumen por rango de fechas": se elige un rango y si se quieren
// los valores totales (con IVA) o sin IVA. Muestra el valor autorizado de cada fecha, con su gráfica.
public class ResumenRangoModel : PageModel
{
    private readonly ClienteApi _api;

    public ResumenRangoModel(ClienteApi api) => _api = api;

    [BindProperty(SupportsGet = true)] public string? Inicio { get; set; }
    [BindProperty(SupportsGet = true)] public string? Fin { get; set; }
    [BindProperty(SupportsGet = true)] public string Modo { get; set; } = "total";

    public ResumenRango? Datos { get; private set; }
    public string? Error { get; private set; }

    public string RangoTexto => Formato.Rango(Inicio, Fin);

    public string DatosJson => JsonSerializer.Serialize(new { rangoTexto = RangoTexto, rango = Datos });

    public async Task OnGetAsync()
    {
        // Si todavía no se ha llenado el formulario (primera vez que se abre la página), no se consulta nada.
        if (string.IsNullOrWhiteSpace(Request.QueryString.Value)) return;

        var resultado = await _api.ResumenRango(Inicio, Fin, Modo);
        if (resultado.Ok) Datos = resultado.Datos;
        else Error = resultado.Error;
    }
}
