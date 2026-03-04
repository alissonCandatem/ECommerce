namespace ECommerce.IA.Api.Services.Interfaces
{
  public interface IQueryCacheService
  {
    Task<string?> BuscarSqlCacheadoAsync(string pergunta, CancellationToken cancellationToken = default);
    Task SalvarAsync(string pergunta, string sql, CancellationToken cancellationToken = default);
  }
}
