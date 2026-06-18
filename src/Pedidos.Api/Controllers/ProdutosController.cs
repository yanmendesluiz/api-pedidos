using Microsoft.AspNetCore.Mvc;
using Pedidos.Api.Models;
using Pedidos.Api.Models.Produtos;
using Pedidos.Api.Services;

namespace Pedidos.Api.Controllers;

[ApiController]
[Route("api/produtos")]
public sealed class ProdutosController(ProdutoService service) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(ProdutoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Criar(CriarProdutoRequest request, CancellationToken ct)
    {
        var result = await service.CriarAsync(request, ct);
        if (!result.Success) return BadRequest(new ApiError(result.Error!));
        return CreatedAtAction(nameof(ObterPorId), new { id = result.Data!.Id }, result.Data);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ProdutoResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
        => Ok(await service.ListarAsync(page, pageSize, ct));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProdutoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(Guid id, CancellationToken ct)
    {
        var produto = await service.ObterPorIdAsync(id, ct);
        return produto is null ? NotFound(new ApiError("Produto não encontrado.")) : Ok(produto);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ProdutoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Atualizar(Guid id, AtualizarProdutoRequest request, CancellationToken ct)
    {
        var result = await service.AtualizarAsync(id, request, ct);
        return result.Success ? Ok(result.Data) : BadRequest(new ApiError(result.Error!));
    }

    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(ProdutoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AlterarStatus(Guid id, AlterarStatusProdutoRequest request, CancellationToken ct)
    {
        var result = await service.AlterarStatusAsync(id, request, ct);
        return result.Success ? Ok(result.Data) : BadRequest(new ApiError(result.Error!));
    }

    [HttpPatch("{id:guid}/estoque")]
    [ProducesResponseType(typeof(ProdutoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AtualizarEstoque(Guid id, AtualizarEstoqueRequest request, CancellationToken ct)
    {
        var result = await service.AtualizarEstoqueAsync(id, request, ct);
        return result.Success ? Ok(result.Data) : BadRequest(new ApiError(result.Error!));
    }

    [HttpPatch("{id:guid}/preco")]
    [ProducesResponseType(typeof(ProdutoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AtualizarPreco(Guid id, AtualizarPrecoRequest request, CancellationToken ct)
    {
        var result = await service.AtualizarPrecoAsync(id, request, ct);
        return result.Success ? Ok(result.Data) : BadRequest(new ApiError(result.Error!));
    }
}
