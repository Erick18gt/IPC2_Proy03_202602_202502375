using System.Text.Json.Serialization;
using Servicio2_API.Servicios;
using Servicio2_API.Utilidades;

var builder = WebApplication.CreateBuilder(args);

// Aquí le digo a la API que use controladores (mis clases de la carpeta Controllers).
builder.Services
    .AddControllers(opciones =>
    {
        // Sin esto, si a UNA sola factura le falta un campo, la API rechaza el archivo completo.
        // Yo prefiero que lea todo lo que pueda y que revise cada factura por su cuenta.
        opciones.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
    })
    .AddJsonOptions(opciones =>
    {
        // Esto es para que no se caiga si en el JSON viene un NIT como número (7378106) en vez de texto ("7378106").
        opciones.JsonSerializerOptions.Converters.Add(new ConversorTextoFlexible());
        // Y esto es por si un valor viene entre comillas, como "100.00".
        opciones.JsonSerializerOptions.NumberHandling = JsonNumberHandling.AllowReadingFromString;
    });

// Swagger: la página con la que pruebo mi API sin necesidad del frontend.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Aquí registro las clases que hacen el trabajo.
// "Singleton" significa que se crea UNA sola vez y todas las peticiones usan la misma.
// Es importante porque así todas comparten el mismo RTU y las mismas facturas.
// Los datos se guardan en archivos dentro de la carpeta "Data" del proyecto.
builder.Services.AddSingleton(new AlmacenDatos(Path.Combine(builder.Environment.ContentRootPath, "Data")));
builder.Services.AddSingleton<GeneradorAutorizacion>();
builder.Services.AddSingleton<ServicioRtu>();
builder.Services.AddSingleton<ServicioAutorizaciones>();
builder.Services.AddSingleton<ServicioConsultas>();

var app = builder.Build();

// Swagger solo aparece cuando corro el proyecto desde Visual Studio.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();
app.Run();
