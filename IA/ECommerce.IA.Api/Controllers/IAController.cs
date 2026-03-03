using ECommerce.IA.Api.Requests;
using ECommerce.IA.Api.Responses;
using ECommerce.IA.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.IA.Api.Controllers
{
  [Route("api/ia")]
  [ApiController]
  [Authorize]
  public class IAController : ControllerBase
  {
    private readonly IQueryService _queryService;
    private readonly ISchemaService _schemaService;

    public IAController(
      IQueryService queryService,
      ISchemaService schemaService
    )
    {
      _queryService = queryService;
      _schemaService = schemaService;
    }

    [HttpPost("consultar")]
    public async Task<ConsultaResponse> Consultar(ConsultaRequest request, CancellationToken cancellationToken)
      => await _queryService.ConsultarAsync(request.Pergunta, cancellationToken);

    [HttpPost("indexar")]
    public async Task<IActionResult> Indexar(CancellationToken cancellationToken)
    {
      await _schemaService.IndexarSchemasAsync(cancellationToken);
      return Ok("Schemas indexados com sucesso");
    }
  }
}
