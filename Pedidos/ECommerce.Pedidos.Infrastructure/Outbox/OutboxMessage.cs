namespace ECommerce.Pedidos.Infrastructure.OutBox
{
  public sealed class OutboxMessage
  {
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Type { get; init; } = null!;
    public string Payload { get; init; } = null!;
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
    public string? Error { get; set; }
  }
}
