using CommentaireService.Data;
using Microsoft.EntityFrameworkCore;
using Steeltoe.Common.Http.Discovery;
using Steeltoe.Discovery.Client;
using Steeltoe.Discovery.Eureka;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDiscoveryClient(builder.Configuration);

builder.Services.AddDiscoveryClient(builder.Configuration);

builder.Services.AddServiceDiscovery(options => options.UseEureka());

builder.Services.AddHttpClient("produit-service", client => {
    client.BaseAddress = new Uri("lb://produit-service/");
}).AddRandomLoadBalancer();

// Configuration du client HTTP pour le service Produit
//builder.Services.AddHttpClient("ProduitService")
//    .AddServiceDiscovery();

//builder.Services.AddHttpClient("ProduitService", client =>
//{
//    client.BaseAddress = new Uri("http://localhost:5185"); // Adresse de ProduitService
//});

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseDiscoveryClient();

app.MapControllers();

app.Run();
