using System.Text.Json;

namespace Programa1_Frontend.Servicios;

public enum TipoMensaje { Desconocido, Rtu, Facturas }

public class AnalisisMensaje
{
    public TipoMensaje Tipo { get; set; } = TipoMensaje.Desconocido;
    public int Cantidad { get; set; }
    public string? Error { get; set; }
}

// Esta clase sirve para mirar un archivo JSON antes de mandarlo a la API:
// saber si es una actualización del RTU o una solicitud de facturas, cuántos registros trae
// y sacar una vista previa para mostrarla en pantalla.
// Solo LEE el archivo. Lo que se manda a la API es el texto original, sin cambios.
public static class DetectorMensaje
{
    public static AnalisisMensaje Analizar(string json)
    {
        try
        {
            using JsonDocument doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind != JsonValueKind.Array)
                return new AnalisisMensaje { Error = "El archivo debe tener una lista JSON (empieza con [ y termina con ])." };

            var analisis = new AnalisisMensaje { Cantidad = doc.RootElement.GetArrayLength() };

            // Se revisan los campos del primer registro que sea un objeto:
            // si trae REFERENCIA, NIT_EMISOR o TIEMPO son facturas; si solo trae NIT, es del RTU.
            foreach (JsonElement elemento in doc.RootElement.EnumerateArray())
            {
                if (elemento.ValueKind != JsonValueKind.Object) continue;

                var campos = elemento.EnumerateObject().Select(p => p.Name.ToUpperInvariant()).ToList();
                if (campos.Contains("REFERENCIA") || campos.Contains("NIT_EMISOR") || campos.Contains("TIEMPO"))
                    analisis.Tipo = TipoMensaje.Facturas;
                else if (campos.Contains("NIT"))
                    analisis.Tipo = TipoMensaje.Rtu;
                break;
            }
            return analisis;
        }
        catch (JsonException ex)
        {
            return new AnalisisMensaje { Error = "El archivo no es un JSON válido: " + ex.Message };
        }
    }

    // Saca los primeros registros del archivo para mostrarlos en una tabla.
    public static (string[] encabezados, List<string[]> filas) VistaPrevia(string json, TipoMensaje tipo, int maximo = 10)
    {
        string[] campos = tipo == TipoMensaje.Rtu
            ? new[] { "NIT", "nombre" }
            : new[] { "TIEMPO", "REFERENCIA", "NIT_EMISOR", "NIT_RECEPTOR", "VALOR", "IVA", "TOTAL" };

        var filas = new List<string[]>();
        using JsonDocument doc = JsonDocument.Parse(json);
        foreach (JsonElement elemento in doc.RootElement.EnumerateArray())
        {
            if (filas.Count >= maximo) break;
            if (elemento.ValueKind != JsonValueKind.Object) continue;
            filas.Add(campos.Select(c => Valor(elemento, c)).ToArray());
        }
        return (campos, filas);
    }

    // Busca un campo sin importar mayúsculas o minúsculas y devuelve su valor como texto.
    private static string Valor(JsonElement objeto, string campo)
    {
        foreach (JsonProperty p in objeto.EnumerateObject())
        {
            if (string.Equals(p.Name, campo, StringComparison.OrdinalIgnoreCase))
                return p.Value.ValueKind == JsonValueKind.String ? (p.Value.GetString() ?? "") : p.Value.GetRawText();
        }
        return "";
    }
}
