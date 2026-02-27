using ECommerce.Mediator.Abstractions;

namespace ECommerce.Produtos.Domain.Primitives
{
  public abstract class AggregateRoot : Entity
  {
    private readonly List<IDomainEvent> _domainEvents = [];

    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected AggregateRoot(Guid id) : base(id) { }

    protected void RaiseDomainEvent(IDomainEvent domainEvent)
        => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents()
        => _domainEvents.Clear();
  }
}
