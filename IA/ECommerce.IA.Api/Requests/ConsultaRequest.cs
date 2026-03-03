namespace ECommerce.IA.Api.Requests
{
  public sealed record ConsultaRequest
  {
    public string Pergunta { get; init; } = null!;
  }
}
