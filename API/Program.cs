using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MiNegocioCR.Api.API.Content;
using MiNegocioCR.Api.API.Filters;
using MiNegocioCR.Api.API.Hubs;
using MiNegocioCR.Api.API.Services;
using MiNegocioCR.Api.Application.AI.Cache;
using MiNegocioCR.Api.Application.AI.Guardrails;
using MiNegocioCR.Api.Application.AI.Intent;
using MiNegocioCR.Api.Application.AI.Interfaces;
using MiNegocioCR.Api.Application.AI.Limits;
using MiNegocioCR.Api.Application.AI.Memory;
using MiNegocioCR.Api.Application.AI.Prompts;
using MiNegocioCR.Api.Application.AI.Routing;
using MiNegocioCR.Api.Application.AI.Sales;
using MiNegocioCR.Api.Application.AI.Services;
using MiNegocioCR.Api.Application.AI.State;
using MiNegocioCR.Api.Application.AI.Tools;
using MiNegocioCR.Api.Application.AI.Upsell;
using MiNegocioCR.Api.Application.Common;
using MiNegocioCR.Api.Application.Configuration;
using MiNegocioCR.Api.Application.Handler;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.ArchiveConversation;
using MiNegocioCR.Api.Application.Interfaces.Auth;
using MiNegocioCR.Api.Application.Interfaces.Business;
using MiNegocioCR.Api.Application.Interfaces.Contacts;
using MiNegocioCR.Api.Application.Interfaces.ConversationTag;
using MiNegocioCR.Api.Application.Interfaces.RepairOrders;
using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Application.Interfaces.Services;
using MiNegocioCR.Api.Application.Interfaces.UseCases.Auth;
using MiNegocioCR.Api.Application.Interfaces.UseCases.Dashboard;
using MiNegocioCR.Api.Application.Interfaces.UseCases.Payments;
using MiNegocioCR.Api.Application.Interfaces.UseCases.Sales;
using MiNegocioCR.Api.Application.Interfaces.Variants;
using MiNegocioCR.Api.Application.Interfaces.Whatsapp;
using MiNegocioCR.Api.Application.UseCases.ArchiveConversationUseCase;
using MiNegocioCR.Api.Application.UseCases.Auth;
using MiNegocioCR.Api.Application.UseCases.Business;
using MiNegocioCR.Api.Application.UseCases.Catalog;
using MiNegocioCR.Api.Application.UseCases.Contacts;
using MiNegocioCR.Api.Application.UseCases.Conversations;
using MiNegocioCR.Api.Application.UseCases.Dashboard;
using MiNegocioCR.Api.Application.UseCases.Payments;
using MiNegocioCR.Api.Application.UseCases.RepairOrder;
using MiNegocioCR.Api.Application.UseCases.Repository;
using MiNegocioCR.Api.Application.UseCases.Sales;
using MiNegocioCR.Api.Application.UseCases.Variants;
using MiNegocioCR.Api.Application.UseCases.Whatsapp;
using MiNegocioCR.Api.Infrastructure.AI;
using MiNegocioCR.Api.Infrastructure.Persistence;
using MiNegocioCR.Api.Infrastructure.Persistence.Repositories;
using MiNegocioCR.Api.Infrastructure.Security;
using MiNegocioCR.Api.Infrastructure.Services;
using Resend;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

// Orden de resolución:
//   1. Variable de entorno POSTGRES_CONNECTION_STRING (producción / Railway)
//   2. ConnectionStrings:DefaultConnection en appsettings*.json (desarrollo local)
var postgresConnectionString =
    builder.Configuration["POSTGRES_CONNECTION_STRING"]
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(postgresConnectionString))
    throw new InvalidOperationException(
        "No se encontró la cadena de conexión a PostgreSQL. " +
        "Configurá la variable de entorno POSTGRES_CONNECTION_STRING " +
        "o ConnectionStrings:DefaultConnection en appsettings.Development.json.");

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.Configure<AppOptions>(builder.Configuration.GetSection(AppOptions.SectionName));
builder.Services.Configure<ResendSettings>(builder.Configuration.GetSection(ResendSettings.SectionName));

var jwtSection = builder.Configuration.GetSection(JwtOptions.SectionName);
var jwtOptions = jwtSection.Get<JwtOptions>() ?? new JwtOptions();
var jwtSigningKey = jwtOptions.ResolveSigningKey();
if (string.IsNullOrWhiteSpace(jwtSigningKey))
    throw new InvalidOperationException("Configurá Jwt:Key en appsettings, variables de entorno o User Secrets.");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSigningKey))
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddSignalR();

