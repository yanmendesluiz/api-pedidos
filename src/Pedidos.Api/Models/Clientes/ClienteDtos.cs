using System.ComponentModel.DataAnnotations;
using Pedidos.Api.Domain;
using Pedidos.Api.Utils;

namespace Pedidos.Api.Models.Clientes;

public sealed record CriarClienteRequest(
    [Required, MaxLength(160)] string Nome,
    [Required, EmailAddress, MaxLength(220)] string Email,
    [Required, MaxLength(32)] string Documento);

public sealed record AlterarStatusClienteRequest(bool Ativo);

public sealed record ClienteResponse(
    Guid Id,
    string Nome,
    string Email,
    string Documento,
    bool Ativo,
    DateTime CriadoEm,
    DateTime AtualizadoEm)
{
    public static ClienteResponse FromEntity(Cliente cliente) => new(
        cliente.Id,
        cliente.Nome,
        cliente.Email,
        cliente.Documento,
        cliente.Ativo,
        DateTimeHelper.ToSaoPaulo(cliente.CriadoEmUtc),
        DateTimeHelper.ToSaoPaulo(cliente.AtualizadoEmUtc));
}
