using Pedidos.Api.Models.Clientes;
using Pedidos.Api.Models.Produtos;
using Pedidos.Api.Services;
using Xunit;

namespace Pedidos.Api.Tests;

public sealed class ClienteProdutoServiceTests
{
    [Fact]
    public async Task NaoDeveCadastrarDoisClientesAtivosComMesmoEmail()
    {
        using var fixture = new TestDb();
        var service = new ClienteService(fixture.Db);

        var primeiro = await service.CriarAsync(new CriarClienteRequest("Maria", "maria@email.com", "39053344705"), CancellationToken.None);
        var segundo = await service.CriarAsync(new CriarClienteRequest("Outra", "maria@email.com", "987654321"), CancellationToken.None);

        Assert.True(primeiro.Success, primeiro.Error);
        Assert.False(segundo.Success);
    }

    [Fact]
    public async Task ProdutoNaoPodeTerPrecoZeroOuEstoqueNegativo()
    {
        using var fixture = new TestDb();
        var service = new ProdutoService(fixture.Db);

        var precoZero = await service.CriarAsync(new CriarProdutoRequest("Produto", null, 0m, 1), CancellationToken.None);
        var estoqueNegativo = await service.CriarAsync(new CriarProdutoRequest("Produto", null, 1m, -1), CancellationToken.None);

        Assert.False(precoZero.Success);
        Assert.False(estoqueNegativo.Success);
    }
}
