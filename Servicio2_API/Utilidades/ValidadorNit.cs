using System.Text.RegularExpressions;

namespace Servicio2_API.Utilidades;


public static class ValidadorNit
{
    // [0-9] (y no \d) porque \d en .NET tambien acepta digitos de otros alfabetos.
    private static readonly Regex Formato = new(@"^[0-9]{1,20}[0-9K]$", RegexOptions.Compiled);


    public static string Normalizar(string? nit) => (nit ?? "").Trim().ToUpperInvariant();

    public static bool EsValido(string? nit)
    {
        string normal = Normalizar(nit);
        if (!Formato.IsMatch(normal)) return false;

        string cuerpo = normal.Substring(0, normal.Length - 1);   // todo menos el ultimo
        char verificador = normal[normal.Length - 1];             // el ultimo caracter
        return CalcularVerificador(cuerpo) == verificador;
    }

    
    public static char CalcularVerificador(string cuerpo)
    {
        int suma = 0;
        int posicion = 2;
        for (int i = cuerpo.Length - 1; i >= 0; i--)
        {
            suma += (cuerpo[i] - '0') * posicion;
            posicion++;
        }
        int resultado = (11 - (suma % 11)) % 11;
        return resultado == 10 ? 'K' : (char)('0' + resultado);
    }
}
