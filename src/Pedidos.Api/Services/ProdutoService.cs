using Microsoft.EntityFrameworkCore;
using Pedidos.Api.Data;
using Pedidos.Api.Domain;
using Pedidos.Api.Models;
using Pedidos.Api.Models.Produtos;
using Pedidos.Api.Utils;

namespace Pedidos.Api.Services;

public sealed class ProdutoService(AppDbContext db)
{
    public async Task<ServiceResult<ProdutoResponse>> CriarAsync(CriarProdutoRequest request, CancellationToken ct)
    {
        var nome = request.Nome?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(nome)) return ServiceResult<ProdutoResponse>.Fail("O nome do produto é obrigatório.");
        if (request.Preco <= 0) return ServiceResult<ProdutoResponse>.Fail("O preço deve ser maior que zero.");
        if (request.EstoqueDisponivel < 0) return ServiceResult<ProdutoResponse>.Fail("O estoque não pode ser negativo.");

        var now = DateTimeHelper.NowUtc();
        var produto = new Produto
        {
            Nome = nome,
            Descricao = request.Descricao?.Trim(),
            Preco = decimal.Round(request.Preco, 2, MidpointRounding.ToEven),
            EstoqueDisponivel = request.EstoqueDisponivel,
            Ativo = true,
            CriadoEmUtc = now,
            AtualizadoEmUtc = now
        };
        db.Produtos.Add(produto);
        await db.SaveChangesAsync(ct);
        return ServiceResult<ProdutoResponse>.Ok(ProdutoResponse.FromEntity(produto));
    }

    public async Task<ProdutoResponse?> ObterPorIdAsync(Guid id, CancellationToken ct)
    {
        var produto = await db.Produtos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return produto is null ? null : ProdutoResponse.FromEntity(produto);
    }

    public async Task<PagedResult<ProdutoResponse>> ListarAsync(int page, int pageSize, CancellationToken ct)
    {
        page = Pagination.NormalizePage(page);
        pageSize = Pagination.NormalizePageSize(pageSize);
        var query = db.Produtos.AsNoTracking().OrderBy(x => x.Nome);
        var total = await query.CountAsync(ct);
        var produtos = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        var items = produtos.Select(ProdutoResponse.FromEntity).ToList();
        return new PagedResult<ProdutoResponse>(items, page, pageSize, total);
    }

    public async Task<ServiceResult<ProdutoResponse>> AtualizarAsync(Guid id, AtualizarProdutoRequest request, CancellationToken ct)
    {
        var produto = await db.Produtos.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (produto is null) return ServiceResult<ProdutoResponse>.Fail("Produto não encontrado.");
        if (string.IsNullOrWhiteSpace(request.Nome)) return ServiceResult<ProdutoResponse>.Fail("O nome do produto é obrigatório.");
        if (request.Preco <= 0) return ServiceResult<ProdutoResponse>.Fail("O preço deve ser maior que zero.");

        produto.Nome = request.Nome.Trim();
        produto.Descricao = request.Descricao?.Trim();
        produto.Preco = decimal.Round(request.Preco, 2, MidpointRounding.ToEven);
        produto.AtualizadoEmUtc = DateTimeHelper.NowUtc();
        await db.SaveChangesAsync(ct);
        return ServiceResult<ProdutoResponse>.Ok(ProdutoResponse.FromEntity(produto));
    }

    public async Task<ServiceResult<ProdutoResponse>> AtualizarEstoqueAsync(Guid id, AtualizarEstoqueRequest request, CancellationToken ct)
    {
        if (request.EstoqueDisponivel < 0) return ServiceResult<ProdutoResponse>.Fail("O estoque não pode ser negativo.");
        var produto = await db.Produtos.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (produto is null) return ServiceResult<ProdutoResponse>.Fail("Produto não encontrado.");
        produto.EstoqueDisponivel = request.EstoqueDisponivel;
        produto.AtualizadoEmUtc = DateTimeHelper.NowUtc();
        await db.SaveChangesAsync(ct);
        return ServiceResult<ProdutoResponse>.Ok(ProdutoResponse.FromEntity(produto));
    }

    public async Task<ServiceResult<ProdutoResponse>> AtualizarPrecoAsync(Guid id, AtualizarPrecoRequest request, CancellationToken ct)
    {
        if (request.Preco <= 0) return ServiceResult<ProdutoResponse>.Fail("O preço deve ser maior que zero.");
        var produto = await db.Produtos.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (produto is null) return ServiceResult<ProdutoResponse>.Fail("Produto não encontrado.");
        produto.Preco = decimal.Round(request.Preco, 2, MidpointRounding.ToEven);
        produto.AtualizadoEmUtc = DateTimeHelper.NowUtc();
        await db.SaveChangesAsync(ct);
        return ServiceResult<ProdutoResponse>.Ok(ProdutoResponse.FromEntity(produto));
    }

    public async Task<ServiceResult<ProdutoResponse>> AlterarStatusAsync(Guid id, AlterarStatusProdutoRequest request, CancellationToken ct)
    {
        var produto = await db.Produtos.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (produto is null) return ServiceResult<ProdutoResponse>.Fail("Produto não encontrado.");
        produto.Ativo = request.Ativo;
        produto.AtualizadoEmUtc = DateTimeHelper.NowUtc();
        await db.SaveChangesAsync(ct);
        return ServiceResult<ProdutoResponse>.Ok(ProdutoResponse.FromEntity(produto));
    }
}
