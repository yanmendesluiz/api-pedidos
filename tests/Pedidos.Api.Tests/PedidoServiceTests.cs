using Pedidos.Api.Domain;
using Pedidos.Api.Models.Pedidos;
using Pedidos.Api.Services;
using Xunit;

namespace Pedidos.Api.Tests;

public sealed class PedidoServiceTests
{
    [Fact]
    public async Task CriarPedido_DeveDebitarEstoqueECalcularTotais()
    {
        using var fixture = new TestDb();
        var cliente = Fixtures.ClienteAtivo();
        var produto = Fixtures.ProdutoAtivo(preco: 10.50m, estoque: 5);
        fixture.Db.Clientes.Add(cliente);
        fixture.Db.Produtos.Add(produto);
        await fixture.Db.SaveChangesAsync();
        var service = new PedidoService(fixture.Db);

        var result = await service.CriarAsync(new CriarPedidoRequest(cliente.Id, [new CriarPedidoItemRequest(produto.Id, 2)]), CancellationToken.None);

        Assert.True(result.Success, result.Error);
        Assert.Equal(21.00m, result.Data!.ValorTotal);
        Assert.Equal(3, fixture.Db.Produtos.Single().EstoqueDisponivel);
        Assert.Single(result.Data.HistoricoStatus);
        Assert.Equal(PedidoStatus.Criado, result.Data.Status);
    }

    [Fact]
    public async Task CriarPedido_ComEstoqueInsuficiente_NaoDeveDebitarEstoque()
    {
        using var fixture = new TestDb();
        var cliente = Fixtures.ClienteAtivo();
        var produto = Fixtures.ProdutoAtivo(preco: 10m, estoque: 1);
        fixture.Db.Clientes.Add(cliente);
        fixture.Db.Produtos.Add(produto);
        await fixture.Db.SaveChangesAsync();
        var service = new PedidoService(fixture.Db);

        var result = await service.CriarAsync(new CriarPedidoRequest(cliente.Id, [new CriarPedidoItemRequest(produto.Id, 2)]), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal(1, fixture.Db.Produtos.Single().EstoqueDisponivel);
        Assert.Empty(fixture.Db.Pedidos);
    }

    [Fact]
    public async Task CriarPedido_DevePreservarPrecoHistoricoMesmoComAlteracaoDoProduto()
    {
        using var fixture = new TestDb();
        var cliente = Fixtures.ClienteAtivo();
        var produto = Fixtures.ProdutoAtivo(preco: 99.90m, estoque: 2);
        fixture.Db.Clientes.Add(cliente);
        fixture.Db.Produtos.Add(produto);
        await fixture.Db.SaveChangesAsync();
        var service = new PedidoService(fixture.Db);

        var result = await service.CriarAsync(new CriarPedidoRequest(cliente.Id, [new CriarPedidoItemRequest(produto.Id, 1)]), CancellationToken.None);
        produto.Preco = 120m;
        await fixture.Db.SaveChangesAsync();

        Assert.True(result.Success, result.Error);
        Assert.Equal(99.90m, result.Data!.Itens.Single().PrecoUnitario);
    }

    [Fact]
    public async Task CancelarPedidoCriado_DeveRetornarEstoqueEGerarHistorico()
    {
        using var fixture = new TestDb();
        var cliente = Fixtures.ClienteAtivo();
        var produto = Fixtures.ProdutoAtivo(preco: 50m, estoque: 3);
        fixture.Db.Clientes.Add(cliente);
        fixture.Db.Produtos.Add(produto);
        await fixture.Db.SaveChangesAsync();
        var service = new PedidoService(fixture.Db);
        var pedido = await service.CriarAsync(new CriarPedidoRequest(cliente.Id, [new CriarPedidoItemRequest(produto.Id, 2)]), CancellationToken.None);

        var cancelado = await service.AlterarStatusAsync(pedido.Data!.Id, new AlterarStatusPedidoRequest(PedidoStatus.Cancelado, "Cliente desistiu"), CancellationToken.None);

        Assert.True(cancelado.Success, cancelado.Error);
        Assert.Equal(PedidoStatus.Cancelado, cancelado.Data!.Status);
        Assert.Equal(3, fixture.Db.Produtos.Single().EstoqueDisponivel);
        Assert.Equal(2, cancelado.Data.HistoricoStatus.Count);
    }

    [Fact]
    public async Task PedidoEnviado_NaoPodeSerCancelado()
    {
        using var fixture = new TestDb();
        var cliente = Fixtures.ClienteAtivo();
        var produto = Fixtures.ProdutoAtivo(preco: 50m, estoque: 3);
        fixture.Db.Clientes.Add(cliente);
        fixture.Db.Produtos.Add(produto);
        await fixture.Db.SaveChangesAsync();
        var service = new PedidoService(fixture.Db);
        var pedido = await service.CriarAsync(new CriarPedidoRequest(cliente.Id, [new CriarPedidoItemRequest(produto.Id, 1)]), CancellationToken.None);
        await service.AlterarStatusAsync(pedido.Data!.Id, new AlterarStatusPedidoRequest(PedidoStatus.Pago, null), CancellationToken.None);
        await service.AlterarStatusAsync(pedido.Data.Id, new AlterarStatusPedidoRequest(PedidoStatus.Enviado, null), CancellationToken.None);

        var cancelado = await service.AlterarStatusAsync(pedido.Data.Id, new AlterarStatusPedidoRequest(PedidoStatus.Cancelado, null), CancellationToken.None);

        Assert.False(cancelado.Success);
        Assert.Contains("inválida", cancelado.Error, StringComparison.OrdinalIgnoreCase);
    }
}
