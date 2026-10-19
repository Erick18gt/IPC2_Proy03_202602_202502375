using System.Text.Json.Serialization;

namespace Servicio2_API.Models;

public class RegistroFactura
{
    public DateTime Fecha { get; set; }            // solo me importa el día; el lugar y la hora los descarto
    public string Referencia { get; set; } = "";
    public string NitEmisor { get; set; } = "";
    public string NitReceptor { get; set; } = "";
    public decimal Valor { get; set; }
    public decimal Iva { get; set; }
    public decimal Total { get; set; }

    // Un true por cada error que se le encontró. Una factura puede tener varios a la vez.
    public bool NitEmisorInvalido { get; set; }
    public bool NitReceptorInvalido { get; set; }
    public bool NitEmisorInexistente { get; set; }
    public bool NitReceptorInexistente { get; set; }
    public bool IvaMalCalculado { get; set; }
    public bool TotalMalCalculado { get; set; }
    public bool ReferenciaDuplicada { get; set; }

    public bool Aprobada { get; set; }
    public long? CodigoAprobacion { get; set; }    // el número yyyymmdd######## (solo si se aprobó)

    // Es true si la factura tiene aunque sea UN error. No se guarda en el archivo porque se calcula sola.
    [JsonIgnore]
    public bool TieneErrores =>
        NitEmisorInvalido || NitReceptorInvalido || NitEmisorInexistente || NitReceptorInexistente ||
        IvaMalCalculado || TotalMalCalculado || ReferenciaDuplicada;
}
