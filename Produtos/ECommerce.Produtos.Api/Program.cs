using ECommerce.Produtos.Application;
using ECommerce.Produtos.Configuration;
using ECommerce.Produtos.Filters;
using ECommerce.Produtos.Infrastructure;

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

if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

// removido para testes internos
//app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();