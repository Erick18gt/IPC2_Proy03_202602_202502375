using System.Text.RegularExpressions;

namespace Servicio2_API.Utilidades;


public static class ExtractorFecha
{
    
    private static readonly Regex Patron = new(
        @"([0-9]{1,2})/([0-9]{1,2})/([0-9]{4})(?:\s+([0-9]{1,2}):([0-9]{2}))?",
        RegexOptions.Compiled);

    
    public static DateTime? Extraer(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return null;

        Match m = Patron.Match(texto);
        if (!m.Success) return null;     // el texto no tiene nada parecido a una fecha

        // Convierto los pedacitos de texto a números.
        int dia = int.Parse(m.Groups[1].Value);
        int mes = int.Parse(m.Groups[2].Value);
        int anio = int.Parse(m.Groups[3].Value);
        int hora = m.Groups[4].Success ? int.Parse(m.Groups[4].Value) : 0;
        int minuto = m.Groups[5].Success ? int.Parse(m.Groups[5].Value) : 0;

        // La regex solo me asegura que "parece" una fecha. Si la fecha no existe,
        // new DateTime lanza un error y yo lo atrapo para devolver null.
        try
        {
            return new DateTime(anio, mes, dia, hora, minuto, 0);
        }
        catch (ArgumentOutOfRangeException)
        {
            return null;
        }
    }
}
