using Microsoft.AspNetCore.Mvc;
using Pedidos.Api.Models;
using Pedidos.Api.Models.Clientes;
using Pedidos.Api.Services;

namespace Pedidos.Api.Controllers;

[ApiController]
[Route("api/clientes")]
public sealed class ClientesController(ClienteService service) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(ClienteResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Criar(CriarClienteRequest request, CancellationToken ct)
    {
        var result = await service.CriarAsync(request, ct);
        if (!result.Success) return BadRequest(new ApiError(result.Error!));
        return CreatedAtAction(nameof(ObterPorId), new { id = result.Data!.Id }, result.Data);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ClienteResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
        => Ok(await service.ListarAsync(page, pageSize, ct));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ClienteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(Guid id, CancellationToken ct)
    {
        var cliente = await service.ObterPorIdAsync(id, ct);
        return cliente is null ? NotFound(new ApiError("Cliente não encontrado.")) : Ok(cliente);
    }

    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(ClienteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AlterarStatus(Guid id, AlterarStatusClienteRequest request, CancellationToken ct)
    {
        var result = await service.AlterarStatusAsync(id, request, ct);
        return result.Success ? Ok(result.Data) : BadRequest(new ApiError(result.Error!));
    }
}
