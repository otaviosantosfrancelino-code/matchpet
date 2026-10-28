using crudcomdb.Data;
using crudcomdb.Interfaces;
using crudcomdb.Repositories;
using crudcomdb.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. BANCO DE DADOS
// O Neon "dorme" quando fica parado e derruba conexões antigas: a nova tentativa automática evita erros aleatórios
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsql => npgsql.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(3), errorCodesToAdd: null)));

// 2. INJEÇÃO DE DEPENDÊNCIA
builder.Services.AddScoped<IPetRepository, PetRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IInteresseRepository, InteresseRepository>();
builder.Services.AddScoped<IItemRepository, ItemRepository>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ToastService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<CnpjService>();
builder.Services.AddScoped<DocumentoValidacaoService>();
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();

// 3. INTERATIVIDADE E LIMITES DE UPLOAD
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddHubOptions(options =>
    {
        options.MaximumReceiveMessageSize = 10 * 1024 * 1024; // 10MB para fotos
    });

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 4. BANCO: aplica migrações pendentes e cria os cadastros de demonstração (uma única vez)
using (var escopo = app.Services.CreateScope())
{
    var db = escopo.ServiceProvider.GetRequiredService<AppDbContext>();
    var log = escopo.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        await db.Database.MigrateAsync();
        if (app.Configuration.GetValue("SeedDemo", true))
            await DadosDemonstracao.CriarAsync(db, log);
    }
    catch (Exception ex)
    {
        log.LogError(ex, "Falha ao preparar o banco de dados na inicialização");
    }
}

// 5. PIPELINE
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

// Endereço inexistente no site mostra a página amigável "/nao-encontrado" (a API continua devolvendo 404 normal)
app.UseWhen(ctx => !ctx.Request.Path.StartsWithSegments("/api"),
    site => site.UseStatusCodePagesWithReExecute("/nao-encontrado"));

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting(); // depois do UseStatusCodePages, para a página 404 conseguir ser encontrada
app.UseAntiforgery();

app.MapControllers();
app.MapRazorComponents<crudcomdb.Components.App>()
    .AddInteractiveServerRenderMode();

app.Run();