using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Programa1_Frontend.Pages.Ayuda;


public class DocumentacionModel : PageModel
{
    private readonly IWebHostEnvironment _entorno;

    public DocumentacionModel(IWebHostEnvironment entorno) => _entorno = entorno;

    public bool Existe { get; private set; }

    public void OnGet()
    {
        string ruta = Path.Combine(_entorno.WebRootPath, "docs", "documentacion.pdf");
        Existe = System.IO.File.Exists(ruta);
    }
}
