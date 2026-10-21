using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Programa1_Frontend.Modelos;
using Programa1_Frontend.Servicios;

namespace Programa1_Frontend.Pages;

// Esta es la pantalla "Enviar mensaje". Recibe uno o varios archivos JSON,
// detecta de qué tipo son (RTU o facturas) y los manda a la API.
public class EnviarModel : PageModel
{
    private readonly ClienteApi _api;

    public EnviarModel(ClienteApi api) => _api = api;

    // Los archivos que eligió el usuario en el formulario.
    [BindProperty]
    public List<IFormFile> Archivos { get; set; } = new();

    // "auto" deja que el programa detecte el tipo. También se puede obligar a "rtu" o a "facturas".
    [BindProperty]
    public string Modo { get; set; } = "auto";

    public List<ResultadoArchivo> Resultados { get; } = new();
    public string? Error { get; private set; }

    public void OnGet() { }

    // Un archivo ya leído y analizado, esperando su turno para enviarse.
    private class ArchivoPendiente
    {
        public ResultadoArchivo Resultado { get; init; } = new();
        public string Texto { get; init; } = "";
        public TipoMensaje Tipo { get; init; }
    }

    public async Task OnPostAsync()
    {
        if (Archivos == null || Archivos.Count == 0)
        {
            Error = "Elige al menos un archivo .json.";
            return;
        }

        // Paso 1: leer y analizar cada archivo.
        var pendientes = new List<ArchivoPendiente>();
        foreach (IFormFile archivo in Archivos)
        {
            var resultado = new ResultadoArchivo { Nombre = archivo.FileName };
            Resultados.Add(resultado);

            string texto;
            using (var lector = new StreamReader(archivo.OpenReadStream()))
                texto = await lector.ReadToEndAsync();

            AnalisisMensaje analisis = DetectorMensaje.Analizar(texto);
            if (analisis.Error != null)
            {
                resultado.Error = analisis.Error;
                continue;
            }

            TipoMensaje tipo = Modo switch
            {
                "rtu" => TipoMensaje.Rtu,
                "facturas" => TipoMensaje.Facturas,
                _ => analisis.Tipo
            };

            if (tipo == TipoMensaje.Desconocido)
            {
                resultado.Error = "No se pudo saber si el archivo es del RTU o de facturas. Elige el tipo de mensaje manualmente.";
                continue;
            }

            resultado.Tipo = tipo == TipoMensaje.Rtu ? "RTU" : "Facturas";
            resultado.Cantidad = analisis.Cantidad;
            (resultado.Encabezados, resultado.Filas) = DetectorMensaje.VistaPrevia(texto, tipo);
            pendientes.Add(new ArchivoPendiente { Resultado = resultado, Texto = texto, Tipo = tipo });
        }

        // Paso 2: enviar a la API. Primero los del RTU y después los de facturas,
        // porque las facturas necesitan que los contribuyentes ya existan.
        foreach (ArchivoPendiente p in pendientes.OrderBy(x => x.Tipo == TipoMensaje.Rtu ? 0 : 1))
        {
            if (p.Tipo == TipoMensaje.Rtu)
            {
                var respuesta = await _api.EnviarRtu(p.Texto);
                if (respuesta.Ok) { p.Resultado.RespuestaRtu = respuesta.Datos; p.Resultado.JsonRespuesta = respuesta.Json; }
                else p.Resultado.Error = respuesta.Error;
            }
            else
            {
                var respuesta = await _api.EnviarAutorizaciones(p.Texto);
                if (respuesta.Ok) { p.Resultado.RespuestaAutorizaciones = respuesta.Datos; p.Resultado.JsonRespuesta = respuesta.Json; }
                else p.Resultado.Error = respuesta.Error;
            }
        }
    }
}
