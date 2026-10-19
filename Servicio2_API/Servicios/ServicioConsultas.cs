using Servicio2_API.Models;
using Servicio2_API.Utilidades;

namespace Servicio2_API.Servicios;


public class ServicioConsultas
{
    private readonly AlmacenDatos _almacen;

    public ServicioConsultas(AlmacenDatos almacen) => _almacen = almacen;


    private static bool EnRango(DateTime fecha, DateTime? inicio, DateTime? fin)
        => (inicio == null || fecha >= inicio) && (fin == null || fecha <= fin);

 
    private string? NombreDe(string nit)
        => _almacen.Rtu.TryGetValue(nit, out Contribuyente? c) ? c.Nombre : null;

    public List<ReporteFecha> ReporteFacturas(DateTime? inicio, DateTime? fin)
    {
        lock (_almacen.Candado)
        {
            return _almacen.Facturas
                .Where(f => EnRango(f.Fecha, inicio, fin))
                .GroupBy(f => f.Fecha)               // separo las facturas en un grupo por cada día
                .OrderBy(g => g.Key)                 // g.Key es el día; los ordeno de menor a mayor
                .Select(g => new ReporteFecha        // por cada día armo su resumen
                {
                    Fecha = Fechas.Texto(g.Key),
                    FacturasRecibidas = g.Count(),
                    Errores = new ErroresFecha
                    {
                        // Cuento en el grupo del día cuántas facturas tienen cada error.
                        NitEmisorInvalido = g.Count(f => f.NitEmisorInvalido),
                        NitReceptorInvalido = g.Count(f => f.NitReceptorInvalido),
                        NitEmisorInexistente = g.Count(f => f.NitEmisorInexistente),
                        NitReceptorInexistente = g.Count(f => f.NitReceptorInexistente),
                        IvaMalCalculado = g.Count(f => f.IvaMalCalculado),
                        TotalMalCalculado = g.Count(f => f.TotalMalCalculado),
                        ReferenciaDuplicada = g.Count(f => f.ReferenciaDuplicada)
                    },
                    FacturasSinErrores = g.Count(f => !f.TieneErrores),
      
                    Emisores = g.Select(f => f.NitEmisor).Distinct().Count(),
                    Receptores = g.Select(f => f.NitReceptor).Distinct().Count()
                })
                .ToList();
        }
    }

   
    public ConsultaContribuyente ConsultarContribuyente(string nit, bool emitidas, DateTime? inicio, DateTime? fin)
    {
        nit = ValidadorNit.Normalizar(nit);

        lock (_almacen.Candado)
        {
            var detalle = _almacen.Facturas
                .Where(f => f.Aprobada && EnRango(f.Fecha, inicio, fin))                  // solo las autorizadas
                .Where(f => (emitidas ? f.NitEmisor : f.NitReceptor) == nit)              // las de ese NIT
                .GroupBy(f => f.Fecha)
                .OrderBy(g => g.Key)
                .Select(g => new LineaContribuyente
                {
                    Fecha = Fechas.Texto(g.Key),
                    CantidadFacturas = g.Count(),
                    MontoTotal = g.Sum(f => f.Total)
                })
                .ToList();

            return new ConsultaContribuyente
            {
                Nit = nit,
                Nombre = NombreDe(nit),
                Tipo = emitidas ? "emitidas" : "recibidas",
                Detalle = detalle,
                TotalFacturado = detalle.Sum(l => l.MontoTotal)
            };
        }
    }

    public ResumenIva ResumenDeIva(DateTime fecha)
    {
        lock (_almacen.Candado)
        {
            var delDia = _almacen.Facturas.Where(f => f.Aprobada && f.Fecha == fecha.Date).ToList();

          
            var emitido = new Dictionary<string, decimal>();
            var recibido = new Dictionary<string, decimal>();
            foreach (RegistroFactura f in delDia)
            {
           
                emitido[f.NitEmisor] = emitido.GetValueOrDefault(f.NitEmisor) + f.Iva;
                recibido[f.NitReceptor] = recibido.GetValueOrDefault(f.NitReceptor) + f.Iva;
            }

            return new ResumenIva
            {
                Fecha = Fechas.Texto(fecha),
                // Union junta los NITs que emitieron con los que recibieron, sin repetir ninguno.
                Nits = emitido.Keys.Union(recibido.Keys)
                    .OrderBy(n => n, StringComparer.Ordinal)
                    .Select(n => new LineaIva
                    {
                        Nit = n,
                        Nombre = NombreDe(n),
                        IvaEmitido = emitido.GetValueOrDefault(n),
                        IvaRecibido = recibido.GetValueOrDefault(n)
                    })
                    .ToList()
            };
        }
    }


    public ResumenRango ResumenPorRango(DateTime? inicio, DateTime? fin, bool sinIva)
    {
        lock (_almacen.Candado)
        {
            var detalle = _almacen.Facturas
                .Where(f => f.Aprobada && EnRango(f.Fecha, inicio, fin))
                .GroupBy(f => f.Fecha)
                .OrderBy(g => g.Key)
                .Select(g => new LineaRango
                {
                    Fecha = Fechas.Texto(g.Key),
                    Valor = g.Sum(f => sinIva ? f.Valor : f.Total)
                })
                .ToList();

            return new ResumenRango
            {
                Modo = sinIva ? "sinIva" : "total",
                Detalle = detalle,
                TotalGeneral = detalle.Sum(l => l.Valor)
            };
        }
    }


    public void Inicializar()
    {
        lock (_almacen.Candado)
            _almacen.Reiniciar();
    }

    // Cuántos datos hay ahorita. Me sirve para comprobar que la API está viva y guardando.
    public object Estado()
    {
        lock (_almacen.Candado)
            return new
            {
                contribuyentes = _almacen.Rtu.Count,
                facturasRecibidas = _almacen.Facturas.Count,
                facturasAprobadas = _almacen.Facturas.Count(f => f.Aprobada)
            };
    }
}
