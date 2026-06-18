namespace Pedidos.Api.Domain;

public sealed class PedidoItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PedidoId { get; set; }
    public Pedido Pedido { get; set; } = default!;
    public Guid ProdutoId { get; set; }
    public Produto Produto { get; set; } = default!;
    public int Quantidade { get; set; }
    public decimal PrecoUnitario { get; set; }
    public decimal ValorTotalItem { get; set; }
}
