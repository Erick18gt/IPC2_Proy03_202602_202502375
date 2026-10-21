using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Programa1_Frontend.Modelos;
using Programa1_Frontend.Servicios;

namespace Programa1_Frontend.Pages.Peticiones;

// Esta página es para "Consultar contribuyente": elegir un NIT, si se quieren las facturas
// emitidas o recibidas, y un rango de fechas. Muestra por cada fecha la cantidad de facturas
// autorizadas y el monto, y al final el total de todo el rango.
public class ContribuyenteModel : PageModel
{
    private readonly ClienteApi _api;

    public ContribuyenteModel(ClienteApi api) => _api = api;

    // SupportsGet = true hace que estos datos se lean de la dirección (?Nit=...&Tipo=...),
    // así el formulario funciona con GET y la consulta se puede copiar como link.
    [BindProperty(SupportsGet = true)] public string? Nit { get; set; }
    [BindProperty(SupportsGet = true)] public string Tipo { get; set; } = "emitidas";
    [BindProperty(SupportsGet = true)] public string? Inicio { get; set; }
    [BindProperty(SupportsGet = true)] public string? Fin { get; set; }

    public ConsultaContribuyente? Datos { get; private set; }
    public List<Contribuyente> Rtu { get; private set; } = new();
    public string? Error { get; private set; }

    public string RangoTexto => Formato.Rango(Inicio, Fin);

    // Los datos que usa el botón de PDF (se meten en la página como JSON).
    public string DatosJson => JsonSerializer.Serialize(new { rangoTexto = RangoTexto, contribuyente = Datos });

    public async Task OnGetAsync()
    {
        // La lista de NITs es solo para ayudar a escribir (autocompletar).
        var rtu = await _api.ListarRtu();
        if (rtu.Ok) Rtu = rtu.Datos!;

        if (string.IsNullOrWhiteSpace(Nit)) return;

        var resultado = await _api.ConsultarContribuyente(Nit, Tipo, Inicio, Fin);
        if (resultado.Ok) Datos = resultado.Datos;
        else Error = resultado.Error;
    }
}
