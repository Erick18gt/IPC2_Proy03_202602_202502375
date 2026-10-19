namespace Servicio2_API.Utilidades;

public static class Calculadora
{

    public static decimal Iva(decimal valor)
        => Math.Round(valor * 0.12m, 2, MidpointRounding.AwayFromZero);
}
