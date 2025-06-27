using CommentaireService.Data;
using CommentaireService.Event;
using Microsoft.EntityFrameworkCore;
using Polly;
using Steeltoe.Connector.RabbitMQ;
using Steeltoe.Discovery.Client;
using Steeltoe.Discovery.Eureka;
using Steeltoe.Messaging.RabbitMQ.Config;
using Steeltoe.Messaging.RabbitMQ.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDiscoveryClient(builder.Configuration);

builder.Services.AddServiceDiscovery(options => options.UseEureka());

builder.Services.AddHttpClient("produit-service", client =>
{
    client.BaseAddress = new Uri("lb://produit-service/");
}).AddRandomLoadBalancer()    
    .AddTransientHttpErrorPolicy(policy => policy.CircuitBreakerAsync(3, TimeSpan.FromSeconds(5),
    onBreak: (result, timespan) => {
        Console.WriteLine($"Circuit ouvert pendant {timespan.TotalSeconds} secondes suite à : {result.Exception?.Message}");
    },
    onReset: () => {
        Console.WriteLine("Circuit fermé, tout est rétabli.");
    },
    onHalfOpen: () => {
        Console.WriteLine("Circuit à moitié ouvert, test en cours...");
    }))
.AddPolicyHandler(Policy.BulkheadAsync<HttpResponseMessage>(maxParallelization: 5, maxQueuingActions: 10));

builder.Services.AddRabbitMQConnection(builder.Configuration);
builder.Services.AddRabbitServices(true);
builder.Services.AddRabbitAdmin();
builder.Services.AddRabbitTemplate();

builder.Services.AddSingleton<ProduitEventHandler>();
builder.Services.AddRabbitListeners<ProduitEventHandler>();
builder.Services.AddRabbitExchange("ms.produit", ExchangeType.TOPIC);


builder.Services.AddSingleton<CommentaireEventHandler>();
builder.Services.AddRabbitListeners<CommentaireEventHandler>();
builder.Services.AddRabbitExchange("ms.commentaire", ExchangeType.TOPIC);

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