// --- Core ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT en header Authorization: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});
builder.Services.AddControllers(options => options.Filters.Add<DomainExceptionFilter>())
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    });
builder.Services.AddHttpClient();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IResend>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var apiKey = configuration["Resend:ApiKey"] ?? configuration["RESEND_API_KEY"] ?? string.Empty;
    var client = sp.GetRequiredService<IHttpClientFactory>().CreateClient();
    return ResendClient.Create(new ResendClientOptions
    {
        ApiToken = apiKey
    }, client);
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:4200",
                "https://localhost:7176/",
                "https://mi-negociocr-frontend.vercel.app",
                "https://mi-negociocr.com",
                "https://www.mi-negociocr.com")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var keysPath = Path.Combine(Directory.GetCurrentDirectory(), "DataProtection-Keys");
Directory.CreateDirectory(keysPath);
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(keysPath))
    .SetApplicationName("MiNegocioCR");

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// --- Db ---
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(postgresConnectionString)
           .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
);
builder.Services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

// --- Repositories ---
builder.Services.AddScoped<IBusinessRepository, BusinessRepository>();
builder.Services.AddScoped<IWhatsappWebhookLogRepository, WhatsappWebhookLogRepository>();
builder.Services.AddScoped<IWhatsappMessageRepository, WhatsappMessageRepository>();
builder.Services.AddScoped<ICatalogRepository, CatalogRepository>();
builder.Services.AddScoped<ICatalogCategoryRepository, CatalogCategoryRepository>();
builder.Services.AddScoped<ICatalogOptionRepository, CatalogOptionRepository>();
builder.Services.AddScoped<ICatalogOptionValueRepository, CatalogOptionValueRepository>();
builder.Services.AddScoped<ICatalogVariantOptionValueRepository, CatalogVariantOptionValueRepository>();
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
builder.Services.AddScoped<IVariantRepository, VariantRepository>();
builder.Services.AddScoped<ISupplierRepository, SupplierRepository>();
builder.Services.AddScoped<IPurchaseRepository, PurchaseRepository>();
builder.Services.AddScoped<ISaleRepository, SaleRepository>();
builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// --- Business & WhatsApp ---
builder.Services.AddScoped<ICreateBusinessUseCase, CreateBusinessUseCase>();
builder.Services.AddScoped<IConfigureSmtpUseCase, ConfigureSmtpUseCase>();
builder.Services.AddScoped<ISetBusinessActiveStatusUseCase, SetBusinessActiveStatusUseCase>();
builder.Services.AddScoped<IGetBusinessByIdUseCase, GetBusinessByIdUseCase>();
builder.Services.AddScoped<IGetBusinessConfigUseCase, GetBusinessConfigUseCase>();
builder.Services.AddScoped<IUpdateBusinessConfigUseCase, UpdateBusinessConfigUseCase>();
builder.Services.AddScoped<IUploadBusinessLogoUseCase, UploadBusinessLogoUseCase>();
builder.Services.AddScoped<IBusinessLogoStorageService, SupabaseBusinessLogoStorageService>();
builder.Services.AddScoped<IEmailService, ResendEmailService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IWhatsappApplicationService, WhatsappApplicationService>();
builder.Services.AddScoped<IEncryptionService, EncryptionService>();
builder.Services.AddScoped<IWhatsappWebhookService, WhatsappWebhookService>();
builder.Services.AddScoped<IWhatsappMessageService, WhatsappMessageService>();
builder.Services.AddScoped<IWhatsAppTokenService, WhatsAppTokenService>();
builder.Services.AddScoped<IQuickReplyService, QuickReplyService>();
builder.Services.AddHttpClient<IWhatsappService, WhatsappService>();
builder.Services.AddScoped<IConversationService, ConversationService>();
builder.Services.AddScoped<IGetUnreadTotalUseCase, GetUnreadTotalUseCase>();
builder.Services.AddScoped<IMarkConversationReadHandler, MarkConversationReadHandler>();
builder.Services.AddScoped<ICreateConversationHandler, CreateConversationHandler>();
builder.Services.AddScoped<IUpdateConversationStatusHandler, UpdateConversationStatusHandler>();
builder.Services.AddScoped<ISendTemplateHandler, SendTemplateHandler>();
builder.Services.AddScoped<IArchiveConversationUseCase, ArchiveConversationUseCase>();

