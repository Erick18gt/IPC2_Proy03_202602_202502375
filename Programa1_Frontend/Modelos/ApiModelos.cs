using System.Text.Json.Serialization;

namespace Programa1_Frontend.Modelos;

// Este archivo tiene las clases que usa el frontend para leer lo que responde la API.
// Son las mismas formas que están en Servicio2_API/Models/Respuestas.cs.
// Los nombres entre comillas de [JsonPropertyName] tienen que ser iguales a los del JSON de la API.

// Lo que responde la API al actualizar el RTU.
public class RespuestaRtu
{
    [JsonPropertyName("CONTRIBUYENTES_NUEVOS")] public int Nuevos { get; set; }
    [JsonPropertyName("CONTRIBUYENTES_ACTUALIZADOS")] public int Actualizados { get; set; }
    [JsonPropertyName("NITS_INVALIDOS")] public int Invalidos { get; set; }
}

// Lo que responde la API al enviar facturas.
public class RespuestaAutorizaciones
{
    [JsonPropertyName("resultadoAutorizaciones")] public ResultadoAutorizaciones Resultado { get; set; } = new();
}

public class ResultadoAutorizaciones
{
    [JsonPropertyName("cantidadAutorizacionesAprobadas")] public int Aprobadas { get; set; }
    [JsonPropertyName("cantidadAutorizacionesRechazadasPorNitEmisorInexistente")] public int RechazadasPorEmisorInexistente { get; set; }
    [JsonPropertyName("cantidadAutorizacionesRechazadasPorNitReceptorInexistente")] public int RechazadasPorReceptorInexistente { get; set; }
    [JsonPropertyName("detalleAprobaciones")] public List<DetalleAprobacion> Detalle { get; set; } = new();
}

public class DetalleAprobacion
{
    [JsonPropertyName("referencia")] public string Referencia { get; set; } = "";
    [JsonPropertyName("codigoAprobacion")] public long CodigoAprobacion { get; set; }
}

// El reporte por fecha (con los errores).
public class ReporteFecha
{
    [JsonPropertyName("fecha")] public string Fecha { get; set; } = "";
    [JsonPropertyName("cantidadFacturasRecibidas")] public int FacturasRecibidas { get; set; }
    [JsonPropertyName("errores")] public ErroresFecha Errores { get; set; } = new();
    [JsonPropertyName("cantidadFacturasSinErrores")] public int FacturasSinErrores { get; set; }
    [JsonPropertyName("cantidadEmisores")] public int Emisores { get; set; }
    [JsonPropertyName("cantidadReceptores")] public int Receptores { get; set; }
}

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

// Consultar contribuyente.
public class ConsultaContribuyente
{
    [JsonPropertyName("nit")] public string Nit { get; set; } = "";
    [JsonPropertyName("nombre")] public string? Nombre { get; set; }
    [JsonPropertyName("tipo")] public string Tipo { get; set; } = "";
    [JsonPropertyName("detalle")] public List<LineaContribuyente> Detalle { get; set; } = new();
    [JsonPropertyName("totalFacturado")] public decimal TotalFacturado { get; set; }
}

public class LineaContribuyente
{
    [JsonPropertyName("fecha")] public string Fecha { get; set; } = "";
    [JsonPropertyName("cantidadFacturas")] public int CantidadFacturas { get; set; }
    [JsonPropertyName("montoTotal")] public decimal MontoTotal { get; set; }
}

// Resumen de IVA por fecha y NIT.
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

// Resumen por rango de fechas.
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

// Un contribuyente del RTU.
public class Contribuyente
{
    [JsonPropertyName("NIT")] public string Nit { get; set; } = "";
    [JsonPropertyName("nombre")] public string? Nombre { get; set; }
}

// Cuántos datos tiene la API en este momento.
public class EstadoApi
{
    [JsonPropertyName("contribuyentes")] public int Contribuyentes { get; set; }
    [JsonPropertyName("facturasRecibidas")] public int FacturasRecibidas { get; set; }
    [JsonPropertyName("facturasAprobadas")] public int FacturasAprobadas { get; set; }
}

public class MensajeApi
{
    [JsonPropertyName("mensaje")] public string Mensaje { get; set; } = "";
}

// Esta clase sirve para devolver el resultado de una petición a la API:
// si salió bien trae los datos, y si salió mal trae el mensaje de error.
public class Resultado<T>
{
    public bool Ok { get; init; }
    public T? Datos { get; init; }
    public string? Error { get; init; }
    public string? Json { get; init; }     // la respuesta tal cual la mandó la API

    public static Resultado<T> Exito(T datos, string json) => new() { Ok = true, Datos = datos, Json = json };
    public static Resultado<T> Falla(string error) => new() { Ok = false, Error = error };
}

// El resultado de enviar UN archivo en la pantalla "Enviar mensaje".
public class ResultadoArchivo
{
    public string Nombre { get; set; } = "";
    public string Tipo { get; set; } = "";               // "RTU" o "Facturas"
    public int Cantidad { get; set; }                    // cuántos registros traía el archivo
    public string[] Encabezados { get; set; } = Array.Empty<string>();
    public List<string[]> Filas { get; set; } = new();   // vista previa de los primeros registros
    public string? Error { get; set; }
    public RespuestaRtu? RespuestaRtu { get; set; }
    public RespuestaAutorizaciones? RespuestaAutorizaciones { get; set; }
    public string? JsonRespuesta { get; set; }
}
