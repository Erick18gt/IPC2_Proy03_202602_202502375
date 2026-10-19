using System.Globalization;

namespace Servicio2_API.Utilidades;


public static class Fechas
{
   
    private static readonly string[] Formatos = { "dd/MM/yyyy", "d/M/yyyy", "yyyy-MM-dd" };

    // Intenta convertir el texto a fecha. Devuelve true si pudo y false si el texto no es una fecha.
    public static bool TryParsear(string? texto, out DateTime fecha)
        => DateTime.TryParseExact((texto ?? "").Trim(), Formatos, CultureInfo.InvariantCulture,
                                  DateTimeStyles.None, out fecha);

    // Escribe la fecha como dd/MM/yyyy, que es como la muestro en las respuestas.
    public static string Texto(DateTime fecha)
        => fecha.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

    public static string? LeerRango(string? inicioTexto, string? finTexto, out DateTime? inicio, out DateTime? fin)
    {
        inicio = null;
        fin = null;

        if (!string.IsNullOrWhiteSpace(inicioTexto))
        {
            if (!TryParsear(inicioTexto, out DateTime i))
                return $"La fecha de inicio '{inicioTexto}' no es valida. Use dd/MM/yyyy.";
            inicio = i.Date;
        }
        if (!string.IsNullOrWhiteSpace(finTexto))
        {
            if (!TryParsear(finTexto, out DateTime f))
                return $"La fecha final '{finTexto}' no es valida. Use dd/MM/yyyy.";
            fin = f.Date;
        }
        if (inicio != null && fin != null && inicio > fin)
            return "La fecha de inicio no puede ser mayor que la fecha final.";

        return null;
    }
}
