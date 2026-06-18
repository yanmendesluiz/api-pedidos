namespace Pedidos.Api.Domain;

public sealed class Pedido
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ClienteId { get; set; }
    public Cliente Cliente { get; set; } = default!;
    public DateTime CriadoEmUtc { get; set; } = DateTime.UtcNow;
    public DateTime AtualizadoEmUtc { get; set; } = DateTime.UtcNow;
    public PedidoStatus Status { get; set; } = PedidoStatus.Criado;
    public decimal ValorTotal { get; set; }
    public ICollection<PedidoItem> Itens { get; set; } = new List<PedidoItem>();
    public ICollection<PedidoStatusHistorico> HistoricoStatus { get; set; } = new List<PedidoStatusHistorico>();
}
