using System.Globalization;

namespace Servicio2_API.Servicios;


public class GeneradorAutorizacion
{
    private readonly AlmacenDatos _almacen;

    public GeneradorAutorizacion(AlmacenDatos almacen) => _almacen = almacen;

  
    public long Siguiente(DateTime fecha)
    {
        string dia = fecha.ToString("yyyyMMdd", CultureInfo.InvariantCulture);

        // Busco cuál fue el último número que di ese día. Si es el primero del día, n queda en 0.
        _almacen.Correlativos.TryGetValue(dia, out int n);
        n++;                           // el siguiente número
        _almacen.Correlativos[dia] = n;   // lo apunto para no repetirlo

       
        return long.Parse(dia + n.ToString("D8", CultureInfo.InvariantCulture));
    }
}
