using Microsoft.EntityFrameworkCore;
using Pedidos.Api.Data;
using Pedidos.Api.Domain;
using Pedidos.Api.Models;
using Pedidos.Api.Models.Clientes;
using Pedidos.Api.Utils;

namespace Pedidos.Api.Services;

public sealed class ClienteService(AppDbContext db)
{
    public async Task<ServiceResult<ClienteResponse>> CriarAsync(CriarClienteRequest request, CancellationToken ct)
    {
        var nome = request.Nome?.Trim() ?? string.Empty;
        var email = request.Email?.Trim().ToLowerInvariant() ?? string.Empty;
        var documento = request.Documento?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(nome)) return ServiceResult<ClienteResponse>.Fail("O nome do cliente é obrigatório.");
        if (string.IsNullOrWhiteSpace(email) || !new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(email)) return ServiceResult<ClienteResponse>.Fail("O e-mail é obrigatório e deve possuir formato válido.");
        if (string.IsNullOrWhiteSpace(documento)) return ServiceResult<ClienteResponse>.Fail("O documento é obrigatório.");
        if (!DocumentValidator.IsValidWhenCpfOrCnpj(documento)) return ServiceResult<ClienteResponse>.Fail("Documento inválido para CPF/CNPJ.");

        if (await db.Clientes.AnyAsync(x => x.Ativo && x.Email == email, ct)) return ServiceResult<ClienteResponse>.Fail("Já existe cliente ativo com este e-mail.");
        if (await db.Clientes.AnyAsync(x => x.Ativo && x.Documento == documento, ct)) return ServiceResult<ClienteResponse>.Fail("Já existe cliente ativo com este documento.");

        var now = DateTimeHelper.NowUtc();
        var cliente = new Cliente { Nome = nome, Email = email, Documento = documento, Ativo = true, CriadoEmUtc = now, AtualizadoEmUtc = now };
        db.Clientes.Add(cliente);
        await db.SaveChangesAsync(ct);
        return ServiceResult<ClienteResponse>.Ok(ClienteResponse.FromEntity(cliente));
    }

    public async Task<ClienteResponse?> ObterPorIdAsync(Guid id, CancellationToken ct)
    {
        var cliente = await db.Clientes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return cliente is null ? null : ClienteResponse.FromEntity(cliente);
    }

    public async Task<PagedResult<ClienteResponse>> ListarAsync(int page, int pageSize, CancellationToken ct)
    {
        page = Pagination.NormalizePage(page);
        pageSize = Pagination.NormalizePageSize(pageSize);
        var query = db.Clientes.AsNoTracking().OrderBy(x => x.Nome);
        var total = await query.CountAsync(ct);
        var clientes = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        var items = clientes.Select(ClienteResponse.FromEntity).ToList();
        return new PagedResult<ClienteResponse>(items, page, pageSize, total);
    }

    public async Task<ServiceResult<ClienteResponse>> AlterarStatusAsync(Guid id, AlterarStatusClienteRequest request, CancellationToken ct)
    {
        var cliente = await db.Clientes.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (cliente is null) return ServiceResult<ClienteResponse>.Fail("Cliente não encontrado.");
        cliente.Ativo = request.Ativo;
        cliente.AtualizadoEmUtc = DateTimeHelper.NowUtc();
        await db.SaveChangesAsync(ct);
        return ServiceResult<ClienteResponse>.Ok(ClienteResponse.FromEntity(cliente));
    }
}
