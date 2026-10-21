using Programa1_Frontend.Servicios;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

// Cliente para hablar con la API. La dirección de la API se cambia en appsettings.json ("ApiUrl").
builder.Services.AddHttpClient<ClienteApi>(cliente =>
{
    string url = builder.Configuration["ApiUrl"] ?? "http://localhost:5223/";
    cliente.BaseAddress = new Uri(url.EndsWith("/") ? url : url + "/");
    cliente.Timeout = TimeSpan.FromMinutes(5);   // por si mandan un archivo muy grande
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();
app.Run();
