namespace ECommerce.IA.Api.Services.Interfaces
{
  public interface IResponseFormatterService
  {
    Task<string> FormatarAsync(string pergunta, string sql, List<Dictionary<string, object?>> dados, CancellationToken cancellationToken = default);
  }
}
