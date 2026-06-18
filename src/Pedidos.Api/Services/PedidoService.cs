using Microsoft.EntityFrameworkCore;
using Pedidos.Api.Data;
using Pedidos.Api.Domain;
using Pedidos.Api.Models;
using Pedidos.Api.Models.Pedidos;
using Pedidos.Api.Utils;

namespace Pedidos.Api.Services;

public sealed class PedidoService(AppDbContext db)
{
    public async Task<ServiceResult<PedidoResponse>> CriarAsync(CriarPedidoRequest request, CancellationToken ct)
    {
        if (request.Itens is null || request.Itens.Count == 0) return ServiceResult<PedidoResponse>.Fail("O pedido deve possuir ao menos um item.");
        if (request.Itens.Any(x => x.Quantidade <= 0)) return ServiceResult<PedidoResponse>.Fail("A quantidade de cada item deve ser maior que zero.");

        await using var transaction = await db.Database.BeginTransactionAsync(ct);

        var cliente = await db.Clientes.FirstOrDefaultAsync(x => x.Id == request.ClienteId, ct);
        if (cliente is null) return ServiceResult<PedidoResponse>.Fail("Cliente não encontrado.");
        if (!cliente.Ativo) return ServiceResult<PedidoResponse>.Fail("Clientes inativos não podem criar pedidos.");

        var produtoIds = request.Itens.Select(x => x.ProdutoId).Distinct().ToList();
        var produtos = await db.Produtos.Where(x => produtoIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, ct);
        var now = DateTimeHelper.NowUtc();
        var pedido = new Pedido
        {
            ClienteId = cliente.Id,
            Cliente = cliente,
            Status = PedidoStatus.Criado,
            CriadoEmUtc = now,
            AtualizadoEmUtc = now
        };

        foreach (var itemRequest in request.Itens)
        {
            if (!produtos.TryGetValue(itemRequest.ProdutoId, out var produto)) return ServiceResult<PedidoResponse>.Fail($"Produto {itemRequest.ProdutoId} não encontrado.");
            if (!produto.Ativo) return ServiceResult<PedidoResponse>.Fail($"Produto {produto.Nome} está inativo.");
            if (produto.EstoqueDisponivel < itemRequest.Quantidade) return ServiceResult<PedidoResponse>.Fail($"Produto {produto.Nome} não possui estoque suficiente.");

            produto.EstoqueDisponivel -= itemRequest.Quantidade;
            produto.AtualizadoEmUtc = now;

            var preco = decimal.Round(produto.Preco, 2, MidpointRounding.ToEven);
            var valorItem = decimal.Round(preco * itemRequest.Quantidade, 2, MidpointRounding.ToEven);
            pedido.Itens.Add(new PedidoItem
            {
                ProdutoId = produto.Id,
                Produto = produto,
                Quantidade = itemRequest.Quantidade,
                PrecoUnitario = preco,
                ValorTotalItem = valorItem
            });
        }

        pedido.ValorTotal = decimal.Round(pedido.Itens.Sum(x => x.ValorTotalItem), 2, MidpointRounding.ToEven);
        pedido.HistoricoStatus.Add(new PedidoStatusHistorico
        {
            StatusAnterior = null,
            NovoStatus = PedidoStatus.Criado,
            AlteradoEmUtc = now,
            Motivo = "Criação do pedido"
        });

        db.Pedidos.Add(pedido);
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        return ServiceResult<PedidoResponse>.Ok(await ObterPedidoResponseAsync(pedido.Id, ct) ?? PedidoResponse.FromEntity(pedido));
    }

    public async Task<PedidoResponse?> ObterPorIdAsync(Guid id, CancellationToken ct) => await ObterPedidoResponseAsync(id, ct);

    public async Task<PagedResult<PedidoResponse>> ListarAsync(int page, int pageSize, CancellationToken ct)
    {
        page = Pagination.NormalizePage(page);
        pageSize = Pagination.NormalizePageSize(pageSize);
        var query = db.Pedidos
            .AsNoTracking()
            .Include(x => x.Cliente)
            .Include(x => x.Itens).ThenInclude(x => x.Produto)
            .Include(x => x.HistoricoStatus)
            .OrderByDescending(x => x.CriadoEmUtc);
        var total = await query.CountAsync(ct);
        var pedidos = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return new PagedResult<PedidoResponse>(pedidos.Select(PedidoResponse.FromEntity).ToList(), page, pageSize, total);
    }

    public async Task<ServiceResult<PedidoResponse>> AlterarStatusAsync(Guid id, AlterarStatusPedidoRequest request, CancellationToken ct)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        var pedido = await db.Pedidos.Include(x => x.Itens).ThenInclude(x => x.Produto).Include(x => x.HistoricoStatus).FirstOrDefaultAsync(x => x.Id == id, ct);
        if (pedido is null) return ServiceResult<PedidoResponse>.Fail("Pedido não encontrado.");
        if (pedido.Status == request.NovoStatus) return ServiceResult<PedidoResponse>.Fail("O novo status é igual ao status atual. Nenhum histórico foi gerado.");
        if (!IsTransicaoPermitida(pedido.Status, request.NovoStatus)) return ServiceResult<PedidoResponse>.Fail($"Transição de status inválida: {pedido.Status} -> {request.NovoStatus}.");

        var now = DateTimeHelper.NowUtc();
        var statusAnterior = pedido.Status;

        if (request.NovoStatus == PedidoStatus.Cancelado && statusAnterior != PedidoStatus.Enviado)
        {
            foreach (var item in pedido.Itens)
            {
                item.Produto.EstoqueDisponivel += item.Quantidade;
                item.Produto.AtualizadoEmUtc = now;
            }
        }

        pedido.Status = request.NovoStatus;
        pedido.AtualizadoEmUtc = now;
        pedido.HistoricoStatus.Add(new PedidoStatusHistorico
        {
            StatusAnterior = statusAnterior,
            NovoStatus = request.NovoStatus,
            AlteradoEmUtc = now,
            Motivo = request.Motivo
        });

        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        return ServiceResult<PedidoResponse>.Ok(await ObterPedidoResponseAsync(id, ct) ?? PedidoResponse.FromEntity(pedido));
    }

    private static bool IsTransicaoPermitida(PedidoStatus atual, PedidoStatus novo) => (atual, novo) switch
    {
        (PedidoStatus.Criado, PedidoStatus.Pago) => true,
        (PedidoStatus.Pago, PedidoStatus.Enviado) => true,
        (PedidoStatus.Criado, PedidoStatus.Cancelado) => true,
        (PedidoStatus.Pago, PedidoStatus.Cancelado) => true,
        _ => false
    };

    private async Task<PedidoResponse?> ObterPedidoResponseAsync(Guid id, CancellationToken ct)
    {
        var pedido = await db.Pedidos.AsNoTracking()
            .Include(x => x.Cliente)
            .Include(x => x.Itens).ThenInclude(x => x.Produto)
            .Include(x => x.HistoricoStatus)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        return pedido is null ? null : PedidoResponse.FromEntity(pedido);
    }
}
