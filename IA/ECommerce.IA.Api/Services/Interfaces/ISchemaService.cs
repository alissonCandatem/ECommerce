namespace ECommerce.IA.Api.Services.Interfaces
{
  public interface ISchemaService
  {
    Task IndexarSchemasAsync(CancellationToken cancellationToken = default);
  }
}
