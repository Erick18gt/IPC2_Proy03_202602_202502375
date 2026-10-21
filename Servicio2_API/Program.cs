using System.Text.Json.Serialization;
using Servicio2_API.Servicios;
using Servicio2_API.Utilidades;

var builder = WebApplication.CreateBuilder(args);


builder.Services
    .AddControllers(opciones =>
    {
        // Sin esto, si a una sola factura le falta un campo, la API rechaza el archivo completo.
       
        opciones.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
    })
    .AddJsonOptions(opciones =>
    {
        
        opciones.JsonSerializerOptions.Converters.Add(new ConversorTextoFlexible());
        
        opciones.JsonSerializerOptions.NumberHandling = JsonNumberHandling.AllowReadingFromString;
    });


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


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
