using Microsoft.AspNetCore.Mvc.RazorPages;
using Programa1_Frontend.Modelos;
using Programa1_Frontend.Servicios;

namespace Programa1_Frontend.Pages;

// Esta es la página de inicio. Le pregunta a la API cuántos datos tiene para mostrarlos arriba.
public class IndexModel : PageModel
{
    private readonly ClienteApi _api;

    public IndexModel(ClienteApi api) => _api = api;

    public EstadoApi? Estado { get; private set; }
    public string? Error { get; private set; }

    public async Task OnGetAsync()
    {
        var resultado = await _api.Estado();
        if (resultado.Ok) Estado = resultado.Datos;
        else Error = resultado.Error;
    }
}
