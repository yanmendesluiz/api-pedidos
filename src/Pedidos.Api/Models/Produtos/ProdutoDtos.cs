using System.ComponentModel.DataAnnotations;
using Pedidos.Api.Domain;
using Pedidos.Api.Utils;

namespace Pedidos.Api.Models.Produtos;

public sealed record CriarProdutoRequest(
    [Required, MaxLength(160)] string Nome,
    [MaxLength(500)] string? Descricao,
    decimal Preco,
    int EstoqueDisponivel);

public sealed record AtualizarProdutoRequest(
    [Required, MaxLength(160)] string Nome,
    [MaxLength(500)] string? Descricao,
    decimal Preco);

public sealed record AtualizarEstoqueRequest(int EstoqueDisponivel);
public sealed record AtualizarPrecoRequest(decimal Preco);
public sealed record AlterarStatusProdutoRequest(bool Ativo);

public sealed record ProdutoResponse(
    Guid Id,
    string Nome,
    string? Descricao,
    decimal Preco,
    int EstoqueDisponivel,
    bool Ativo,
    DateTime CriadoEm,
    DateTime AtualizadoEm)
{
    public static ProdutoResponse FromEntity(Produto produto) => new(
        produto.Id,
        produto.Nome,
        produto.Descricao,
        produto.Preco,
        produto.EstoqueDisponivel,
        produto.Ativo,
        DateTimeHelper.ToSaoPaulo(produto.CriadoEmUtc),
        DateTimeHelper.ToSaoPaulo(produto.AtualizadoEmUtc));
}
