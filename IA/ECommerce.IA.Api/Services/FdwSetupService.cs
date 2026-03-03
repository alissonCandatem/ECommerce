using Npgsql;

namespace ECommerce.IA.Api.Services
{
  public sealed class FdwSetupService : IHostedService
  {
    private readonly IConfiguration _configuration;
    private readonly ILogger<FdwSetupService> _logger;

    private readonly Dictionary<string, string> _servidores = new()
    {
      { "usuarios_server", "postgres-usuarios" },
      { "produtos_server", "postgres-produtos" },
      { "pedidos_server", "postgres-pedidos" }
    };

    private readonly Dictionary<string, (string Server, string Schema, string[] Tables)> _tabelas = new()
    {
      { "usuarios_fdw", ("usuarios_server", "public", new[] { "users" }) },
      { "produtos_fdw", ("produtos_server", "public", new[] { "produtos" }) },
      { "pedidos_fdw", ("pedidos_server", "public", new[] { "pedidos", "pedido_itens" }) }
    };

    public FdwSetupService(
        IConfiguration configuration,
        ILogger<FdwSetupService> logger)
    {
      _configuration = configuration;
      _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
      _logger.LogInformation("Configurando Foreign Data Wrappers...");

      var connectionString = _configuration.GetConnectionString("IA");
      await using var connection = new NpgsqlConnection(connectionString);
      await connection.OpenAsync(cancellationToken);

      await CriarExtensaoAsync(connection, cancellationToken);
      await CriarServidoresAsync(connection, cancellationToken);
      await CriarMapeamentosAsync(connection, cancellationToken);
      await CriarSchemasAsync(connection, cancellationToken);
      await ImportarTabelasAsync(connection, cancellationToken);

      _logger.LogInformation("Foreign Data Wrappers configurados com sucesso");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static async Task CriarExtensaoAsync(NpgsqlConnection connection, CancellationToken cancellationToken)
    {
      await ExecutarAsync(connection, "CREATE EXTENSION IF NOT EXISTS postgres_fdw;", cancellationToken);
    }

    private async Task CriarServidoresAsync(NpgsqlConnection connection, CancellationToken cancellationToken)
    {
      foreach (var (nome, host) in _servidores)
      {
        var dbname = host.Replace("postgres-", "ecommerce_");

        await ExecutarAsync(connection, $"""
          DO $$
          BEGIN
              IF NOT EXISTS (
                  SELECT 1 FROM pg_foreign_server WHERE srvname = '{nome}'
              ) THEN
                  CREATE SERVER {nome}
                      FOREIGN DATA WRAPPER postgres_fdw
                      OPTIONS (host '{host}', port '5432', dbname '{dbname}');
              END IF;
          END $$;
         """, cancellationToken);

        _logger.LogInformation($"Servidor FDW {nome} configurado");
      }
    }

    private async Task CriarMapeamentosAsync(NpgsqlConnection connection, CancellationToken cancellationToken)
    {
      foreach (var (nome, _) in _servidores)
      {
        await ExecutarAsync(connection, $"""
          DO $$
          BEGIN
              IF NOT EXISTS (
                  SELECT 1 FROM pg_user_mappings 
                  WHERE srvname = '{nome}' AND usename = 'postgres'
              ) THEN
                  CREATE USER MAPPING FOR postgres
                      SERVER {nome}
                      OPTIONS (user 'postgres', password 'postgres');
              END IF;
          END $$;
         """, cancellationToken);
      }
    }

    private async Task CriarSchemasAsync(NpgsqlConnection connection, CancellationToken cancellationToken)
    {
      foreach (var (schema, _) in _tabelas)
      {
        await ExecutarAsync(connection, $"DROP SCHEMA IF EXISTS {schema} CASCADE;", cancellationToken);
        await ExecutarAsync(connection, $"CREATE SCHEMA {schema};", cancellationToken);
      }
    }

    private async Task ImportarTabelasAsync(NpgsqlConnection connection, CancellationToken cancellationToken)
    {
      foreach (var (schema, (server, remoteSchema, tables)) in _tabelas)
      {
        var tableList = string.Join(", ", tables);

        await ExecutarAsync(connection, $"""
          IMPORT FOREIGN SCHEMA {remoteSchema}
              LIMIT TO ({tableList})
              FROM SERVER {server}
              INTO {schema};
         """, cancellationToken);

        _logger.LogInformation($"Tabelas [{tableList}] importadas do servidor {server} para o schema {schema}");
      }
    }

    private static async Task ExecutarAsync(NpgsqlConnection connection, string sql, CancellationToken cancellationToken)
    {
      await using var command = new NpgsqlCommand(sql, connection);
      await command.ExecuteNonQueryAsync(cancellationToken);
    }
  }
}
