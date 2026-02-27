using ECommerce.Usuarios.Application;
using ECommerce.Usuarios.Configuration;
using ECommerce.Usuarios.Filters;
using ECommerce.Usuarios.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options =>
{
  options.Filters.Add<ResultResponseFilter>();
})
.AddJsonOptions(options =>
{
  options.JsonSerializerOptions.DefaultIgnoreCondition =
      System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
});

builder.Services.AddSwaggerConfiguration();
builder.Services.AddAuthConfiguration(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();