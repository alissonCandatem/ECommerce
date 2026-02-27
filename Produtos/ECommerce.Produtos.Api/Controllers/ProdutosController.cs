using ECommerce.Mediator.Abstractions;
using ECommerce.Produtos.Application.Commands.Produto;
using ECommerce.Produtos.Application.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Produtos.Api.Controllers
{
  [Route("api/produtos")]
  [ApiController]
  [Authorize]
  public class ProdutosController : ControllerBase
  {
    private readonly ICommandDispatcher _dispatcher;

    public ProdutosController(ICommandDispatcher dispatcher)
    {
      _dispatcher = dispatcher;
    }

    [HttpGet]
    public async Task<IActionResult> ObterTodos([FromQuery] string? categoria)
        => Ok(await _dispatcher.Send(new ObterProdutosQuery { Categoria = categoria }));

    [HttpPost]
    public async Task<IActionResult> Criar(CriarProdutoCommand command)
        => Ok(await _dispatcher.Send(command));

    [HttpPut("{id}")]
    public async Task<IActionResult> Atualizar(Guid id, AtualizarProdutoCommand command)
    {
      return Ok(await _dispatcher.Send(command with { Id = id }));
    }

    [HttpPatch("{id}/estoque")]
    public async Task<IActionResult> AtualizarEstoque(Guid id, AtualizarEstoqueCommand command)
    {
      return Ok(await _dispatcher.Send(command with { Id = id }));
    }
  }
}
