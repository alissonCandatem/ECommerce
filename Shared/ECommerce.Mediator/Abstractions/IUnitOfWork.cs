namespace ECommerce.Mediator
{
  public interface IUnitOfWork
  {
    Task Begin(CancellationToken cancellationToken = default);
    Task Commit(CancellationToken cancellationToken = default);
    Task Rollback(CancellationToken cancellationToken = default);
  }
}
