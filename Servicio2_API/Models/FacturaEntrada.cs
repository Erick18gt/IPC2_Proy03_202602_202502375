using System.Text.Json.Serialization;

namespace Servicio2_API.Models;

// Una factura tal como llega en el JSON que me manda el frontend (o Postman).

public class FacturaEntrada
{
    [JsonPropertyName("TIEMPO")]
    public string? Tiempo { get; set; }          // ej: "Guatemala, 15/01/2025 15:25 hrs."

    [JsonPropertyName("REFERENCIA")]
    public string? Referencia { get; set; }

    [JsonPropertyName("NIT_EMISOR")]
    public string? NitEmisor { get; set; }

    [JsonPropertyName("NIT_RECEPTOR")]
    public string? NitReceptor { get; set; }

    // Uso decimal (y no double) porque es dinero y necesito que los centavos queden exactos.
    [JsonPropertyName("VALOR")]
    public decimal Valor { get; set; }

    [JsonPropertyName("IVA")]
    public decimal Iva { get; set; }

    [JsonPropertyName("TOTAL")]
    public decimal Total { get; set; }
}
