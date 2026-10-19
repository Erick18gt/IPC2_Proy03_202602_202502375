using System.Text.Json.Serialization;

namespace Servicio2_API.Models;

// En este archivo están todas las "formas" de lo que mi API responde.


public class RespuestaRtu
{
    [JsonPropertyName("CONTRIBUYENTES_NUEVOS")]
    public int Nuevos { get; set; }

    [JsonPropertyName("CONTRIBUYENTES_ACTUALIZADOS")]
    public int Actualizados { get; set; }

    [JsonPropertyName("NITS_INVALIDOS")]
    public int Invalidos { get; set; }
}

// Lo que respondo cuando me mandan facturas para autorizar.
public class RespuestaAutorizaciones
{
    [JsonPropertyName("resultadoAutorizaciones")]
    public ResultadoAutorizaciones Resultado { get; set; } = new();
}

public class ResultadoAutorizaciones
{
    [JsonPropertyName("cantidadAutorizacionesAprobadas")]
    public int Aprobadas { get; set; }

    [JsonPropertyName("cantidadAutorizacionesRechazadasPorNitEmisorInexistente")]
    public int RechazadasPorEmisorInexistente { get; set; }

    [JsonPropertyName("cantidadAutorizacionesRechazadasPorNitReceptorInexistente")]
    public int RechazadasPorReceptorInexistente { get; set; }

    // La lista de facturas aprobadas, cada una con su número de autorización.
    [JsonPropertyName("detalleAprobaciones")]
    public List<DetalleAprobacion> Detalle { get; set; } = new();
}

public class DetalleAprobacion
{
    [JsonPropertyName("referencia")]
    public string Referencia { get; set; } = "";

    [JsonPropertyName("codigoAprobacion")]
    public long CodigoAprobacion { get; set; }
}


// El reporte por fecha: es el cuadro del enunciado con "FECHA, cantidad de facturas, errores, etc."
public class ReporteFecha
{
    [JsonPropertyName("fecha")] public string Fecha { get; set; } = "";
    [JsonPropertyName("cantidadFacturasRecibidas")] public int FacturasRecibidas { get; set; }
    [JsonPropertyName("errores")] public ErroresFecha Errores { get; set; } = new();
    [JsonPropertyName("cantidadFacturasSinErrores")] public int FacturasSinErrores { get; set; }
    [JsonPropertyName("cantidadEmisores")] public int Emisores { get; set; }
    [JsonPropertyName("cantidadReceptores")] public int Receptores { get; set; }
}

// La cantidad de facturas con cada tipo de error en un día.
public class ErroresFecha
{
    [JsonPropertyName("nitEmisorInvalido")] public int NitEmisorInvalido { get; set; }
    [JsonPropertyName("nitReceptorInvalido")] public int NitReceptorInvalido { get; set; }
    [JsonPropertyName("nitEmisorInexistente")] public int NitEmisorInexistente { get; set; }
    [JsonPropertyName("nitReceptorInexistente")] public int NitReceptorInexistente { get; set; }
    [JsonPropertyName("ivaMalCalculado")] public int IvaMalCalculado { get; set; }
    [JsonPropertyName("totalMalCalculado")] public int TotalMalCalculado { get; set; }
    [JsonPropertyName("referenciaDuplicada")] public int ReferenciaDuplicada { get; set; }
}

// "Consultar contribuyente": las facturas emitidas o recibidas por un NIT, día por día.
public class ConsultaContribuyente
{
    [JsonPropertyName("nit")] public string Nit { get; set; } = "";
    [JsonPropertyName("nombre")] public string? Nombre { get; set; }
    [JsonPropertyName("tipo")] public string Tipo { get; set; } = "";
    [JsonPropertyName("detalle")] public List<LineaContribuyente> Detalle { get; set; } = new();
    [JsonPropertyName("totalFacturado")] public decimal TotalFacturado { get; set; }
}

// Una línea de esa consulta: un día con su cantidad de facturas y el monto.
public class LineaContribuyente
{
    [JsonPropertyName("fecha")] public string Fecha { get; set; } = "";
    [JsonPropertyName("cantidadFacturas")] public int CantidadFacturas { get; set; }
    [JsonPropertyName("montoTotal")] public decimal MontoTotal { get; set; }
}

// "Resumen de IVA por fecha y NIT": en un día, cuánto IVA emitió y recibió cada NIT.
public class ResumenIva
{
    [JsonPropertyName("fecha")] public string Fecha { get; set; } = "";
    [JsonPropertyName("nits")] public List<LineaIva> Nits { get; set; } = new();
}

public class LineaIva
{
    [JsonPropertyName("nit")] public string Nit { get; set; } = "";
    [JsonPropertyName("nombre")] public string? Nombre { get; set; }
    [JsonPropertyName("ivaEmitido")] public decimal IvaEmitido { get; set; }
    [JsonPropertyName("ivaRecibido")] public decimal IvaRecibido { get; set; }
}

// "Resumen por rango de fechas": el valor autorizado de cada día (con IVA o sin IVA).
public class ResumenRango
{
    [JsonPropertyName("modo")] public string Modo { get; set; } = "";
    [JsonPropertyName("detalle")] public List<LineaRango> Detalle { get; set; } = new();
    [JsonPropertyName("totalGeneral")] public decimal TotalGeneral { get; set; }
}

public class LineaRango
{
    [JsonPropertyName("fecha")] public string Fecha { get; set; } = "";
    [JsonPropertyName("valor")] public decimal Valor { get; set; }
}
