using System.Globalization;
using System.Text.Json;

namespace Programa1_Frontend.Servicios;

// Esta clase tiene ayudas para mostrar cosas en pantalla.
public static class Formato
{
    // Pone el dinero como "Q 1,234.50". Uso InvariantCulture para que se vea igual en cualquier computadora.
    public static string Dinero(decimal valor)
        => "Q " + valor.ToString("N2", CultureInfo.InvariantCulture);

    // Convierte "2025-01-15" (lo que manda el calendario de HTML) a "15/01/2025".
    public static string FechaTexto(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return "";
        string[] formatos = { "yyyy-MM-dd", "dd/MM/yyyy", "d/M/yyyy" };
        return DateTime.TryParseExact(texto.Trim(), formatos, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fecha)
            ? fecha.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)
            : texto;
    }

    // Escribe el rango de fechas en una frase: "15/01/2025 al 16/01/2025", "desde ...", "hasta ..." o "todas las fechas".
    public static string Rango(string? inicio, string? fin)
    {
        string i = FechaTexto(inicio);
        string f = FechaTexto(fin);
        if (i == "" && f == "") return "todas las fechas";
        if (i == "") return "hasta " + f;
        if (f == "") return "desde " + i;
        return i + " al " + f;
    }

    // Para mostrar el número de autorización sin separadores.
    public static string Codigo(long codigo)
        => codigo.ToString(CultureInfo.InvariantCulture);

    // Acomoda un JSON en varias líneas para que se pueda leer. Solo es para mostrarlo, no cambia los datos.
    public static string JsonBonito(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return "";
        try
        {
            using JsonDocument doc = JsonDocument.Parse(json);
            return JsonSerializer.Serialize(doc.RootElement, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (JsonException)
        {
            return json;
        }
    }
}
