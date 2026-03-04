using ECommerce.IA.Api.Services.Interfaces;

namespace ECommerce.IA.Api.Services
{
  public sealed class SqlGeneratorService : ISqlGeneratorService
  {
    private readonly IOllamaService _ollamaService;
    private readonly ISqlExecutorService _sqlExecutor;
    private readonly ILogger<SqlGeneratorService> _logger;
    private const int MaxTentativas = 3;

    public SqlGeneratorService(
      IOllamaService ollamaService,
      ISqlExecutorService sqlExecutor,
      ILogger<SqlGeneratorService> logger
    )
    {
      _ollamaService = ollamaService;
      _sqlExecutor = sqlExecutor;
      _logger = logger;
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
      var schemasDisponiveis = ExtrairSchemasDoContexto(contexto);

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
        - Gere APENAS comandos SELECT. Nunca gere INSERT, UPDATE, DELETE, DROP, TRUNCATE ou ALTER.
        - Use apenas os schemas e tabelas fornecidos.
        - Todas as tabelas estão em schemas com sufixo _fdw no formato <banco>_fdw.<tabela>.
        - Nunca use o mesmo alias para tabelas diferentes
        - Descubra relações entre tabelas pelos nomes das colunas.
        - Use sintaxe PostgreSQL válida.
        - Não invente colunas ou tabelas.
        - Se a pergunta exigir múltiplas etapas lógicas, resolva usando CTEs sequenciais.
        - Para ranking use ORDER BY com LIMIT, nunca use RANK() OVER ou RANK() WITHIN GROUP
        - Não explique nada.
        - Retorne apenas no formato especificado.

        Pergunta: {pergunta}
       """;
    }

    private string MontarPromptComErro(string pergunta, string contexto, string sqlAnterior, string erro)
    {
      var schemasDisponiveis = ExtrairSchemasDoContexto(contexto);

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
        - Gere APENAS comandos SELECT. Nunca gere INSERT, UPDATE, DELETE, DROP, TRUNCATE ou ALTER.
        - Use apenas os schemas e tabelas fornecidos.
        - Todas as tabelas estão em schemas com sufixo _fdw no formato <banco>_fdw.<tabela>.
        - Nunca use o mesmo alias para tabelas diferentes
        - Descubra relações entre tabelas pelos nomes das colunas.
        - Use sintaxe PostgreSQL válida.
        - Não invente colunas ou tabelas.
        - Se a pergunta exigir múltiplas etapas lógicas, resolva usando CTEs sequenciais.
        - Para ranking use ORDER BY com LIMIT, nunca use RANK() OVER ou RANK() WITHIN GROUP
        - Não explique nada.
        - Retorne apenas no formato especificado.
                           
        Pergunta do usuário: {pergunta}
       """;
    }

    private string ExtrairSchemasDoContexto(string contexto)
    {
      var schemas = new List<string>();
      string? bancoAtual = null;
      string? tabelaAtual = null;

      foreach (var linha in contexto.Split('\n'))
      {
        if (linha.StartsWith("Banco:"))
          bancoAtual = linha.Replace("Banco:", "").Trim().ToLower();
        else if (linha.StartsWith("Tabela:"))
        {
          tabelaAtual = linha.Replace("Tabela:", "").Trim();
          if (bancoAtual != null && tabelaAtual != null)
            schemas.Add($"- {bancoAtual}_fdw.{tabelaAtual}");
        }
      }

      return string.Join("\n", schemas.Distinct());
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
