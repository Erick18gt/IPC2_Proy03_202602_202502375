using Servicio2_API.Models;
using Servicio2_API.Utilidades;

namespace Servicio2_API.Servicios;

// Aquí manejo el RTU: crear contribuyentes nuevos y actualizar los que ya existen.
public class ServicioRtu
{
    private readonly AlmacenDatos _almacen;

    public ServicioRtu(AlmacenDatos almacen) => _almacen = almacen;


  
    public RespuestaRtu Actualizar(List<Contribuyente?> lista)
    {
        var respuesta = new RespuestaRtu();

        lock (_almacen.Candado)    // nadie más toca los datos mientras yo trabajo
        {
            foreach (Contribuyente? c in lista)
            {
                if (c == null || !ValidadorNit.EsValido(c.Nit))
                {
                    respuesta.Invalidos++;
                    continue;      // paso al siguiente contribuyente
                }

                string nit = ValidadorNit.Normalizar(c.Nit);

                if (_almacen.Rtu.ContainsKey(nit)) respuesta.Actualizados++;
                else respuesta.Nuevos++;

                // Se guarda igual en los dos casos: si no existía lo crea, y si existía lo reemplaza.
                _almacen.Rtu[nit] = new Contribuyente { Nit = nit, Nombre = c.Nombre ?? "" };
            }

            _almacen.GuardarRtu();   // guardo en el archivo una sola vez, al terminar
        }
        return respuesta;
    }

    // Devuelve todos los contribuyentes ordenados por NIT.
    public List<Contribuyente> Listar()
    {
        lock (_almacen.Candado)
            return _almacen.Rtu.Values.OrderBy(c => c.Nit, StringComparer.Ordinal).ToList();
    }
}
