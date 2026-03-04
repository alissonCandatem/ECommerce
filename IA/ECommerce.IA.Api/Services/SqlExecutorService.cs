using ECommerce.IA.Api.Services.Interfaces;
using Npgsql;

namespace ECommerce.IA.Api.Services
{
  public sealed class SqlExecutorService : ISqlExecutorService
  {
    private readonly IConfiguration _configuration;
    private readonly ILogger<SqlExecutorService> _logger;

    public SqlExecutorService(
      IConfiguration configuration,
      ILogger<SqlExecutorService> logger
    )
    {
      _configuration = configuration;
      _logger = logger;
    }

    public async Task<List<Dictionary<string, object?>>> ExecutarAsync(string sql, CancellationToken cancellationToken = default)
    {
      var connectionString = _configuration.GetConnectionString("IA");
      var resultado = new List<Dictionary<string, object?>>();

      await using var connection = new NpgsqlConnection(connectionString);
      await connection.OpenAsync(cancellationToken);

      await using var command = new NpgsqlCommand(sql, connection);
      await using var reader = await command.ExecuteReaderAsync(cancellationToken);

      while (await reader.ReadAsync(cancellationToken))
      {
        var row = new Dictionary<string, object?>();
        for (var i = 0; i < reader.FieldCount; i++)
          row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);

        resultado.Add(row);
      }

      _logger.LogInformation($"Query retornou {resultado.Count} registros");
      return resultado;
    }
  }
}
