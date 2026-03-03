namespace ECommerce.IA.Api.Responses
{
  public sealed record ConsultaResponse
  {
    public string Pergunta { get; init; } = null!;
    public string SqlGerado { get; init; } = null!;
    public string Resposta { get; init; } = null!;
    public object? Dados { get; init; }
  }
}
