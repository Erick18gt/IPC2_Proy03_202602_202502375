using System.Text;
using System.Text.Json;
using Programa1_Frontend.Modelos;

namespace Programa1_Frontend.Servicios;

// Esta clase es para hablar con la API (Servicio2_API). Todas las peticiones del frontend pasan por aquí,
// así las páginas no tienen que saber nada de URLs ni de HTTP.
public class ClienteApi
{
    private static readonly JsonSerializerOptions Opciones = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _http;

    public ClienteApi(HttpClient http) => _http = http;

    // ----------------------------------------------------------- envío de mensajes

    // Manda el archivo tal cual lo subió el usuario, sin tocarlo,
    // porque el enunciado dice que los mensajes de entrada no se pueden modificar.
    public Task<Resultado<RespuestaRtu>> EnviarRtu(string json)
        => Llamar<RespuestaRtu>(HttpMethod.Post, "api/rtu", json);

    public Task<Resultado<RespuestaAutorizaciones>> EnviarAutorizaciones(string json)
        => Llamar<RespuestaAutorizaciones>(HttpMethod.Post, "api/autorizaciones", json);

    // ----------------------------------------------------------- consultas

    public Task<Resultado<List<ReporteFecha>>> ReporteFacturas(string? inicio, string? fin)
        => Llamar<List<ReporteFecha>>(HttpMethod.Get,
            Url("api/consultas/reporte-facturas", ("inicio", inicio), ("fin", fin)));

    public Task<Resultado<ConsultaContribuyente>> ConsultarContribuyente(string nit, string tipo, string? inicio, string? fin)
        => Llamar<ConsultaContribuyente>(HttpMethod.Get,
            Url("api/consultas/contribuyente", ("nit", nit), ("tipo", tipo), ("inicio", inicio), ("fin", fin)));

    public Task<Resultado<ResumenIva>> ResumenIva(string fecha)
        => Llamar<ResumenIva>(HttpMethod.Get, Url("api/consultas/resumen-iva", ("fecha", fecha)));

    public Task<Resultado<ResumenRango>> ResumenRango(string? inicio, string? fin, string modo)
        => Llamar<ResumenRango>(HttpMethod.Get,
            Url("api/consultas/resumen-rango", ("inicio", inicio), ("fin", fin), ("modo", modo)));

    // ----------------------------------------------------------- RTU y sistema

    public Task<Resultado<List<Contribuyente>>> ListarRtu()
        => Llamar<List<Contribuyente>>(HttpMethod.Get, "api/rtu");

    public Task<Resultado<EstadoApi>> Estado()
        => Llamar<EstadoApi>(HttpMethod.Get, "api/sistema/estado");

    public Task<Resultado<MensajeApi>> Inicializar()
        => Llamar<MensajeApi>(HttpMethod.Post, "api/sistema/inicializar");

    // ----------------------------------------------------------- lo de adentro

    // Arma una URL con sus parámetros, por ejemplo api/consultas/resumen-iva?fecha=2025-01-15.
    // Los parámetros vacíos no se agregan.
    private static string Url(string ruta, params (string nombre, string? valor)[] parametros)
    {
        var partes = parametros
            .Where(p => !string.IsNullOrWhiteSpace(p.valor))
            .Select(p => $"{p.nombre}={Uri.EscapeDataString(p.valor!.Trim())}");
        string query = string.Join("&", partes);
        return query.Length == 0 ? ruta : ruta + "?" + query;
    }

    // Hace la petición y convierte la respuesta a la clase T.
    // Si la API no está encendida, o responde con error, devuelve un mensaje claro en vez de romperse.
    private async Task<Resultado<T>> Llamar<T>(HttpMethod metodo, string ruta, string? cuerpoJson = null)
    {
        try
        {
            using var peticion = new HttpRequestMessage(metodo, ruta);
            if (cuerpoJson != null)
                peticion.Content = new StringContent(cuerpoJson, Encoding.UTF8, "application/json");

            using var respuesta = await _http.SendAsync(peticion);
            string texto = await respuesta.Content.ReadAsStringAsync();

            if (!respuesta.IsSuccessStatusCode)
                return Resultado<T>.Falla($"La API respondió con error {(int)respuesta.StatusCode}: {texto}");

            T? datos = JsonSerializer.Deserialize<T>(texto, Opciones);
            if (datos == null) return Resultado<T>.Falla("La API respondió algo vacío.");
            return Resultado<T>.Exito(datos, texto);
        }
        catch (HttpRequestException)
        {
            return Resultado<T>.Falla("No se pudo conectar con la API. Revisa que Servicio2_API esté corriendo y que la dirección en appsettings.json (ApiUrl) sea la correcta.");
        }
        catch (TaskCanceledException)
        {
            return Resultado<T>.Falla("La API tardó demasiado en responder.");
        }
        catch (JsonException)
        {
            return Resultado<T>.Falla("La respuesta de la API no tiene el formato esperado.");
        }
    }
}