// --- Conversation Tag ---
builder.Services.AddScoped<IConversationTag, MiNegocioCR.Api.Application.ConversationTag.ConversationTag>();

// --- Contacts ---
builder.Services.AddScoped<IContact, MiNegocioCR.Api.Application.Contact.Contacts>();
builder.Services.AddScoped<IListContactsUseCase, ListContactsUseCase>();
builder.Services.AddScoped<ISearchContactsUseCase, SearchContactsUseCase>();
builder.Services.AddScoped<IUpdateContactUseCase, UpdateContactUseCase>();
builder.Services.AddScoped<ISoftDeleteContactUseCase, SoftDeleteContactUseCase>();
builder.Services.AddScoped<IHardDeleteContactUseCase, HardDeleteContactUseCase>();

// --- Repair orders ---
builder.Services.AddScoped<ICreateRepairOrderUseCase, CreateRepairOrderUseCase>();
builder.Services.AddScoped<IUpdateRepairOrderStatusUseCase, UpdateRepairOrderStatusUseCase>();
builder.Services.AddScoped<IGetRepairOrdersByBusinessUseCase, GetRepairOrdersByBusinessUseCase>();
builder.Services.AddScoped<IGetRepairOrderByIdUseCase, GetRepairOrderByIdUseCase>();
builder.Services.AddScoped<IUpdateRepairOrderUseCase, UpdateRepairOrderUseCase>();
builder.Services.AddScoped<IGetRepairOrderByBusinessIdAndStatusUseCase, GetRepairOrderByBusinessIdAndStatusUseCase>();
builder.Services.AddScoped<ISearchRepairOrdersUseCase, SearchRepairOrdersUseCase>();
builder.Services.AddScoped<ISendRepairOrderEmailUseCase, SendRepairOrderEmailUseCase>();
builder.Services.AddScoped<IChargeRepairOrderUseCase, ChargeRepairOrderUseCase>();
builder.Services.AddScoped<IGetRepairOrderBalanceUseCase, GetRepairOrderBalanceUseCase>();
builder.Services.AddScoped<IRepairOrderImageStorageService, SupabaseRepairOrderImageStorageService>();
builder.Services.AddScoped<IUploadRepairOrderImagesUseCase, UploadRepairOrderImagesUseCase>();
builder.Services.AddScoped<IGetRepairOrderImagesUseCase, GetRepairOrderImagesUseCase>();
builder.Services.AddScoped<IDeleteRepairOrderImageUseCase, DeleteRepairOrderImageUseCase>();

