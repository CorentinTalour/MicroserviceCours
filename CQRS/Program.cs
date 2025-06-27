using CQRS.Data;
using CQRS.Event;
using Microsoft.EntityFrameworkCore;
using Steeltoe.Connector.RabbitMQ;
using Steeltoe.Messaging.RabbitMQ.Config;
using Steeltoe.Messaging.RabbitMQ.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Utilisation du DbContextFactory ici
builder.Services.AddDbContextFactory<ReadDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("ReadDbConnection")));

// RabbitMQ
builder.Services.AddRabbitMQConnection(builder.Configuration);
builder.Services.AddRabbitServices(true);
builder.Services.AddRabbitAdmin();
builder.Services.AddRabbitTemplate();
builder.Services.AddRabbitExchange("ms.produit", ExchangeType.TOPIC);

// Event Handlers (Singleton attendu par Steeltoe)
builder.Services.AddSingleton<ProductEventHandler>();
builder.Services.AddRabbitListeners<ProductEventHandler>();

builder.Services.AddSingleton<CommentaireEventHandler>();
builder.Services.AddRabbitListeners<CommentaireEventHandler>();
builder.Services.AddRabbitExchange("ms.commentaire", ExchangeType.TOPIC);

builder.Services.AddControllers();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

var app = builder.Build();

app.MapControllers();

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.Run();