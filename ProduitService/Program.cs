using Microsoft.EntityFrameworkCore;
using Polly;
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