// --- Inventory & sales ---
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<ILowStockAlertService, LowStockAlertService>();
builder.Services.AddScoped<IRegisterSaleUseCase, MiNegocioCR.Api.Application.UseCases.Sales.RegisterSaleUseCase>();
builder.Services.AddScoped<ICreateSaleFromRepairUseCase, CreateSaleFromRepairUseCase>();
builder.Services.AddScoped<ISendSaleEmailUseCase, SendSaleEmailUseCase>();
builder.Services.AddScoped<IGetSalesByBusinessUseCase, GetSalesByBusinessUseCase>();
builder.Services.AddScoped<IGetSaleByIdUseCase, GetSaleByIdUseCase>();
builder.Services.AddScoped<IGetDashboardSummaryUseCase, GetDashboardSummaryUseCase>();
builder.Services.AddScoped<IGetSalesTrendUseCase, GetSalesTrendUseCase>();
builder.Services.AddScoped<IGetTicketAverageUseCase, GetTicketAverageUseCase>();
builder.Services.AddScoped<IGetRecentActivityUseCase, GetRecentActivityUseCase>();
builder.Services.AddScoped<IGetTopProductsUseCase, GetTopProductsUseCase>();
builder.Services.AddScoped<IGetPendingOrdersDashboardUseCase, GetPendingOrdersDashboardUseCase>();
builder.Services.AddScoped<IGetProfitBySourceUseCase, GetProfitBySourceUseCase>();
builder.Services.AddScoped<ICreatePaymentUseCase, CreatePaymentUseCase>();
builder.Services.AddScoped<IGetPaymentsByRepairOrderUseCase, GetPaymentsByRepairOrderUseCase>();
builder.Services.AddScoped<ICreateCatalogItemUseCase, CreateCatalogItemUseCase>();
builder.Services.AddScoped<IUpdateCatalogItemUseCase, UpdateCatalogItemUseCase>();
builder.Services.AddScoped<IToggleCatalogItemStatusUseCase, ToggleCatalogItemStatusUseCase>();
builder.Services.AddScoped<IDeleteCatalogItemUseCase, DeleteCatalogItemUseCase>();
builder.Services.AddScoped<IGetCatalogItemsByBusinessUseCase, GetCatalogItemsByBusinessUseCase>();
builder.Services.AddScoped<IVariantImageStorageService, SupabaseVariantImageStorageService>();
builder.Services.AddScoped<IUploadCatalogVariantImagesUseCase, UploadCatalogVariantImagesUseCase>();
builder.Services.AddScoped<IGetCatalogVariantImagesUseCase, GetCatalogVariantImagesUseCase>();
builder.Services.AddScoped<IDeleteCatalogVariantImageUseCase, DeleteCatalogVariantImageUseCase>();
builder.Services.AddScoped<ISetPrimaryCatalogVariantImageUseCase, SetPrimaryCatalogVariantImageUseCase>();
builder.Services.AddScoped<ICreateCategoryUseCase, CreateCategoryUseCase>();
builder.Services.AddScoped<IGetCategoriesByBusinessUseCase, GetCategoriesByBusinessUseCase>();
builder.Services.AddScoped<IUpdateCategoryUseCase, UpdateCategoryUseCase>();
builder.Services.AddScoped<IToggleCategoryStatusUseCase, ToggleCategoryStatusUseCase>();
builder.Services.AddScoped<IDeleteCategoryUseCase, DeleteCategoryUseCase>();
builder.Services.AddScoped<ICreateOptionUseCase, CreateOptionUseCase>();
builder.Services.AddScoped<IGetOptionsByItemUseCase, GetOptionsByItemUseCase>();
builder.Services.AddScoped<IUpdateOptionUseCase, UpdateOptionUseCase>();
builder.Services.AddScoped<IToggleOptionStatusUseCase, ToggleOptionStatusUseCase>();
builder.Services.AddScoped<IDeleteOptionUseCase, DeleteOptionUseCase>();
builder.Services.AddScoped<ICreateOptionValueUseCase, CreateOptionValueUseCase>();
builder.Services.AddScoped<IGetValuesByOptionUseCase, GetValuesByOptionUseCase>();
builder.Services.AddScoped<IUpdateOptionValueUseCase, UpdateOptionValueUseCase>();
builder.Services.AddScoped<IToggleOptionValueStatusUseCase, ToggleOptionValueStatusUseCase>();
builder.Services.AddScoped<IDeleteOptionValueUseCase, DeleteOptionValueUseCase>();
builder.Services.AddScoped<ICreateVariantUseCase, CreateVariantUseCase>();
builder.Services.AddScoped<IGetVariantsByCatalogItemUseCase, GetVariantsByCatalogItemUseCase>();
builder.Services.AddScoped<IGetVariantsByBusinessUseCase, GetVariantsByBusinessUseCase>();
builder.Services.AddScoped<IUpdateVariantUseCase, UpdateVariantUseCase>();
builder.Services.AddScoped<IDeleteVariantUseCase, DeleteVariantUseCase>();
builder.Services.AddScoped<IRegisterPurchaseUseCase, RegisterPurchaseUseCase>();
builder.Services.AddScoped<IAdjustInventoryUseCase, AdjustInventoryUseCase>();

// --- Auth ---
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
builder.Services.AddScoped<IRequestPasswordResetUseCase, RequestPasswordResetUseCase>();
builder.Services.AddScoped<IValidateResetTokenUseCase, ValidateResetTokenUseCase>();
builder.Services.AddScoped<IResetPasswordUseCase, ResetPasswordUseCase>();
builder.Services.AddScoped<IAdminAuthService, AdminAuthService>();

