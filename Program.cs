using crudcomdb.Data;
using crudcomdb.Interfaces;
using crudcomdb.Repositories;
using crudcomdb.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. BANCO DE DADOS
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. INJEÇÃO DE DEPENDÊNCIA
builder.Services.AddScoped<IPetRepository, PetRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IInteresseRepository, InteresseRepository>();
builder.Services.AddScoped<AuthService>();
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

// 4. PIPELINE
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

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapControllers();
app.MapRazorComponents<crudcomdb.Components.App>()
    .AddInteractiveServerRenderMode();

app.Run();