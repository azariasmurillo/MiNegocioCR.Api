using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.API.Filters;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.Auth;
using MiNegocioCR.Api.Application.Interfaces.Business;
using MiNegocioCR.Api.Application.Interfaces.MiNegocioCR.Api.Application.Interfaces.UseCases.Sales;
using MiNegocioCR.Api.Application.Interfaces.RepairOrders;
using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Application.Interfaces.Services;
using MiNegocioCR.Api.Application.Interfaces.Whatsapp;
using MiNegocioCR.Api.Application.UseCases.Business;
using MiNegocioCR.Api.Application.UseCases.RepairOrder;
using MiNegocioCR.Api.Application.UseCases.Sales;
using MiNegocioCR.Api.Application.UseCases.Whatsapp;
using MiNegocioCR.Api.Infrastructure.Auth;
using MiNegocioCR.Api.Infrastructure.Persistence;
using MiNegocioCR.Api.Infrastructure.Persistence.Repositories;
using MiNegocioCR.Api.Infrastructure.Security;
using MiNegocioCR.Api.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

var firebasePath = Path.Combine(
    builder.Environment.ContentRootPath,
    "Infrastructure/Auth/firebase-adminsdk.json");

FirebaseApp.Create(new AppOptions()
{
    Credential = GoogleCredential.FromFile(firebasePath)
});

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddHttpClient();
var keysPath = Path.Combine(Directory.GetCurrentDirectory(), "DataProtection-Keys");
Directory.CreateDirectory(keysPath);
builder.Services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(keysPath)).SetApplicationName("MiNegocioCR");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<ICreateRepairOrderUseCase, CreateRepairOrderUseCase>();
builder.Services.AddScoped<IUpdateRepairOrderStatusUseCase, UpdateRepairOrderStatusUseCase>();
builder.Services.AddScoped<IGetRepairOrdersByBusinessUseCase, GetRepairOrdersByBusinessUseCase>();
builder.Services.AddScoped<IGetRepairOrderByIdUseCase, GetRepairOrderByIdUseCase>();
builder.Services.AddScoped<IUpdateRepairOrderUseCase, UpdateRepairOrderUseCase>();
builder.Services.AddScoped<IGetRepairOrderByBusinessIdAndStatusUseCase, GetRepairOrderByBusinessIdAndStatusUseCase>();
builder.Services.AddScoped<ICreateBusinessUseCase, CreateBusinessUseCase>();
builder.Services.AddScoped<IConfigureSmtpUseCase, ConfigureSmtpUseCase>();
builder.Services.AddScoped<ISetBusinessActiveStatusUseCase, SetBusinessActiveStatusUseCase>();
builder.Services.AddScoped<IGetBusinessByIdUseCase, GetBusinessByIdUseCase>();
builder.Services.AddScoped<IAppDbContext, AppDbContext>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IWhatsappApplicationService, WhatsappApplicationService>();
builder.Services.AddScoped<IEncryptionService, EncryptionService>();
builder.Services.AddScoped<IWhatsappWebhookService, WhatsappWebhookService>();
builder.Services.AddScoped<IWhatsappMessageRepository, WhatsappMessageRepository>();
builder.Services.AddScoped<IWhatsappMessageService, WhatsappMessageService>();
builder.Services.AddScoped<IBusinessRepository, BusinessRepository>();
builder.Services.AddScoped<IWhatsappWebhookLogRepository, WhatsappWebhookLogRepository>();
builder.Services.AddScoped<ICatalogRepository, CatalogRepository>();
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
builder.Services.AddScoped<IVariantRepository, VariantRepository>();
builder.Services.AddScoped<ISupplierRepository, SupplierRepository>();
builder.Services.AddScoped<IPurchaseRepository, PurchaseRepository>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IRegisterSaleUseCase, RegisterSaleUseCase>();
builder.Services.AddScoped<ILowStockAlertService, LowStockAlertService>();
builder.Services.AddScoped<ISaleRepository, SaleRepository>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IFirebaseAuthService, FirebaseAuthService>();

builder.Services.AddHttpClient<IWhatsappService, WhatsappService>();

builder.Services.AddControllers(options => {options.Filters.Add<DomainExceptionFilter>();});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

app.UseForwardedHeaders();

app.UseRouting(); // 👈 AGREGA ESTO
app.UseMiddleware<FirebaseAuthMiddleware>();
// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
app.UseSwaggerUI();
//}

//app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.MapGet("/", () => Results.Ok(new
{
    message = "MiNegocioCR API",
    status = "running",
    docs = "/swagger",
    health = "/health"
}));

app.MapControllers();
Console.WriteLine("🔥🔥🔥 VERSION 100% NUEVA ACTIVA 🔥🔥🔥");

app.MapGet("/privacy", () =>
{
    var html = """
    <!DOCTYPE html>
    <html lang="es">
    <head>
        <meta charset="UTF-8">
        <title>Política de Privacidad - Mi-NegocioCR</title>
        <style>
            body {
                font-family: Arial, sans-serif;
                max-width: 900px;
                margin: 40px auto;
                padding: 20px;
                line-height: 1.6;
                color: #333;
            }
            h1, h2 {
                color: #1f2937;
            }
        </style>
    </head>
    <body>
        <h1>Política de Privacidad</h1>
        <p><strong>Última actualización:</strong> Marzo 2026</p>

        <h2>1. Información General</h2>
        <p>
            Mi-NegocioCR es una plataforma SaaS que permite a negocios gestionar la comunicación con sus clientes 
            mediante la API oficial de WhatsApp Business proporcionada por Meta.
        </p>

        <h2>2. Información que recopilamos</h2>
        <ul>
            <li>Números de teléfono de clientes</li>
            <li>Mensajes enviados y recibidos a través de WhatsApp</li>
            <li>Información básica del negocio registrada por el usuario</li>
        </ul>

        <h2>3. Uso de la Información</h2>
        <p>
            La información se utiliza exclusivamente para:
        </p>
        <ul>
            <li>Enviar y recibir mensajes mediante WhatsApp Business</li>
            <li>Gestión de órdenes, servicios o soporte</li>
            <li>Mejorar el funcionamiento de la plataforma</li>
        </ul>

        <h2>4. Protección de Datos</h2>
        <p>
            Aplicamos medidas técnicas y organizativas razonables para proteger la información contra 
            acceso no autorizado, pérdida o alteración.
        </p>

        <h2>5. Compartición de Información</h2>
        <p>
            Mi-NegocioCR no vende ni comparte datos personales con terceros, excepto cuando sea requerido 
            por ley o necesario para el funcionamiento de la API oficial de WhatsApp Business.
        </p>

        <h2>6. Contacto</h2>
        <p>
            Para consultas relacionadas con esta política:
            <br>
            <strong>Email:</strong> soporte@mi-negociocr.com
        </p>

        <p>© 2026 Mi-NegocioCR</p>
    </body>
    </html>
    """;

    return Results.Content(html, "text/html");
});

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
