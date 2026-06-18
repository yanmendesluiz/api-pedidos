using Pedidos.Api.Domain;

namespace Pedidos.Api.Tests;

public static class Fixtures
{
    public static Cliente ClienteAtivo() => new()
    {
        Nome = "Cliente Teste",
        Email = $"cliente-{Guid.NewGuid():N}@email.com",
        Documento = Guid.NewGuid().ToString("N")[..14],
        Ativo = true,
        CriadoEmUtc = DateTime.UtcNow,
        AtualizadoEmUtc = DateTime.UtcNow
    };

    public static Produto ProdutoAtivo(decimal preco, int estoque) => new()
    {
        Nome = $"Produto {Guid.NewGuid():N}",
        Descricao = "Produto para teste",
        Preco = preco,
        EstoqueDisponivel = estoque,
        Ativo = true,
        CriadoEmUtc = DateTime.UtcNow,
        AtualizadoEmUtc = DateTime.UtcNow
    };
}