// --- AI ---
builder.Services.AddHttpClient<IAIClient, OpenAIClient>();
builder.Services.AddScoped<IAIService, AIService>();
builder.Services.AddScoped<IAIChatRequestValidator, AIChatRequestValidator>();
builder.Services.AddScoped<ISalesConversationHandler, SalesConversationHandler>();
builder.Services.AddScoped<IToolSelector, ToolSelector>();
builder.Services.AddScoped<ISaleService, SaleService>();
builder.Services.AddScoped<IUpsellService, UpsellService>();
builder.Services.AddScoped<IAITool, InventoryTool>();
builder.Services.AddScoped<IAITool, RepairOrderTool>();
builder.Services.AddScoped<IAITool, RepairServiceTool>();
builder.Services.AddScoped<IAITool, SalesTool>();
builder.Services.AddScoped<IConversationMemoryService, ConversationMemoryService>();
builder.Services.AddScoped<IModelRouter, ModelRouter>();
builder.Services.AddScoped<IResponseCache, ResponseCache>();
builder.Services.AddScoped<ITokenLimiter, TokenLimiter>();
builder.Services.AddScoped<IConversationStateService, ConversationStateService>();
builder.Services.AddScoped<ISetEnableAIChatUseCase, SetEnableAIChatUseCase>();
builder.Services.AddScoped<IAITokenBudgetService, AITokenBudgetService>();
builder.Services.AddSingleton<IPromptBuilder, SalesPromptBuilder>();
builder.Services.AddSingleton<IDomainFilter, DomainFilter>();
builder.Services.AddSingleton<IIntentClassifier, IntentClassifier>();

// --- Build ---
var app = builder.Build();

// Migraciones: ejecutar manualmente con `dotnet ef database update` apuntando a la
// conexión directa de Supabase (puerto 5432, no el pooler 6543).
// Variable de entorno: POSTGRES_CONNECTION_STRING

app.UseForwardedHeaders();

// IMPORTANTE: el exception handler debe correr ANTES de UseCors para que,
// cuando ocurre un 500, la respuesta de error también incluya los headers CORS.
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var origin = context.Request.Headers.Origin.ToString();
        var allowedOrigins = new[]
        {
            "http://localhost:4200",
            "https://localhost:7176",
            "https://mi-negociocr-frontend.vercel.app",
            "https://mi-negociocr.com",
            "https://www.mi-negociocr.com"
        };
        if (!string.IsNullOrEmpty(origin) && allowedOrigins.Contains(origin))
            context.Response.Headers.Append("Access-Control-Allow-Origin", origin);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode  = 500;

        var feature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        var message = feature?.Error?.Message ?? "Internal server error";

        // No exponer detalles en producción
        var isDev = app.Environment.IsDevelopment();
        var body  = isDev
            ? System.Text.Json.JsonSerializer.Serialize(new { error = message, detail = feature?.Error?.ToString() })
            : System.Text.Json.JsonSerializer.Serialize(new { error = "Error interno del servidor." });

        await context.Response.WriteAsync(body);
    });
});

app.UseRouting();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.UseSwagger();
app.UseSwaggerUI();

// --- Routes ---
app.MapGet("/", () => Results.Ok(new
{
    message = "MiNegocioCR API",
    status = "running",
    docs = "/swagger",
    health = "/health"
}));

app.MapGet("/privacy", () => Results.Content(PrivacyPageContent.Html, "text/html"));

// --- Admin (pÃ¡gina protegida) ---
app.MapGet("/admin", (HttpContext ctx) =>
{
    var auth = ctx.RequestServices.GetRequiredService<IAdminAuthService>();
    var cookie = ctx.Request.Cookies[auth.CookieName];
    if (auth.ValidateAuthCookie(cookie))
        return Results.Redirect("/admin/dashboard", false);
    return Results.Content(AdminPageContent.LoginHtml, "text/html");
});
app.MapGet("/admin/dashboard", (HttpContext ctx) =>
{
    var auth = ctx.RequestServices.GetRequiredService<IAdminAuthService>();
    var cookie = ctx.Request.Cookies[auth.CookieName];
    if (!auth.ValidateAuthCookie(cookie))
        return Results.Redirect("/admin", false);
    return Results.Content(AdminPageContent.DashboardHtml, "text/html");
});
app.MapGet("/admin/logout", (HttpContext ctx) =>
{
    var auth = ctx.RequestServices.GetRequiredService<IAdminAuthService>();
    ctx.Response.Cookies.Delete(auth.CookieName, new CookieOptions { Path = "/" });
    return Results.Redirect("/admin", false);
});

// SignalR: exponer en /chatHub y en /api/chatHub (por proxy o si el front usa baseUrl + /api)
app.MapHub<ChatHub>("/chatHub");
app.MapHub<ChatHub>("/api/chatHub");

app.MapControllers();

app.Run();




