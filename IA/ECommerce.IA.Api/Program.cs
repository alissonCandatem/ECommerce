using ECommerce.IA.Api.Configuration;
using ECommerce.IA.Api.Data;
using ECommerce.IA.Api.Services;
using ECommerce.IA.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
      options.JsonSerializerOptions.DefaultIgnoreCondition =
          System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddSwaggerConfiguration();
builder.Services.AddAuthConfiguration(builder.Configuration);

// banco vetorial
builder.Services.AddDbContext<IADbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("IA"),
        o => o.UseVector()));

// ollama
builder.Services.AddHttpClient<IOllamaService, OllamaService>(client =>
{
  client.BaseAddress = new Uri(builder.Configuration["Ollama:BaseUrl"]!);
  client.Timeout = TimeSpan.FromMinutes(5); // dependendo do modelo pode ser que demore para retornar a resposta
});

// services
builder.Services.AddScoped<ISchemaService, SchemaService>();
builder.Services.AddScoped<IEmbeddingService, EmbeddingService>();
builder.Services.AddScoped<IQueryService, QueryService>();
builder.Services.AddHostedService<FdwSetupService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();