using ECommerce.IA.Api.Services.Interfaces;

namespace ECommerce.IA.Api.Services
{
  public sealed class SqlGeneratorService : ISqlGeneratorService
  {
    private readonly IOllamaService _ollamaService;
    private readonly ISqlExecutorService _sqlExecutor;
    private readonly ILogger<SqlGeneratorService> _logger;
    private const int MaxTentativas = 3;
    private readonly IConfiguration _configuration;

    public SqlGeneratorService(
      IOllamaService ollamaService,
      ISqlExecutorService sqlExecutor,
      ILogger<SqlGeneratorService> logger,
      IConfiguration configuration
    )
    {
      _ollamaService = ollamaService;
      _sqlExecutor = sqlExecutor;
      _logger = logger;
      _configuration = configuration;
    }

    public async Task<(string Sql, List<Dictionary<string, object?>> Dados)> GerarAsync(string pergunta, string contexto, CancellationToken cancellationToken = default)
    {
      string? ultimoErro = null;
      string? ultimoSql = null;

      for (var tentativa = 1; tentativa <= MaxTentativas; tentativa++)
      {
        try
        {
          _logger.LogInformation($"Gerando SQL - tentativa {tentativa}/{MaxTentativas}");

          _logger.LogInformation($"Prompt com erro: {ultimoErro != null}");

          var prompt = ultimoErro == null ? MontarPrompt(pergunta, contexto) : MontarPromptComErro(pergunta, contexto, ultimoSql!, ultimoErro);

          var sqlBruto = await _ollamaService.GerarSqlAsync(prompt, cancellationToken);

          var sql = ExtrairSql(sqlBruto);
          ultimoSql = sql;

          _logger.LogInformation($"SQL gerado (tentativa {tentativa}): {sql}");

          var dados = await _sqlExecutor.ExecutarAsync(sql, cancellationToken);

          _logger.LogInformation($"SQL válido na tentativa {tentativa}");

          return (sql, dados);
        }
        catch (Exception ex)
        {
          ultimoErro = ex.Message;
          _logger.LogWarning($"Tentativa {tentativa} falhou: {ex.Message}");

          if (tentativa == MaxTentativas)
            throw new InvalidOperationException($"Não foi possível gerar um SQL válido após {MaxTentativas} tentativas. " + $"Último erro: {ex.Message}");
        }
      }

      throw new InvalidOperationException("Erro inesperado");
    }

    private string MontarPrompt(string pergunta, string contexto)
    {
      var schemasDisponiveis = ObterSchemasDisponiveis();

      return $"""
        Garanta que o SQL responda EXATAMENTE à intenção semântica da pergunta.
        
        Formato obrigatório:
        BANCO: IA
        SQL: <sql aqui>
        
        Schemas FDW disponíveis (use EXATAMENTE esses nomes):
        {schemasDisponiveis}
        
        Estrutura das tabelas:
        {contexto}
        
        Regras:
        - Use JOIN para cruzar tabelas, identificando relações pelos nomes das colunas
        - Todas as tabelas ficam em schemas com sufixo _fdw. Exemplo: se o banco é "Usuarios" e a tabela é "users", use "usuarios_fdw.users"
        - Para montar o schema use: <nome_do_banco_em_minusculo>_fdw.<nome_da_tabela>
        - Quando a pergunta envolver dados de múltiplas tabelas ou bancos, use JOIN
        - Identifique as relações entre tabelas pelos nomes das colunas (ex: usuario_id, pedido_id)
        - Nunca selecione colunas sensíveis como senha_hash, refresh_token, refresh_token_expiry ou similares
        - Use sintaxe PostgreSQL
        - Use WHERE quando a pergunta envolver filtro temporal, numérico ou condicional
        - Sem comentários, sem parâmetros, sem markdown
        - Termine com ponto e vírgula
        - O SQL deve refletir exatamente a intenção da pergunta.
        - Se a pergunta envolver ranking (segundo, terceiro, top N), o SQL deve implementar corretamente essa posição.
        - Se envolver dependência entre agregações, use subqueries ou CTEs.

        Pergunta: {pergunta}
       """;
    }

    private string MontarPromptComErro(string pergunta, string contexto, string sqlAnterior, string erro)
    {
      var schemasDisponiveis = ObterSchemasDisponiveis();

      return $"""
        Você é um gerador de SQL puro para PostgreSQL. Sua única saída deve ser exatamente neste formato:
                
        BANCO: IA
        SQL: <comando SQL válido>
        
       Schemas FDW disponíveis (use EXATAMENTE esses nomes):
        {schemasDisponiveis}

        Tabelas disponíveis:
        {contexto}

        O SQL anterior gerou um erro. Analise o erro e corrija o SQL.
                
        SQL com erro:
        {sqlAnterior}
                
        Erro retornado pelo PostgreSQL:
        {erro}
        
       ANÁLISE O ERRO E CORRIJA.

        Regras:
        - Use JOIN para cruzar tabelas, identificando relações pelos nomes das colunas
        - Todas as tabelas ficam em schemas com sufixo _fdw. Exemplo: se o banco é "Usuarios" e a tabela é "users", use "usuarios_fdw.users"
        - Para montar o schema use: <nome_do_banco_em_minusculo>_fdw.<nome_da_tabela>
        - Quando a pergunta envolver dados de múltiplas tabelas ou bancos, use JOIN
        - Identifique as relações entre tabelas pelos nomes das colunas (ex: usuario_id, pedido_id)
        - Nunca selecione colunas sensíveis como senha_hash, refresh_token, refresh_token_expiry ou similares
        - Use sintaxe PostgreSQL
        - Use WHERE quando a pergunta envolver filtro temporal, numérico ou condicional
        - Sem comentários, sem parâmetros, sem markdown
        - Termine com ponto e vírgula
        - O SQL deve refletir exatamente a intenção da pergunta.
        - Se a pergunta envolver ranking (segundo, terceiro, top N), o SQL deve implementar corretamente essa posição.
        - Se envolver dependência entre agregações, use subqueries ou CTEs.
                           
        Pergunta do usuário: {pergunta}
       """;
    }

    private string ObterSchemasDisponiveis()
    {
      var bancos = _configuration
          .GetSection("ConnectionStrings")
          .GetChildren()
          .Select(x => x.Key)
          .Where(k => k != "IA")
          .ToList();

      return string.Join("\n", bancos.Select(b => $"- {b.ToLower()}_fdw.*"));
    }

    private static string ExtrairSql(string sqlBruto)
    {
      var linhas = sqlBruto.Split('\n')
          .Where(l => !l.TrimStart().StartsWith("BANCO:", StringComparison.OrdinalIgnoreCase))
          .ToArray();

      var sql = string.Join("\n", linhas)
          .Replace("SQL:", "", StringComparison.OrdinalIgnoreCase)
          .Replace("```sql", "")
          .Replace("```", "")
          .Trim();

      var semicolon = sql.IndexOf(';');
      if (semicolon > 0)
        sql = sql[..(semicolon + 1)];

      return sql.Trim();
    }
  }
}
