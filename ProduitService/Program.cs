using Microsoft.EntityFrameworkCore;
using Steeltoe.Common.Http.Discovery;
using Steeltoe.Discovery.Client;
using Steeltoe.Discovery.Eureka;
using WebApplication1.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDiscoveryClient(builder.Configuration);

builder.Services.AddServiceDiscovery(options => options.UseEureka());

builder.Services.AddHttpClient("commentaire-service", client => {
    client.BaseAddress = new Uri("lb://commentaire-service/");
}).AddRandomLoadBalancer();

// Configuration du client HTTP pour le service Commentaire
//builder.Services.AddHttpClient("CommentaireService")
//    .AddServiceDiscovery();

//builder.Services.AddHttpClient("CommentaireService", client =>
//{
//    client.BaseAddress = new Uri("http://localhost:5242"); // Adresse du service Commentaire
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