namespace ECommerce.IA.Api.Services.Interfaces
{
  public interface ISqlExecutorService
  {
    Task<List<Dictionary<string, object?>>> ExecutarAsync(string sql, CancellationToken cancellationToken = default);
  }
}
