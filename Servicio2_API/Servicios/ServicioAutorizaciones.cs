using Servicio2_API.Models;
using Servicio2_API.Utilidades;

namespace Servicio2_API.Servicios;

public class ServicioAutorizaciones
{
    
    private const bool CompararTotalContraIvaRecibido = true;

    private readonly AlmacenDatos _almacen;
    private readonly GeneradorAutorizacion _generador;
    private readonly ILogger<ServicioAutorizaciones> _log;

    public ServicioAutorizaciones(AlmacenDatos almacen, GeneradorAutorizacion generador,
                                  ILogger<ServicioAutorizaciones> log)
    {
        _almacen = almacen;
        _generador = generador;
        _log = log;
    }

  
    public RespuestaAutorizaciones Procesar(List<FacturaEntrada?> facturas, DateTime hoy)
    {
        var resultado = new ResultadoAutorizaciones();

        lock (_almacen.Candado)   
        {
            foreach (FacturaEntrada? factura in facturas)
            {
                if (factura == null) continue;

            
                DateTime? fecha = ExtractorFecha.Extraer(factura.Tiempo);
                if (fecha == null)
                {
                    _log.LogWarning("Factura '{Ref}' ignorada: no se pudo leer una fecha valida en '{Tiempo}'",
                                    factura.Referencia, factura.Tiempo);
                    continue;
                }

                
                RegistroFactura registro = Evaluar(factura, fecha.Value.Date);

           
                if (!registro.TieneErrores)
                {
                    registro.Aprobada = true;
                    registro.CodigoAprobacion = _generador.Siguiente(hoy);

                    resultado.Aprobadas++;
                    resultado.Detalle.Add(new DetalleAprobacion
                    {
                        Referencia = registro.Referencia,
                        CodigoAprobacion = registro.CodigoAprobacion.Value
                    });
                }

                // La respuesta del enunciado pide contar aparte las rechazadas por NIT inexistente.
                if (registro.NitEmisorInexistente) resultado.RechazadasPorEmisorInexistente++;
                if (registro.NitReceptorInexistente) resultado.RechazadasPorReceptorInexistente++;

                _almacen.Facturas.Add(registro);
            }

            _almacen.GuardarFacturas();   // guardo en archivos una sola vez, al terminar todas
        }
        return new RespuestaAutorizaciones { Resultado = resultado };
    }


    private RegistroFactura Evaluar(FacturaEntrada f, DateTime fecha)
    {
        var registro = new RegistroFactura
        {
            Fecha = fecha,
            Referencia = (f.Referencia ?? "").Trim(),
            NitEmisor = ValidadorNit.Normalizar(f.NitEmisor),
            NitReceptor = ValidadorNit.Normalizar(f.NitReceptor),
            Valor = f.Valor,
            Iva = f.Iva,
            Total = f.Total
        };

        registro.ReferenciaDuplicada = !_almacen.Referencias.Add(registro.Referencia);

        if (!ValidadorNit.EsValido(registro.NitEmisor)) registro.NitEmisorInvalido = true;
        else if (!_almacen.Rtu.ContainsKey(registro.NitEmisor)) registro.NitEmisorInexistente = true;

  
        if (!ValidadorNit.EsValido(registro.NitReceptor)) registro.NitReceptorInvalido = true;
        else if (!_almacen.Rtu.ContainsKey(registro.NitReceptor)) registro.NitReceptorInexistente = true;

   
        decimal ivaCorrecto = Calculadora.Iva(f.Valor);
        registro.IvaMalCalculado = f.Iva != ivaCorrecto;

        decimal ivaParaTotal = CompararTotalContraIvaRecibido ? f.Iva : ivaCorrecto;
        registro.TotalMalCalculado = f.Total != f.Valor + ivaParaTotal;

        return registro;
    }
}
