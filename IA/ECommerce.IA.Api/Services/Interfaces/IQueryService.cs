using ECommerce.IA.Api.Responses;

namespace ECommerce.IA.Api.Services.Interfaces
{
  public interface IQueryService
  {
    Task<ConsultaResponse> ConsultarAsync(string pergunta, CancellationToken cancellationToken = default);
  }
}
