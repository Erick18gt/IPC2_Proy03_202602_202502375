using Microsoft.AspNetCore.Mvc;
using Servicio2_API.Servicios;
using Servicio2_API.Utilidades;

namespace Servicio2_API.Controllers;

// Aquí están las consultas del frontend: los reportes, el resumen de IVA y las demás.
// Todas son GET y los datos van en la URL
// Las fechas las acepto como 15/01/2025 o como 2025-01-15.
// En los rangos, si no mandan inicio o fin, ese lado queda sin límite.
[ApiController]
[Route("api/consultas")]
public class ConsultasController : ControllerBase
{
    private readonly ServicioConsultas _servicio;

    public ConsultasController(ServicioConsultas servicio) => _servicio = servicio;


    // El reporte por fecha con los errores.
    [HttpGet("reporte-facturas")]
    public IActionResult ReporteFacturas([FromQuery] string? inicio, [FromQuery] string? fin)
    {
        // LeerRango revisa que las fechas sean válidas
        string? error = Fechas.LeerRango(inicio, fin, out DateTime? i, out DateTime? f);
        if (error != null) return BadRequest(error);
        return Ok(_servicio.ReporteFacturas(i, f));
    }

 
    // "tipo" puede ser emitidas o recibidas
    [HttpGet("contribuyente")]
    public IActionResult Contribuyente([FromQuery] string? nit, [FromQuery] string? tipo,
                                       [FromQuery] string? inicio, [FromQuery] string? fin)
    {
        if (string.IsNullOrWhiteSpace(nit)) return BadRequest("Falta el parametro 'nit'.");

        string t = (tipo ?? "").Trim().ToLowerInvariant();
        if (t != "emitidas" && t != "recibidas") return BadRequest("El parametro 'tipo' debe ser 'emitidas' o 'recibidas'.");

        string? error = Fechas.LeerRango(inicio, fin, out DateTime? i, out DateTime? f);
        if (error != null) return BadRequest(error);

        return Ok(_servicio.ConsultarContribuyente(nit, t == "emitidas", i, f));
    }


    // El IVA emitido y recibido de cada NIT en un día.
    [HttpGet("resumen-iva")]
    public IActionResult ResumenIva([FromQuery] string? fecha)
    {
        if (!Fechas.TryParsear(fecha, out DateTime d))
            return BadRequest("Falta el parametro 'fecha' o no es valido. Use dd/MM/yyyy.");
        return Ok(_servicio.ResumenDeIva(d));
    }


    // "modo" puede ser total (valor + IVA) o sinIva
    [HttpGet("resumen-rango")]
    public IActionResult ResumenRango([FromQuery] string? inicio, [FromQuery] string? fin, [FromQuery] string? modo)
    {
        string m = (modo ?? "total").Trim().ToLowerInvariant();
        if (m != "total" && m != "siniva") return BadRequest("El parametro 'modo' debe ser 'total' o 'sinIva'.");

        string? error = Fechas.LeerRango(inicio, fin, out DateTime? i, out DateTime? f);
        if (error != null) return BadRequest(error);

        return Ok(_servicio.ResumenPorRango(i, f, m == "siniva"));
    }
}
