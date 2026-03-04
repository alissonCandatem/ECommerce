using ECommerce.IA.Api.Services.Interfaces;
using System.Text.Json;

namespace ECommerce.IA.Api.Services
{
  public sealed class ResponseFormatterService : IResponseFormatterService
  {
    private readonly IOllamaService _ollamaService;
    private readonly ILogger<ResponseFormatterService> _logger;

    public ResponseFormatterService(
      IOllamaService ollamaService,
      ILogger<ResponseFormatterService> logger
    )
    {
      _ollamaService = ollamaService;
      _logger = logger;
    }

    public async Task<string> FormatarAsync(string pergunta, string sql, List<Dictionary<string, object?>> dados, CancellationToken cancellationToken = default)
    {
      var prompt = MontarPrompt(pergunta, sql, dados);
      var resposta = await _ollamaService.GerarRespostaAsync(prompt, cancellationToken);

      _logger.LogInformation("Resposta formatada com sucesso");
      return resposta;
    }

    private static string MontarPrompt(string pergunta, string sql, List<Dictionary<string, object?>> dados)
    {
      var sqlUpper = sql.ToUpper().Trim();
      var leitura = sqlUpper.StartsWith("INSERT") ||
                      sqlUpper.StartsWith("UPDATE") ||
                      sqlUpper.StartsWith("DELETE");

      if (leitura)
      {
        return $"""
          Responda em português de forma direta em 1 frase confirmando que a operação foi realizada.
          Não mencione SQL ou detalhes técnicos.
                    
          Operação solicitada: {pergunta}
         """;
      }

      var dadosJson = JsonSerializer.Serialize(dados, new JsonSerializerOptions
      {
        WriteIndented = true
      });

      return $"""
        Responda a pergunta abaixo em português de forma direta e objetiva em no máximo 3 frases.
        Não mencione SQL, banco de dados ou detalhes técnicos.
        Baseie sua resposta nos dados fornecidos.
                
        Pergunta: {pergunta}
        Dados retornados: {dadosJson}
                
        Resposta direta:
       """;
    }
  }
}
