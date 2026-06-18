using System.ComponentModel.DataAnnotations;
using Pedidos.Api.Domain;
using Pedidos.Api.Utils;

namespace Pedidos.Api.Models.Pedidos;

public sealed record CriarPedidoRequest(
    Guid ClienteId,
    [Required, MinLength(1)] IReadOnlyList<CriarPedidoItemRequest> Itens);

public sealed record CriarPedidoItemRequest(Guid ProdutoId, int Quantidade);

public sealed record AlterarStatusPedidoRequest(PedidoStatus NovoStatus, string? Motivo);

public sealed record PedidoItemResponse(
    Guid Id,
    Guid ProdutoId,
    string ProdutoNome,
    int Quantidade,
    decimal PrecoUnitario,
    decimal ValorTotalItem)
{
    public static PedidoItemResponse FromEntity(PedidoItem item) => new(
        item.Id,
        item.ProdutoId,
        item.Produto?.Nome ?? string.Empty,
        item.Quantidade,
        item.PrecoUnitario,
        item.ValorTotalItem);
}

public sealed record PedidoHistoricoResponse(
    Guid Id,
    PedidoStatus? StatusAnterior,
    PedidoStatus NovoStatus,
    DateTime AlteradoEm,
    string? Motivo)
{
    public static PedidoHistoricoResponse FromEntity(PedidoStatusHistorico historico) => new(
        historico.Id,
        historico.StatusAnterior,
        historico.NovoStatus,
        DateTimeHelper.ToSaoPaulo(historico.AlteradoEmUtc),
        historico.Motivo);
}

public sealed record PedidoResponse(
    Guid Id,
    Guid ClienteId,
    string ClienteNome,
    DateTime CriadoEm,
    DateTime AtualizadoEm,
    PedidoStatus Status,
    decimal ValorTotal,
    IReadOnlyList<PedidoItemResponse> Itens,
    IReadOnlyList<PedidoHistoricoResponse> HistoricoStatus)
{
    public static PedidoResponse FromEntity(Pedido pedido) => new(
        pedido.Id,
        pedido.ClienteId,
        pedido.Cliente?.Nome ?? string.Empty,
        DateTimeHelper.ToSaoPaulo(pedido.CriadoEmUtc),
        DateTimeHelper.ToSaoPaulo(pedido.AtualizadoEmUtc),
        pedido.Status,
        pedido.ValorTotal,
        pedido.Itens.Select(PedidoItemResponse.FromEntity).ToList(),
        pedido.HistoricoStatus.OrderBy(x => x.AlteradoEmUtc).Select(PedidoHistoricoResponse.FromEntity).ToList());
}
