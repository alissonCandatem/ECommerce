namespace ECommerce.IA.Api.Services.Interfaces
{
  public interface ISqlGeneratorService
  {
    Task<(string Sql, List<Dictionary<string, object?>> Dados)> GerarAsync(string pergunta, string contexto, CancellationToken cancellationToken = default);
  }
}
