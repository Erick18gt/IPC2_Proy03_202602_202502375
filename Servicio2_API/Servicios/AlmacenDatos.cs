using System.Text.Json;
using Servicio2_API.Models;
using Servicio2_API.Utilidades;

namespace Servicio2_API.Servicios;


public class AlmacenDatos
{
    private static readonly JsonSerializerOptions Opciones = new() { WriteIndented = true };

    private readonly string _carpeta;

    // Si dos peticiones llegan al mismo tiempo, con este candado hago que una espere a la otra.
    // Así no se mezclan los datos ni se repite un número de autorización.
    public object Candado { get; } = new();

    // El RTU. Es un diccionario: la llave es el NIT y el valor es el contribuyente.
    // Lo elegí así porque buscar un NIT en un diccionario es directo, sin recorrer toda la lista.
    public Dictionary<string, Contribuyente> Rtu { get; private set; } = new();

    // Todas las facturas recibidas, en el orden en que llegaron.
    public List<RegistroFactura> Facturas { get; private set; } = new();

    // Las referencias que ya han llegado. Uso un HashSet porque Add() me dice si la referencia
    // ya estaba (devuelve false), y así detecto las referencias duplicadas.
    public HashSet<string> Referencias { get; private set; } = new();

    // Para el correlativo: la llave es el día ("20261022") y el valor es el último número que di ese día.
    public Dictionary<string, int> Correlativos { get; private set; } = new();

    public AlmacenDatos(string carpeta)
    {
        _carpeta = carpeta;
        Directory.CreateDirectory(_carpeta);   // si la carpeta Data no existe, la creo
        Cargar();                              // y cargo lo que haya guardado de antes
    }

    private string Ruta(string nombre) => Path.Combine(_carpeta, nombre);


    private void Cargar()
    {
        var contribuyentes = LeerArchivo<List<Contribuyente>>("rtu.json") ?? new();
        Rtu = new Dictionary<string, Contribuyente>();
        foreach (var c in contribuyentes)
            Rtu[ValidadorNit.Normalizar(c.Nit)] = c;

        Facturas = LeerArchivo<List<RegistroFactura>>("facturas.json") ?? new();
        // Las referencias no las guardo aparte: las saco de las facturas que ya tengo.
        Referencias = new HashSet<string>(Facturas.Select(f => f.Referencia));

        Correlativos = LeerArchivo<Dictionary<string, int>>("correlativos.json") ?? new();
    }

    // Lee un archivo y lo convierte a objetos. Si el archivo no existe devuelve null (empiezo vacío).
    private T? LeerArchivo<T>(string nombre) where T : class
    {
        string ruta = Ruta(nombre);
        if (!File.Exists(ruta)) return null;
        try
        {
            return JsonSerializer.Deserialize<T>(File.ReadAllText(ruta));
        }
        catch (JsonException)
        {
            // Si el archivo está dañado no lo borro: le cambio el nombre a ".corrupto" y empiezo limpio.
            File.Move(ruta, ruta + ".corrupto", overwrite: true);
            return null;
        }
    }

    private void Escribir(string nombre, object datos)
    {
        string ruta = Ruta(nombre);
        string temporal = ruta + ".tmp";
        File.WriteAllText(temporal, JsonSerializer.Serialize(datos, Opciones));
        File.Move(temporal, ruta, overwrite: true);
    }

    public void GuardarRtu() => Escribir("rtu.json", Rtu.Values.OrderBy(c => c.Nit).ToList());

    public void GuardarFacturas()
    {
        Escribir("facturas.json", Facturas);
        Escribir("correlativos.json", Correlativos);
    }

    public void Reiniciar()
    {
        Rtu = new();
        Facturas = new();
        Referencias = new();
        Correlativos = new();

        foreach (string nombre in new[] { "rtu.json", "facturas.json", "correlativos.json" })
        {
            string ruta = Ruta(nombre);
            if (File.Exists(ruta)) File.Delete(ruta);
        }
    }
}
