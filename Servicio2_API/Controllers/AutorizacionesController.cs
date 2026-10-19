using Microsoft.AspNetCore.Mvc;
using Servicio2_API.Models;
using Servicio2_API.Servicios;

namespace Servicio2_API.Controllers;

// Puerta de entrada para las solicitudes de autorización: la URL es /api/autorizaciones.
[ApiController]
[Route("api/autorizaciones")]
public class AutorizacionesController : ControllerBase
{
    private readonly ServicioAutorizaciones _servicio;

    public AutorizacionesController(ServicioAutorizaciones servicio) => _servicio = servicio;

    // POST api/autorizaciones: me mandan la lista de facturas en un JSON.
    // Se la paso al servicio junto con la fecha de hoy (DateTime.Now), que se usa para
    // el número de autorización, y devuelvo lo que él me responda.
    [HttpPost]
    public IActionResult Procesar([FromBody] List<FacturaEntrada?>? facturas)
    {
        if (facturas == null) return BadRequest("El cuerpo debe ser una lista JSON de facturas.");
        return Ok(_servicio.Procesar(facturas, DateTime.Now));
    }
}
