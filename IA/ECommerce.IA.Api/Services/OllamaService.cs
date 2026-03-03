using ECommerce.IA.Api.Services.Interfaces;
using Pgvector;
using System.Text;
using System.Text.Json;

namespace ECommerce.IA.Api.Services
{
  public sealed class OllamaService : IOllamaService
  {
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OllamaService> _logger;

    public OllamaService(
      HttpClient httpClient,
      IConfiguration configuration,
      ILogger<OllamaService> logger
    )
    {
      _httpClient = httpClient;
      _configuration = configuration;
      _logger = logger;
    }

    public async Task<Vector> GerarEmbeddingAsync(string texto, CancellationToken cancellationToken = default)
    {
      var model = _configuration["Ollama:EmbeddingModel"];
      var request = new { model, prompt = texto };
      var json = JsonSerializer.Serialize(request);
      var content = new StringContent(json, Encoding.UTF8, "application/json");

      var response = await _httpClient.PostAsync("/api/embeddings", content, cancellationToken);

      response.EnsureSuccessStatusCode();

      var result = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: cancellationToken);

      var embedding = result.GetProperty("embedding")
        .EnumerateArray()
        .Select(x => x.GetSingle())
        .ToArray();

      _logger.LogInformation($"Embedding gerado com {embedding.Length} dimensões");

      return new Vector(embedding);
    }

    public async Task<string> GerarSqlAsync(string prompt, CancellationToken cancellationToken = default)
    {
      return await GerarAsync(_configuration["Ollama:SqlModel"]!, prompt, cancellationToken);
    }

    public async Task<string> GerarRespostaAsync(string prompt, CancellationToken cancellationToken = default)
    {
      return await GerarAsync(_configuration["Ollama:ChatModel"]!, prompt, cancellationToken);
    }

    private async Task<string> GerarAsync(string model, string prompt, CancellationToken cancellationToken)
    {
      var request = new { model, prompt, stream = false };
      var json = JsonSerializer.Serialize(request);
      var content = new StringContent(json, Encoding.UTF8, "application/json");

      _logger.LogInformation($"Enviando prompt para o modelo {model}");

      var response = await _httpClient.PostAsync("/api/generate", content, cancellationToken);

      response.EnsureSuccessStatusCode();

      var result = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: cancellationToken);

      return result.GetProperty("response").GetString()!;
    }
  }
}
