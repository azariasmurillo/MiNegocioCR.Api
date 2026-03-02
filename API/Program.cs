using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.API.Filters;
using MiNegocioCR.Api.Aplication.Interfaces;
using MiNegocioCR.Api.Aplication.Interfaces.Business;
using MiNegocioCR.Api.Aplication.Interfaces.ReapirOrders;
using MiNegocioCR.Api.Aplication.Interfaces.Whatsapp;
using MiNegocioCR.Api.Aplication.Services;
using MiNegocioCR.Api.Aplication.UseCases.Business;
using MiNegocioCR.Api.Aplication.UseCases.RepairOrder;
using MiNegocioCR.Api.Aplication.UseCases.Whatsapp;
using MiNegocioCR.Api.Infrastructure.Persistence;
using MiNegocioCR.Api.Infrastructure.Security;
using MiNegocioCR.Api.Infrastructure.Services;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

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
app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
