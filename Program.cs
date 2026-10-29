using crudcomdb.Data;
using crudcomdb.Endpoints;
using crudcomdb.Interfaces;
using crudcomdb.Repositories;
using crudcomdb.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. BANCO DE DADOS
// O Neon "dorme" quando fica parado e derruba conexões antigas: a nova tentativa automática evita erros aleatórios
// Fábrica: cada operação do chat/rede social abre o seu próprio contexto (seguro com mensagens em tempo real)
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsql => npgsql.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(3), errorCodesToAdd: null)));
// As telas antigas continuam recebendo um contexto por conexão do usuário
builder.Services.AddScoped(sp => sp.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext());

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
builder.Services.AddScoped<ChatService>();
builder.Services.AddScoped<SocialService>();
builder.Services.AddScoped<EstatisticasService>();
builder.Services.AddSingleton<NotificacoesService>();   // avisa em tempo real (nova mensagem, pedido de amizade)
builder.Services.AddSingleton<GeoService>();            // coordenadas das cidades para o mapa
builder.Services.AddHttpClient();
builder.Services.AddResponseCompression(o => o.EnableForHttps = true);

// No Render o site fica atrás de um proxy: estes cabeçalhos trazem o IP real de quem acessa
builder.Services.Configure<Microsoft.AspNetCore.Builder.ForwardedHeadersOptions>(o =>
{
    o.ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto;
    o.KnownNetworks.Clear();
    o.KnownProxies.Clear();
});
builder.Services.AddHttpContextAccessor();

// 3. INTERATIVIDADE E LIMITES DE UPLOAD
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddHubOptions(options =>
    {
        options.MaximumReceiveMessageSize = 10 * 1024 * 1024; // 10MB para fotos
    })
    .AddCircuitOptions(o => o.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(5)); // celular que troca de rede não perde a tela

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
        {
            await DadosDemonstracao.CriarAsync(db, log);
            await DadosDemonstracaoComunidade.CriarAsync(db, log);
        }
    }
    catch (Exception ex)
    {
        log.LogError(ex, "Falha ao preparar o banco de dados na inicialização");
    }
}

// 5. PIPELINE
app.UseForwardedHeaders();
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
app.UseResponseCompression();
app.UseRouting(); // depois do UseStatusCodePages, para a página 404 conseguir ser encontrada
app.UseAntiforgery();

// Arquivos do site (CSS, JS, imagens) já comprimidos e com cache longo:
// o navegador baixa uma vez e reaproveita nas próximas visitas
app.MapStaticAssets();

// Usado por um serviço de "ping" para o Render não dormir (ver GUIA_PUBLICACAO.md)
app.MapGet("/health", () => Results.Text("ok"));

app.MapImagens();
app.MapControllers();
app.MapRazorComponents<crudcomdb.Components.App>()
    .AddInteractiveServerRenderMode();

app.Run();