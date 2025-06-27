using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Eureka;
using Steeltoe.Discovery.Client;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = "http://localhost:8080/realms/master";
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateAudience = false
        };
    });

builder.Services.AddAuthorization();

// Eureka Discovery
builder.Services.AddDiscoveryClient(builder.Configuration);

// Ocelot + Eureka provider
builder.Services.AddOcelot()
    .AddEureka();

var app = builder.Build();

app.UseDiscoveryClient();

app.UseAuthentication();
app.UseAuthorization();

await app.UseOcelot();

app.Run();