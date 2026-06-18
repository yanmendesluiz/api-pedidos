namespace Pedidos.Api.Domain;

public sealed class PedidoStatusHistorico
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PedidoId { get; set; }
    public Pedido Pedido { get; set; } = default!;
    public PedidoStatus? StatusAnterior { get; set; }
    public PedidoStatus NovoStatus { get; set; }
    public DateTime AlteradoEmUtc { get; set; } = DateTime.UtcNow;
    public string? Motivo { get; set; }
}
