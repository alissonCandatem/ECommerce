using ECommerce.Mediator;
using ECommerce.Produtos.Domain.Primitives;
using ECommerce.Produtos.Infrastructure.OutBox;
using Microsoft.EntityFrameworkCore.Storage;
using System.Text.Json;

namespace ECommerce.Produtos.Infrastructure
{
  public sealed class UnitOfWork : IUnitOfWork
  {
    private readonly AppDbContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(AppDbContext context)
    {
      _context = context;
    }

    public async Task Begin(CancellationToken cancellationToken = default)
    {
      if (_transaction != null)
        return;

      _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task Commit(CancellationToken cancellationToken = default)
    {
      await SaveOutboxMessages();
      await _context.SaveChangesAsync(cancellationToken);

      if (_transaction != null)
      {
        await _transaction.CommitAsync(cancellationToken);
        await _transaction.DisposeAsync();
        _transaction = null;
      }
    }

    public async Task Rollback(CancellationToken cancellationToken = default)
    {
      if (_transaction != null)
      {
        await _transaction.RollbackAsync(cancellationToken);
        await _transaction.DisposeAsync();
        _transaction = null;
      }
    }

    private Task SaveOutboxMessages()
    {
      var aggregates = _context.ChangeTracker
      .Entries<AggregateRoot>()
      .Where(e => e.Entity.DomainEvents.Any())
      .Select(e => e.Entity)
      .ToList();

      var outboxMessages = aggregates
      .SelectMany(a => a.DomainEvents)
      .Select(e => new OutboxMessage
      {
        Id = Guid.NewGuid(),
        Type = e.GetType().AssemblyQualifiedName!,
        Payload = JsonSerializer.Serialize(e, e.GetType()),
        OccurredAt = e.OccurredAt
      })
      .ToList();

      if (outboxMessages.Any())
      {
        _context.OutboxMessages.AddRange(outboxMessages);
        aggregates.ForEach(a => a.ClearDomainEvents());
      }

      return Task.CompletedTask;
    }
  }
}
