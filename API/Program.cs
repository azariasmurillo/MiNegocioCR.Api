using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
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
using MiNegocioCR.Api.Application.Handler;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.ArchiveConversation;
using MiNegocioCR.Api.Application.Interfaces.Auth;
using MiNegocioCR.Api.Application.Interfaces.Business;
using MiNegocioCR.Api.Application.Interfaces.Contacts;
using MiNegocioCR.Api.Application.Interfaces.ConversationTag;
using MiNegocioCR.Api.Application.Interfaces.MiNegocioCR.Api.Application.Interfaces.UseCases.Sales;
using MiNegocioCR.Api.Application.Interfaces.RepairOrders;
using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Application.Interfaces.Services;
using MiNegocioCR.Api.Application.Interfaces.Whatsapp;
using MiNegocioCR.Api.Application.UseCases.ArchiveConversationUseCase;
using MiNegocioCR.Api.Application.UseCases.Business;
using MiNegocioCR.Api.Application.UseCases.Conversations;
using MiNegocioCR.Api.Application.UseCases.RepairOrder;
using MiNegocioCR.Api.Application.UseCases.Repository;
using MiNegocioCR.Api.Application.UseCases.Sales;
using MiNegocioCR.Api.Application.UseCases.Whatsapp;
using MiNegocioCR.Api.Domain.Entities;
using MiNegocioCR.Api.Infrastructure.AI;
using MiNegocioCR.Api.Infrastructure.Auth;
using MiNegocioCR.Api.Infrastructure.Persistence;
using MiNegocioCR.Api.Infrastructure.Persistence.Repositories;
using MiNegocioCR.Api.Infrastructure.Security;
using MiNegocioCR.Api.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// --- Firebase ---
var firebaseJson = Environment.GetEnvironmentVariable("FIREBASE_CREDENTIALS");
if (FirebaseApp.DefaultInstance == null)
{
    if (!string.IsNullOrEmpty(firebaseJson))
        FirebaseApp.Create(new AppOptions { Credential = GoogleCredential.FromJson(firebaseJson) });
    else
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "Infrastructure", "Auth", "firebase-adminsdk.json");
        FirebaseApp.Create(new AppOptions { Credential = GoogleCredential.FromFile(path) });
    }
}

builder.Services.AddSignalR();

// --- Core ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers(options => options.Filters.Add<DomainExceptionFilter>());
builder.Services.AddHttpClient();
builder.Services.AddMemoryCache();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:4200", "https://localhost:4200")
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
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

// --- Repositories ---
builder.Services.AddScoped<IBusinessRepository, BusinessRepository>();
builder.Services.AddScoped<IWhatsappWebhookLogRepository, WhatsappWebhookLogRepository>();
builder.Services.AddScoped<IWhatsappMessageRepository, WhatsappMessageRepository>();
builder.Services.AddScoped<ICatalogRepository, CatalogRepository>();
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
builder.Services.AddScoped<IVariantRepository, VariantRepository>();
builder.Services.AddScoped<ISupplierRepository, SupplierRepository>();
builder.Services.AddScoped<IPurchaseRepository, PurchaseRepository>();
builder.Services.AddScoped<ISaleRepository, SaleRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// --- Business & WhatsApp ---
builder.Services.AddScoped<ICreateBusinessUseCase, CreateBusinessUseCase>();
builder.Services.AddScoped<IConfigureSmtpUseCase, ConfigureSmtpUseCase>();
builder.Services.AddScoped<ISetBusinessActiveStatusUseCase, SetBusinessActiveStatusUseCase>();
builder.Services.AddScoped<IGetBusinessByIdUseCase, GetBusinessByIdUseCase>();
builder.Services.AddScoped<IEmailService, EmailService>();
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
builder.Services.AddScoped<ILinkConversationRepairOrderHandler, LinkConversationRepairOrderHandler>();
builder.Services.AddScoped<ISendTemplateHandler, SendTemplateHandler>();
builder.Services.AddScoped<IArchiveConversationUseCase, ArchiveConversationUseCase>();

// --- Conversation Tag ---
builder.Services.AddScoped<IConversationTag, MiNegocioCR.Api.Application.ConversationTag.ConversationTag>();

// --- Contacts ---
builder.Services.AddScoped<IContact, MiNegocioCR.Api.Application.Contact.Contacts>();

// --- Repair orders ---
builder.Services.AddScoped<ICreateRepairOrderUseCase, CreateRepairOrderUseCase>();
builder.Services.AddScoped<IUpdateRepairOrderStatusUseCase, UpdateRepairOrderStatusUseCase>();
builder.Services.AddScoped<IGetRepairOrdersByBusinessUseCase, GetRepairOrdersByBusinessUseCase>();
builder.Services.AddScoped<IGetRepairOrderByIdUseCase, GetRepairOrderByIdUseCase>();
builder.Services.AddScoped<IUpdateRepairOrderUseCase, UpdateRepairOrderUseCase>();
builder.Services.AddScoped<IGetRepairOrderByBusinessIdAndStatusUseCase, GetRepairOrderByBusinessIdAndStatusUseCase>();

// --- Inventory & sales ---
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<ILowStockAlertService, LowStockAlertService>();
builder.Services.AddScoped<IRegisterSaleUseCase, MiNegocioCR.Api.Application.UseCases.Sales.RegisterSaleUseCase>();
builder.Services.AddScoped<ICreateCatalogItemUseCase, CreateCatalogItemUseCase>();
builder.Services.AddScoped<ICreateVariantUseCase, CreateVariantUseCase>();
builder.Services.AddScoped<IRegisterPurchaseUseCase, RegisterPurchaseUseCase>();
builder.Services.AddScoped<IAdjustInventoryUseCase, AdjustInventoryUseCase>();

// --- Auth ---
builder.Services.AddScoped<IFirebaseAuthService, FirebaseAuthService>();
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

app.UseForwardedHeaders();
app.UseRouting();
app.UseCors();
app.UseMiddleware<FirebaseAuthMiddleware>();
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

// --- Admin (página protegida) ---
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
