using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Programa1_Frontend.Pages.Ayuda;

public class EstudianteModel : PageModel
{
    private readonly IConfiguration _config;

    public EstudianteModel(IConfiguration config) => _config = config;

    public string Nombre => _config["Estudiante:Nombre"] ?? "";
    public string Carnet => _config["Estudiante:Carnet"] ?? "";
    public string Curso => _config["Estudiante:Curso"] ?? "";
    public string Seccion => _config["Estudiante:Seccion"] ?? "";
    public string Semestre => _config["Estudiante:Semestre"] ?? "";
    public string Repositorio => _config["Estudiante:Repositorio"] ?? "";
}
