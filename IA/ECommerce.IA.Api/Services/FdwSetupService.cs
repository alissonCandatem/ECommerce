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

    public FdwSetupService(
      IConfiguration configuration,
      ILogger<FdwSetupService> logger
    )
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

    private async Task ImportarTabelasAsync(NpgsqlConnection connection, CancellationToken cancellationToken)
    {
      foreach (var (nomeServidor, _) in _servidores)
      {
        var schema = nomeServidor.Replace("_server", "_fdw");

        await ExecutarAsync(connection, $"""
          DROP SCHEMA IF EXISTS {schema} CASCADE;
          CREATE SCHEMA {schema};
          IMPORT FOREIGN SCHEMA public
              EXCEPT (outbox_messages, "__EFMigrationsHistory")
              FROM SERVER {nomeServidor}
              INTO {schema};
         """,
         cancellationToken
        );

        _logger.LogInformation($"Schema {schema} importado do servidor {nomeServidor}");
      }
    }

    private static async Task ExecutarAsync(NpgsqlConnection connection, string sql, CancellationToken cancellationToken)
    {
      await using var command = new NpgsqlCommand(sql, connection);
      await command.ExecuteNonQueryAsync(cancellationToken);
    }
  }
}
