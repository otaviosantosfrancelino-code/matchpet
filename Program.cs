using crudcomdb.Data;
using crudcomdb.Endpoints;
using crudcomdb.Interfaces;
using crudcomdb.Repositories;
using crudcomdb.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. BANCO DE DADOS
// O Neon "dorme" quando fica parado e derruba conexões antigas: a nova tentativa automática evita erros aleatórios
// Fábrica: cada operação do chat/rede social abre o seu próprio contexto (seguro com mensagens em tempo real)
// A senha do banco NUNCA fica no código (o repositório é público):
// no Render vem da variável ConnectionStrings__DefaultConnection; no computador, do "dotnet user-secrets".
builder.Configuration.AddUserSecrets<Program>(optional: true);
var conexaoBanco = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(conexaoBanco))
    throw new InvalidOperationException("Configure a conexão do banco: variável de ambiente ConnectionStrings__DefaultConnection (Render) ou 'dotnet user-secrets set ConnectionStrings:DefaultConnection ...' (local).");
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseNpgsql(conexaoBanco,
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
builder.Services.AddScoped<NotificacaoService>();
builder.Services.AddScoped<RankingService>();
builder.Services.AddSingleton<NotificacoesService>();   // avisa em tempo real (nova mensagem, pedido de amizade)
builder.Services.AddSingleton<GeoService>();            // coordenadas das cidades para o mapa
builder.Services.AddSingleton<LimiteTentativasService>(); // bloqueia quem erra a senha muitas vezes
builder.Services.AddScoped<ModeracaoService>();
builder.Services.AddScoped<FavoritosService>();
builder.Services.AddScoped<QuestionariosService>();
builder.Services.AddSingleton<CofreDocumentos>();          // criptografa a foto do documento (LGPD)
builder.Services.AddHostedService<AcompanhamentoService>(); // lembretes 7 e 30 dias depois da adoção

// Chaves de criptografia guardadas no banco: as sessões salvas e os documentos continuam legíveis
// depois de cada nova publicação no Render (antes, cada deploy gerava chaves novas e todos eram deslogados)
builder.Services.AddDataProtection()
    .SetApplicationName("MatchPetSP")
    .PersistKeysToDbContext<AppDbContext>();

// Limite de pedidos por IP na API (evita robôs e abuso)
builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    o.AddPolicy("api", ctx => System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
        ctx.Connection.RemoteIpAddress?.ToString() ?? "desconhecido",
        _ => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions { PermitLimit = 60, Window = TimeSpan.FromMinutes(1) }));
});
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
            await DadosDemonstracaoV3.CriarAsync(db, log);
        }
        await ContasBanca.PrepararAsync(db, app.Configuration, app.Environment, log);
        await escopo.ServiceProvider.GetRequiredService<ModeracaoService>().ProtegerDocumentosAntigosAsync();
    }
    catch (Exception ex)
    {
        log.LogError(ex, "Falha ao preparar o banco de dados na inicialização");
    }
}

// 5. PIPELINE
app.UseForwardedHeaders();

// Cabeçalhos de segurança em todas as respostas
app.Use(async (ctx, proximo) =>
{
    var h = ctx.Response.Headers;
    h["X-Content-Type-Options"] = "nosniff";                       // o navegador não "adivinha" o tipo do arquivo
    h["X-Frame-Options"] = "SAMEORIGIN";                            // outro site não pode abrir o Match-Pet escondido numa moldura
    h["Content-Security-Policy"] = "frame-ancestors 'self'; base-uri 'self'; object-src 'none'; form-action 'self'";
    h["Referrer-Policy"] = "strict-origin-when-cross-origin";       // não vaza o endereço completo para outros sites
    h["Permissions-Policy"] = "camera=(), microphone=(), geolocation=(self), payment=()";
    await proximo();
});
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
app.UseRateLimiter();
app.UseAntiforgery();

// Arquivos do site (CSS, JS, imagens) já comprimidos e com cache longo:
// o navegador baixa uma vez e reaproveita nas próximas visitas
app.MapStaticAssets();

// Usado por um serviço de "ping" para o Render não dormir (ver GUIA_PUBLICACAO.md)
app.MapGet("/health", () => Results.Text("ok"));

app.MapImagens();
app.MapCompartilhamento();
app.MapControllers();
app.MapRazorComponents<crudcomdb.Components.App>()
    .AddInteractiveServerRenderMode();

app.Run();