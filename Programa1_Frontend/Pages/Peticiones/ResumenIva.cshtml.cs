using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Programa1_Frontend.Modelos;
using Programa1_Frontend.Servicios;

namespace Programa1_Frontend.Pages.Peticiones;

// Esta página es para el "Resumen de IVA por fecha y NIT": se elige una fecha y se ve,
// por cada NIT, el IVA que emitió y el IVA que recibió ese día. También tiene su gráfica.
public class ResumenIvaModel : PageModel
{
    private readonly ClienteApi _api;

    public ResumenIvaModel(ClienteApi api) => _api = api;

    [BindProperty(SupportsGet = true)] public string? Fecha { get; set; }

    public ResumenIva? Datos { get; private set; }
    public string? Error { get; private set; }

    public string DatosJson => JsonSerializer.Serialize(new { rangoTexto = Formato.FechaTexto(Fecha), iva = Datos });

    public async Task OnGetAsync()
    {
        if (string.IsNullOrWhiteSpace(Fecha)) return;

        var resultado = await _api.ResumenIva(Fecha);
        if (resultado.Ok) Datos = resultado.Datos;
        else Error = resultado.Error;
    }
}
