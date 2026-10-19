using Microsoft.AspNetCore.Mvc;
using Servicio2_API.Models;
using Servicio2_API.Servicios;

namespace Servicio2_API.Controllers;

// Este controlador es la puerta de entrada para todo lo del RTU: la URL es /api/rtu.

[ApiController]
[Route("api/rtu")]
public class RtuController : ControllerBase
{
    private readonly ServicioRtu _servicio;

    // La API me entrega el ServicioRtu ya creado
    public RtuController(ServicioRtu servicio) => _servicio = servicio;


    // [FromBody] significa que esa lista viene dentro del JSON que envían.
    [HttpPost]
    public IActionResult Actualizar([FromBody] List<Contribuyente?>? lista)
    {
        if (lista == null) return BadRequest("El cuerpo debe ser una lista JSON de contribuyentes.");
        return Ok(_servicio.Actualizar(lista));
    }

    // GET api/rtu: devuelvo todos los contribuyentes que tengo guardados.
    [HttpGet]
    public IActionResult Listar() => Ok(_servicio.Listar());
}
