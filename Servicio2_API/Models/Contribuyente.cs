using System.Text.Json.Serialization;

namespace Servicio2_API.Models;

// Cada contribuyente del RTU: su NIT y su nombre.
// El nombre entre comillas de cada [JsonPropertyName] es como se llama en el JSON del enunciado.
public class Contribuyente
{
    [JsonPropertyName("NIT")]
    public string? Nit { get; set; }

    [JsonPropertyName("nombre")]
    public string? Nombre { get; set; }
}
