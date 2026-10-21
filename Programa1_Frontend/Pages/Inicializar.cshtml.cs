using Microsoft.AspNetCore.Mvc.RazorPages;
using Programa1_Frontend.Modelos;
using Programa1_Frontend.Servicios;

namespace Programa1_Frontend.Pages;

// Esta página es la opción "Inicializar": le manda un mensaje a la API para que borre todos los datos
// y el sistema quede como al principio.
public class InicializarModel : PageModel
{
    private readonly ClienteApi _api;

    public InicializarModel(ClienteApi api) => _api = api;

    public EstadoApi? Estado { get; private set; }
    public string? Mensaje { get; private set; }
    public string? Error { get; private set; }

    public async Task OnGetAsync() => await CargarEstado();

    public async Task OnPostAsync()
    {
        var resultado = await _api.Inicializar();
        if (resultado.Ok) Mensaje = resultado.Datos!.Mensaje;
        else Error = resultado.Error;

        await CargarEstado();
    }

    // Muestra cuántos datos tiene la API (antes y después de inicializar).
    private async Task CargarEstado()
    {
        var estado = await _api.Estado();
        if (estado.Ok) Estado = estado.Datos;
        else Error ??= estado.Error;
    }
}
