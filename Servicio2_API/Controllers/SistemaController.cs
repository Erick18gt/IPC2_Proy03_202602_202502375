using Microsoft.AspNetCore.Mvc;
using Servicio2_API.Servicios;

namespace Servicio2_API.Controllers;


[ApiController]
[Route("api/sistema")]
public class SistemaController : ControllerBase
{
    private readonly ServicioConsultas _servicio;

    public SistemaController(ServicioConsultas servicio) => _servicio = servicio;

  
    [HttpPost("inicializar")]
    public IActionResult Inicializar()
    {
        _servicio.Inicializar();
        return Ok(new { mensaje = "Sistema inicializado: no hay datos." });
    }

 
    [HttpGet("estado")]
    public IActionResult Estado() => Ok(_servicio.Estado());
}
