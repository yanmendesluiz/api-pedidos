using Microsoft.AspNetCore.Mvc;
using Pedidos.Api.Models;
using Pedidos.Api.Models.Pedidos;
using Pedidos.Api.Services;

namespace Pedidos.Api.Controllers;

[ApiController]
[Route("api/pedidos")]
public sealed class PedidosController(PedidoService service) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(PedidoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Criar(CriarPedidoRequest request, CancellationToken ct)
    {
        var result = await service.CriarAsync(request, ct);
        if (!result.Success) return BadRequest(new ApiError(result.Error!));
        return CreatedAtAction(nameof(ObterPorId), new { id = result.Data!.Id }, result.Data);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<PedidoResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
        => Ok(await service.ListarAsync(page, pageSize, ct));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PedidoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(Guid id, CancellationToken ct)
    {
        var pedido = await service.ObterPorIdAsync(id, ct);
        return pedido is null ? NotFound(new ApiError("Pedido não encontrado.")) : Ok(pedido);
    }

    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(PedidoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AlterarStatus(Guid id, AlterarStatusPedidoRequest request, CancellationToken ct)
    {
        var result = await service.AlterarStatusAsync(id, request, ct);
        return result.Success ? Ok(result.Data) : BadRequest(new ApiError(result.Error!));
    }
}
